using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Logging;
using Moq;
using Ploeh.AutoFixture.Xunit;
using Should;
using TestControllers;
using Xunit.Extensions;

namespace ScriptCs.WebApi.Tests
{
    public class ControllerTypeManagerTests
    {
        public class TheConstructor
        {
            [Theory, ScriptCsAutoData]
            public void SetsTheLogger(ControllerTypeManager manager)
            {
                manager._logger.ShouldNotBeNull();
            }
        }

        public class TheGetLoadedTypesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ReturnsAllAssembliesThatHaveNotBeenFiltered(ControllerTypeManager manager)
            {
                var assemblies =
                    manager.GetLoadedTypes()
                        .Where(
                            t =>
                                ControllerTypeManager.IgnoredAssemblyPrefixes.Any(
                                    p => t.Assembly.GetName().Name.StartsWith(p)));
                assemblies.ShouldBeEmpty();
            }
        }

        public class TheGetControllerTypesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ReturnsInMemoryControllersWhenUsingTheParameterlessOverload(ControllerTypeManager manager)
            {
                var results = manager.GetControllerTypes();
                results.ShouldContain(typeof(TestController));
            }
            
            [Theory, ScriptCsAutoData]
            public void LogsAllDiscoveredControllers([Frozen] Mock<ILog> mockLogger, ControllerTypeManager manager)
            {
                var results = manager.GetControllerTypes();
                mockLogger.Verify(l => l.Debug(It.Is<String>(s => s == "ScriptCs.WebApi - Found controller: TestControllers.TestController")));
            }

            [Theory, ScriptCsAutoData]
            public void DoesNotReturnControllersInAnAssemblyWithAScriptCsPrefix(ControllerTypeManager manager)
            {
                var results = manager.GetControllerTypes();
                results.ShouldNotContain(typeof(TestInMemoryController));
            }

            [Theory, ScriptCsAutoData]
            public void ReturnsAllControllersFromPassedInAssembliesWhenUsingTheAssembliesOverload(ControllerTypeManager manager)
            {
                var assemblies = new Assembly[] {typeof (TestInMemoryController).Assembly};
                var results = manager.GetControllerTypes(assemblies);
                results.ShouldContain(typeof(TestInMemoryController));
            }
        }

        public class TestInMemoryController : ApiController
        {
            
        }
    }
}
