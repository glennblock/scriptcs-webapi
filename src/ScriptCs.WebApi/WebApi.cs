﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Common.Logging;
using Microsoft.Owin.Builder;
using Microsoft.SqlServer.Server;
using Owin;
using ScriptCs.Contracts;

using Microsoft.Owin.Hosting;
using System;
using System.Net.Http;

namespace ScriptCs.WebApi
{
    /// <summary>
    /// Creates a web api using an OWIN host
    /// </summary>
    public class WebApi : IScriptPackContext
    {
        private readonly ILog _logger;
        private readonly IControllerTypeManager _typeManager;
        private Action<IAppBuilder> _startupAction;
        private HttpConfiguration _config;
        private Type[] _controllerTypes;
        private bool _useJsonOnly;
        private MediaTypeFormatter _theFormatter;

        public WebApi(ILog logger, IControllerTypeManager typeManager)
        {
            Guard.AgainstNullArgument("logger", logger);
            Guard.AgainstNullArgument("typeManager", typeManager);

            _logger = logger;
            _typeManager = typeManager;
        }

        public WebApi Configure(params Type[] controllerTypes)
        {
            return Configure((Action<IAppBuilder>) null, controllerTypes);
        }

        public WebApi Configure(params Assembly[] controllerAssemblies)
        {
            return Configure(null, controllerAssemblies);
        }

        public WebApi Configure(Action<IAppBuilder> startupAction, params Type[] controllerTypes)
        {
            _startupAction = startupAction;
            return Configure(startupAction, new HttpConfiguration(), controllerTypes);
        }

        public WebApi Configure(Action<IAppBuilder> startupAction, params Assembly[] controllerAssemblies)
        {
            _startupAction = startupAction;
            return Configure(startupAction, new HttpConfiguration(), controllerAssemblies);
        }

        public WebApi Configure(Action<IAppBuilder> startupAction, HttpConfiguration config, params Type[] controllerTypes)
        {
            Guard.AgainstNullArgument("config", config);

            _startupAction = startupAction;
            if (controllerTypes.Length == 0)
            {
                controllerTypes = _typeManager.GetControllerTypes().ToArray();
            }

            _controllerTypes = controllerTypes;
            _config = config;
            ApplyDefaultConfiguration(_config, _controllerTypes);
            return this;
        }

        public WebApi Configure(Action<IAppBuilder> startupAction, HttpConfiguration config, params Assembly[] controllerAssemblies)
        {
            _startupAction = startupAction;
            return Configure(startupAction, config, _typeManager.GetControllerTypes(controllerAssemblies).ToArray());
        }

        public WebApi Configure(HttpConfiguration config, params Type[] controllerTypes)
        {
            return Configure(null, config, controllerTypes);
        }

        public WebApi UseJsonOnly()
        {
            _useJsonOnly = true;
            _theFormatter = null;
            return this;
        }

        public WebApi UseFormatterOnly(MediaTypeFormatter formatter)
        {
            Guard.AgainstNullArgument("formatter", formatter);

            _theFormatter = formatter;
            _useJsonOnly = false;
            return this;
        }

        public IDisposable Start(string baseAddress)
        {
            if (_config == null)
            {
                _config = new HttpConfiguration();
                ApplyDefaultConfiguration(_config, _controllerTypes);
            }

            return WebApp.Start(baseAddress, appBuilder =>
            {
                appBuilder.UseWebApi(_config);
                if (_startupAction != null)
                {
                    _startupAction(appBuilder);
                }
            });
        }

        public FormatterBuilder NewFormatter()
        {
            return new FormatterBuilder();
        }

        private void ApplyDefaultConfiguration(HttpConfiguration config,
                                                      IEnumerable<Type> controllerTypes)
        {
            Guard.AgainstNullArgument("controllerTypes", controllerTypes);
            Guard.AgainstNullArgument("config", config);

            if (!ControllerResolver.AllAssignableToIHttpController(controllerTypes))
            {
                throw new ArgumentException("controllerTypes", "Does not contain any controllers");
            }

            config.Services.Replace(typeof (IHttpControllerTypeResolver), new ControllerResolver(controllerTypes.ToList()));

            config.Routes.Clear();
            config.Routes.MapHttpRoute(name: "DefaultApi",
                                       routeTemplate: "api/{controller}/{id}",
                                       defaults: new {id = RouteParameter.Optional}
            );

            if (_useJsonOnly)
            {
                var json = config.Formatters.JsonFormatter;
                config.Formatters.Clear();
                config.Formatters.Add(json);
            }
            else if (_theFormatter != null)
            {
                config.Formatters.Clear();
                config.Formatters.Add(_theFormatter);
            }
        }
    }
}