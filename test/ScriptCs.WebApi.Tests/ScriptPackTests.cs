using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.WebApi.Tests
{
    public class ScriptPackTests
    {
        public class TheGetContextMethod
        {
            [Theory, ScriptCsAutoData]
            public void ReturnsANewWebApiInstance(ScriptPack scriptPack)
            {
                ((IScriptPack)scriptPack).GetContext().ShouldBeType<WebApi>();
            }
        }

        public class TheInitializeMethod
        {
            [Theory, ScriptCsAutoData]
            public void AddReferencesToTheSession([Frozen] Mock<IScriptPackSession> session, ScriptPack scriptPack)
            {
                ((IScriptPack)scriptPack).Initialize(session.Object);
                session.Verify(s=>s.AddReference("System.Net.Http"));
            }

            [Theory, ScriptCsAutoData]
            public void ImportsNamespacesToTheSession([Frozen] Mock<IScriptPackSession> session, ScriptPack scriptPack)
            {
                ((IScriptPack)scriptPack).Initialize(session.Object);
                session.Verify(s => s.ImportNamespace("System.Net.Http"));
                session.Verify(s => s.ImportNamespace("System.Net.Http.Headers"));
                session.Verify(s => s.ImportNamespace("System.Web.Http"));
                session.Verify(s => s.ImportNamespace("System.Web.Http.Routing"));
                session.Verify(s => s.ImportNamespace("Owin"));
            }
        }
    }
}
