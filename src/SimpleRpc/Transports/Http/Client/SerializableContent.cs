using SimpleRpc.Serialization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleRpc.Transports.Http.Client
{
    internal class SerializableContent : HttpContent
    {
        private readonly IMessageSerializer _serializer;
        private readonly RpcRequest _request;

        public SerializableContent(IMessageSerializer serializer, RpcRequest request)
        {
            _serializer = serializer;
            _request = request;
            Headers.ContentType = new MediaTypeHeaderValue(_serializer.ContentType);
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            MemoryStream memoryStream = new MemoryStream();
            await _serializer.Serialize(_request, memoryStream);
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
