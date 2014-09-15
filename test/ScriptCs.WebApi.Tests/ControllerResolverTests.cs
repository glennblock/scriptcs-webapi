using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Moq;
using Xunit;
using Xunit.Extensions;
using Should;

namespace ScriptCs.WebApi.Tests
{
    public class ControllerResolverTests
    {
        private static IList<Type> _controllers;

        static ControllerResolverTests()
        {
            _controllers = new List<Type>();
            _controllers.Add(new Mock<IHttpController>().Object.GetType());
            _controllers.Add(new Mock<IHttpController>().Object.GetType());
            _controllers.Add(typeof(string));
        }

        public class TheGetControllerTypesMethod
        {
            [Fact]
            public void ReturnsTheControllerTypesThatWerePassedToTheConstructor()
            {
                var resolver = new ControllerResolver(_controllers);
                var controllers = resolver.GetControllerTypes(null).ToList();
                controllers.ShouldContain(_controllers[0]);
                controllers.ShouldContain(_controllers[1]);
            }
        }

        public class TheWhereControllerTypeMethod
        {
            [Fact]
            public void ReturnsOnlyIHttpControllers()
            {
                var controllers = ControllerResolver.WhereControllerType(_controllers);
                controllers.Count().ShouldEqual(2);
                controllers.ShouldContain(_controllers[0]);
                controllers.ShouldContain(_controllers[1]);
            }
        }

    }
}
