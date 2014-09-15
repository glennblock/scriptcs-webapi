using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;

namespace ScriptCs.WebApi
{
    [Export(typeof(IControllerTypeManager))]
    public class ControllerTypeManager : IControllerTypeManager
    {
        internal readonly ILog _logger;
        private const string RoslynAssemblyNameCharacter = "ℛ";

        internal static readonly string[] IgnoredAssemblyPrefixes = new[]
            {
                "Autofac,",
                "Autofac.",
                "Common.Logging",
                "log4net,",
                "Nancy,",
                "Nancy.",
                "NuGet.",
                "PowerArgs,",
                "Roslyn.",
                "scriptcs,",
                "ScriptCs.",
                "ServiceStack.",
            };


        [ImportingConstructor]
        public ControllerTypeManager(ILog logger)
        {
            Guard.AgainstNullArgument("logger", logger);

            _logger = logger;
        }

        public IEnumerable<Type> GetLoadedTypes()
        {
            var types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a=>!(a.FullName.StartsWith("System") || a.FullName.StartsWith("mscorlib")));

            foreach (Assembly assembly in assemblies.Where(a => !IgnoredAssemblyPrefixes.Any(p => a.GetName().Name.StartsWith(p))))
            {
                try
                {
                    types.AddRange(assembly.GetTypes());
                    _logger.Debug("Scriptcs.WebApi - Loaded assembly " + assembly);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    _logger.Warn(string.Format("ScriptCs.WebApi - Count not load types from {0}", assembly.FullName));
                    foreach (var load in ex.LoaderExceptions)
                    {
                        _logger.Warn(string.Format("ScriptCs.WebApi - Exception {0}", load));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(string.Format("ScriptCs.WebApi - Count not load types from {0}: {1}", assembly.FullName, ex.Message));
                }

            }
            return types;
        }

        public IEnumerable<Type> GetControllerTypes()
        {
            var types = GetLoadedTypes();
            var tempControllerTypes = ControllerResolver.WhereControllerType(types).ToList();
            var controllerTypes = tempControllerTypes.Where(t => !t.IsAbstract && t.Assembly.FullName.Substring(0, 1) != RoslynAssemblyNameCharacter).ToList();
            var roslynTypes = tempControllerTypes.Where(t => t.Assembly.FullName.StartsWith(RoslynAssemblyNameCharacter)).ToList();

            if (roslynTypes.Any())
            {
                controllerTypes.Add(roslynTypes.Last());
            }
            
            foreach (var controller in controllerTypes)
            {
                _logger.Debug(string.Format("ScriptCs.WebApi - Found controller: {0}", controller.FullName));
            }
            return controllerTypes;
        }

        public IEnumerable<Type> GetControllerTypes(Assembly[] assemblies)
        {
            Guard.AgainstNullArgument("assemblies", assemblies);

            IEnumerable<Assembly> controllerAssemblies =
                new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies()).Union(assemblies);

            Type[] types = controllerAssemblies.SelectMany(a => a.GetTypes()).ToArray();
            List<Type> controllerTypes = ControllerResolver.WhereControllerType(types).ToList();

            foreach (var controller in controllerTypes)
            {
                _logger.Debug(string.Format("ScriptCs.WebApi - Found controller: {0}", controller.FullName));
            }

            return controllerTypes;
        }


    }
}
