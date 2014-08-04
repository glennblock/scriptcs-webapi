using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.WebApi.Tests
{
    public class FormatterBuilderTests
    {
        public class TheConstructor
        {
            [Theory, ScriptCsAutoData]
            public void InitializesMappings(FormatterBuilder builder)
            {
                builder._mappings.ShouldNotBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void InitializesSupportedMediaTypes(FormatterBuilder builder)
            {
                builder._supportedMediaTypes.ShouldNotBeNull();
            }

            [Theory, ScriptCsAutoData]
            public void InitializesSupportedEncodings(FormatterBuilder builder)
            {
                builder._supportedEncodings.ShouldNotBeNull();
            }
        }

        public class TheCanReadTypeMethod
        {
            [Theory, ScriptCsAutoData]
            public void StoresTheCondition(FormatterBuilder builder)
            {
                Func<Type, bool> condition = b => true;
                builder.CanReadType(condition);
                builder._canReadType.ShouldEqual(condition);
            }
        }

        public class TheCanWriteTypeMethod
        {
            [Theory, ScriptCsAutoData]
            public void StoresTheCondition(FormatterBuilder builder)
            {
                Func<Type, bool> condition = b => true;
                builder.CanWriteType(condition);
                builder._canWriteType.ShouldEqual(condition);
            }
        }

        public class TheReadFromStreamMethod
        {
            [Theory, ScriptCsAutoData]
            public void StoresTheReadFromStreamFunc(FormatterBuilder builder)
            {
                Func<ReadFromStreamArgs, Task<object>> readFromStream = r => null;
                builder.ReadFromStream(readFromStream);
                builder._readFromStream.ShouldEqual(readFromStream);
            }
        }

        public class TheWriteToStreamMethod
        {
            [Theory, ScriptCsAutoData]
            public void StoresTheWriteToStreamFunc(FormatterBuilder builder)
            {
                Func<WriteToStreamArgs, Task> writeToStream = w => null;
                builder.WriteToStream(writeToStream);
                builder._writeToStream.ShouldEqual(writeToStream);
            }
        }

        public class TheSupportEncodingMethod
        {
            [Theory, ScriptCsAutoData]
            public void AddsTheEncoding(FormatterBuilder builder)
            {
                var encoding = new ASCIIEncoding();
                builder.SupportEncoding(encoding);
                builder._supportedEncodings.ShouldContain(encoding);
            }
        }

        public class TheSupportMediaTypeMethod
        {
            [Theory, ScriptCsAutoData]
            public void AddsTheMediaTypeWhenMediaTypeIsString(FormatterBuilder builder)
            {
                builder.SupportMediaType("application/foo");
                builder._supportedMediaTypes.Single(m => m.MediaType.Equals("application/foo"));
            }

            [Theory, ScriptCsAutoData]
            public void AddsTheMediaTypeWhenMediaTypeIsMediaTypeHeaderValue(FormatterBuilder builder)
            {
                builder.SupportMediaType(new MediaTypeHeaderValue("application/foo"));
                builder._supportedMediaTypes.Single(m => m.MediaType.Equals("application/foo"));
            }
        }

        public class TheMapQueryStringMethod
        {
            [Theory, ScriptCsAutoData]
            public void AddsTheMappingWhenMediaTypeIsString(FormatterBuilder builder)
            {
                builder.MapQueryString("param", "value", "application/foo");
                builder._mappings.Cast<QueryStringMapping>().Single(
                    m => m.QueryStringParameterName.Equals("param") &&
                         m.QueryStringParameterValue.Equals("value") &&
                         m.MediaType.MediaType.Equals("application/foo"));
            }

            [Theory, ScriptCsAutoData]
            public void AddsTheMappingWhenMediaTypeIsMediaTypeHeaderValue(FormatterBuilder builder)
            {
                builder.MapQueryString("param", "value", new MediaTypeHeaderValue("application/foo"));
                builder._mappings.Cast<QueryStringMapping>().Single(
                    m => m.QueryStringParameterName.Equals("param") &&
                         m.QueryStringParameterValue.Equals("value") &&
                         m.MediaType.MediaType.Equals("application/foo"));
            }
        }

        public class TheMapRequestHeaderMethod
        {
            [Theory, ScriptCsAutoData]
            public void AddsTheMappingWhenMediaTypeIsString(FormatterBuilder builder)
            {
                builder.MapRequestHeader("header", "value", StringComparison.CurrentCulture, false, "application/foo");
                builder._mappings.Cast<RequestHeaderMapping>().Single(
                    m => m.HeaderName.Equals("header") &&
                         m.HeaderValue.Equals("value") &&
                         m.HeaderValueComparison.Equals(StringComparison.CurrentCulture) &&
                         m.MediaType.MediaType.Equals("application/foo"));
            }

            [Theory, ScriptCsAutoData]
            public void AddsTheMappingWhenMediaTypeIsMediaTypeHeaderValue(FormatterBuilder builder)
            {
                builder.MapRequestHeader("header", "value", StringComparison.CurrentCulture, false, new MediaTypeHeaderValue("application/foo"));
                builder._mappings.Cast<RequestHeaderMapping>().Single(
                    m => m.HeaderName.Equals("header") &&
                         m.HeaderValue.Equals("value") &&
                         m.HeaderValueComparison.Equals(StringComparison.CurrentCulture) &&
                         m.MediaType.MediaType.Equals("application/foo"));
            }
        }

        public class TheMapUriExtensionMethod
        {
            [Theory, ScriptCsAutoData]
            public void AddsTheMappingWhenMediaTypeIsString(FormatterBuilder builder)
            {
                builder.MapUriExtension("foo", "application/foo");
                builder._mappings.Cast<UriPathExtensionMapping>().Single(
                    m => m.UriPathExtension.Equals("foo") &&
                        m.MediaType.MediaType.Equals("application/foo"));
            }

            [Theory, ScriptCsAutoData]
            public void AddsTheMappingWhenMediaTypeIsMediaTypeHeaderValue(FormatterBuilder builder)
            {
                builder.MapUriExtension("foo", new MediaTypeHeaderValue("application/foo"));
                builder._mappings.Cast<UriPathExtensionMapping>().Single(
                    m => m.UriPathExtension.Equals("foo") &&
                        m.MediaType.MediaType.Equals("application/foo"));
            }
        }

        public class TheBuildMethod
        {
            private static FormatterBuilder _builder;
            private static MediaTypeFormatter _formatter;

            static TheBuildMethod()
            {
                _builder = new FormatterBuilder();
                _builder._supportedMediaTypes.Add(new MediaTypeHeaderValue("application/foo"));
                _builder._supportedEncodings.Add(new ASCIIEncoding());
                _builder._mappings.Add(new UriPathExtensionMapping("foo", "application/foo"));
                _formatter = _builder.Build();
            }

            [Fact]
            public void CreatesAFormatter()
            {
                _formatter.ShouldNotBeNull();
            }

            [Fact]
            public void AddsSupportedMediaTypes()
            {
                _formatter.SupportedMediaTypes.Single(
                    m=>m.MediaType.Equals("application/foo"));
            }

            [Fact]
            public void AddsEncodings()
            {
                _formatter.SupportedEncodings.Cast<ASCIIEncoding>().Single();
            }

            [Fact]
            public void AddsMediaTypeMappings()
            {
                _formatter.MediaTypeMappings.Cast<UriPathExtensionMapping>().Single(
                    m => m.UriPathExtension.Equals("foo") &&
                         m.MediaType.MediaType.Equals("application/foo"));
            }
        }
    }
}
