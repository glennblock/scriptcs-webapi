using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using ScriptCs;
using ScriptCs.Contracts;

namespace ScriptCs.WebApi
{
    public class ScriptPack : IScriptPack
    {
        internal readonly ILog _logger;
        internal readonly IControllerTypeManager _typeManager;

        [ImportingConstructor]
        public ScriptPack(ILog logger, IControllerTypeManager typeManager)
        {
            Guard.AgainstNullArgument("logger", logger);
            Guard.AgainstNullArgument("typeManager", typeManager);

            _logger = logger;
            _typeManager = typeManager;
        }

        IScriptPackContext IScriptPack.GetContext()
        {
            return new WebApi(_logger, _typeManager);
        }

        void IScriptPack.Initialize(IScriptPackSession session)
        {
            Guard.AgainstNullArgument("session", session);

            session.AddReference("System.Net.Http");
            var namespaces = new[]
                {
                    "System.Net.Http",
                    "System.Net.Http.Headers",
                    "System.Web.Http",
                    "System.Web.Http.Routing",
                    "Owin"
                }.ToList();

            namespaces.ForEach(session.ImportNamespace);
        }

        void IScriptPack.Terminate()
        {
        }
    }
}
