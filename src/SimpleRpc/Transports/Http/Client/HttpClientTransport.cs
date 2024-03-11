using SimpleRpc.Serialization;
using SimpleRpc.Transports.Abstractions.Client;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Transports.Http.Client
{
    public class HttpClientTransport<TService> : BaseClientTransport<TService>
        where TService : class
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMessageSerializer _serializer;
        private  readonly string _clientName;

        public HttpClientTransport(string clientName, IMessageSerializer serializer, IHttpClientFactory httpClientFactory)
        {
            _clientName = clientName;
            _httpClientFactory = httpClientFactory;
            _serializer = serializer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object HandleSync(RpcRequest rpcRequest) => SendRequest<object>(rpcRequest).ConfigureAwait(false).GetAwaiter().GetResult();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task HandleAsync(RpcRequest rpcRequest) => SendRequest<object>(rpcRequest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task<T> HandleAsyncWithResult<T>(RpcRequest rpcRequest) => SendRequest<T>(rpcRequest);

        private async Task<T> SendRequest<T>(RpcRequest rpcRequest)
        {
            using (var httpClient = _httpClientFactory.CreateClient(_clientName))
            {
                var streamContent = new SerializableContent(_serializer, rpcRequest);
                using (var httpResponseMessage = await httpClient.PostAsync(string.Empty, streamContent, CancellationToken.None).ConfigureAwait(false))
                {
                    httpResponseMessage.EnsureSuccessStatusCode();

                    var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    string responseMediaType = httpResponseMessage.Content.Headers.ContentType.MediaType;

                    if (_serializer.ContentType == responseMediaType)
                    {
                        var rpcResponse = await _serializer.Deserialize<RpcResponse>(stream);

                        if (rpcResponse.Error != null)
                        {
                            throw new RpcException(rpcResponse.Error);
                        }

                        return _serializer.UnpackResult<T>(rpcRequest, rpcResponse);
                    }
                    else if (responseMediaType == System.Net.Mime.MediaTypeNames.Application.Octet 
                        &&  (typeof(T) == typeof(object) || typeof(Stream).IsAssignableFrom(typeof(T)))
                        )
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        return (T)(object)memoryStream;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unexpected response type: {responseMediaType}");
                    }
                }
            }
        }
    }
}
