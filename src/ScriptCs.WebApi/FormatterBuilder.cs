using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptCs.WebApi
{
    public class FormatterBuilder
    {
        internal Func<Type, bool> _canReadType = t => true;
        internal Func<Type, bool> _canWriteType = t => true;
        internal Func<ReadFromStreamArgs, Task<object>> _readFromStream;
        internal Func<WriteToStreamArgs, Task> _writeToStream;
        internal readonly IList<MediaTypeMapping> _mappings;
        internal readonly IList<MediaTypeHeaderValue> _supportedMediaTypes;
        internal readonly IList<Encoding> _supportedEncodings;

        public FormatterBuilder()
        {
            _mappings = new List<MediaTypeMapping>();
            _supportedMediaTypes = new List<MediaTypeHeaderValue>();
            _supportedEncodings = new List<Encoding>();
        }

        public FormatterBuilder CanReadType(Func<Type, bool> condition)
        {
            Guard.AgainstNullArgument("condition", condition);

            _canReadType = condition;
            return this;
        }

        public FormatterBuilder CanWriteType(Func<Type, bool> condition)
        {
            Guard.AgainstNullArgument("condition", condition);

            _canWriteType = condition;
            return this;
        }

        public FormatterBuilder ReadFromStream(
            Func<ReadFromStreamArgs, Task<object>> readFromStream)
        {
            Guard.AgainstNullArgument("readFromStream", readFromStream);

            _readFromStream = readFromStream;
            return this;
        }

        public FormatterBuilder WriteToStream(
            Func<WriteToStreamArgs, Task> writeToStream)
        {
            Guard.AgainstNullArgument("writeToStream", writeToStream);

            _writeToStream = writeToStream;
            return this;
        }

        public FormatterBuilder SupportMediaType(MediaTypeHeaderValue mediaType)
        {
            Guard.AgainstNullArgument("mediaType", mediaType);

            _supportedMediaTypes.Add(mediaType);
            return this;
        }

        public FormatterBuilder SupportMediaType(string mediaType)
        {
            Guard.AgainstNullArgument("mediaType", mediaType);

           _supportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            return this;
        }

        public FormatterBuilder SupportEncoding(Encoding encoding)
        {
            Guard.AgainstNullArgument("encoding", encoding);

            _supportedEncodings.Add(encoding);
            return this;
        }

        public FormatterBuilder MapQueryString(
            string parameterName, 
            string parameterValue,
            MediaTypeHeaderValue mediaType)
        {
            Guard.AgainstNullArgument("parameterName", parameterName);
            Guard.AgainstNullArgument("parameterValue", parameterName);
            Guard.AgainstNullArgument("mediaType", mediaType);


            _mappings.Add(new QueryStringMapping(parameterName, parameterValue, mediaType));
            return this;
        }

        public FormatterBuilder MapQueryString(
            string parameterName,
            string parameterValue,
            string mediaType)
        {
            Guard.AgainstNullArgument("parameterName", parameterName);
            Guard.AgainstNullArgument("parameterValue", parameterName);
            Guard.AgainstNullArgument("mediaType", mediaType);

            _mappings.Add(new QueryStringMapping(parameterName, parameterValue, mediaType));
            return this;
        }

        public FormatterBuilder MapRequestHeader(
            string headerName, 
            string headerValue, 
            System.StringComparison valueComparison, 
            bool isValueSubstring, 
            string mediaType)
        {
            Guard.AgainstNullArgument("headerName", headerName);
            Guard.AgainstNullArgument("headerValue", headerValue);
            Guard.AgainstNullArgument("mediaType", mediaType);

            _mappings.Add(new RequestHeaderMapping(headerName, headerValue, valueComparison, isValueSubstring, mediaType));
            return this;
        }

        public FormatterBuilder MapRequestHeader(
            string headerName,
            string headerValue,
            System.StringComparison valueComparison,
            bool isValueSubstring,
            MediaTypeHeaderValue mediaType
        )
        {
            Guard.AgainstNullArgument("headerName", headerName);
            Guard.AgainstNullArgument("headerValue", headerValue);
            Guard.AgainstNullArgument("mediaType", mediaType);

            _mappings.Add(new RequestHeaderMapping(headerName, headerValue, valueComparison, isValueSubstring, mediaType));
            return this;
        }

        public FormatterBuilder MapUriExtension(
            string extension,
            string mediaType
        )
        {
            Guard.AgainstNullArgument("extension", extension);
            Guard.AgainstNullArgument("mediaType", mediaType);

            _mappings.Add(new UriPathExtensionMapping(extension, mediaType));
            return this;
        }

        public FormatterBuilder MapUriExtension(
            string extension,
            MediaTypeHeaderValue mediaType
            )
        {
            Guard.AgainstNullArgument("extension", extension);
            Guard.AgainstNullArgument("mediaType", mediaType);
            
            _mappings.Add(new UriPathExtensionMapping(extension, mediaType));
            return this;
        }

        public MediaTypeFormatter Build()
        {
            var formatter = new Formatter(_canReadType, _canWriteType, _readFromStream, _writeToStream);

            foreach (var mediaType in _supportedMediaTypes)
            {
                formatter.SupportedMediaTypes.Add(mediaType);
            }

            foreach (var mapping in _mappings)
            {
                formatter.MediaTypeMappings.Add(mapping);
            }

            foreach (var encoding in _supportedEncodings)
            {
                formatter.SupportedEncodings.Add(encoding);
            }

            return formatter;
        }
    }
}