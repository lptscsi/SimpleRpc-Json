using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SimpleRpc.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SimpleRpc.Transports.Http.Server
{
    internal class HttpTransportMidleware<TService>
        where TService : class
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpTransportMidleware<TService>> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpServerTransportOptions<TService> _httpServerTransportOptions;
        private readonly RpcServer<TService> _rpcServer;

        public HttpTransportMidleware(
            RequestDelegate next, 
            ILogger<HttpTransportMidleware<TService>> logger, 
            IServiceProvider serviceProvider, 
            RpcServer<TService> rpcServer,
            HttpServerTransportOptions<TService> httpServerTransportOptions)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _rpcServer = rpcServer;
            _httpServerTransportOptions = httpServerTransportOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path != _httpServerTransportOptions.Path)
            {
                await _next(context);
            }
            else
            {
                var rpcError = (RpcError)null;
                var serializer = SerializationHelper.Json;
          
                if (rpcError == null)
                {
                    try
                    {
                        RpcRequest rpcRequest = await serializer.Deserialize<RpcRequest>(context.Request.Body);
                        object rpcResponse = await _rpcServer.Invoke(rpcRequest);

                        if (rpcResponse is RpcResponse)
                        {
                            context.Response.ContentType = serializer.ContentType;
                            MemoryStream memoryStream = new MemoryStream();
                            await serializer.Serialize(rpcResponse, memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            await memoryStream.CopyToAsync(context.Response.Body);
                        }
                        else if (rpcResponse is Stream stream)
                        {
                            try
                            {
                                context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
                                await stream.CopyToAsync(context.Response.Body);
                            }
                            finally
                            {
                                stream.Dispose();
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unexpected response type: { rpcResponse?.GetType().Name ?? "null" }");
                        }
                    }
                    catch (Exception e)
                    {
                        rpcError = new RpcError
                        {
                            Code = RpcErrorCode.IncorrectRequestBodyFormat,
                            Exception = e.Message,
                        };

                        _logger.LogError(e, rpcError.Code.ToString());
                
                        RpcResponse rpcResponse = new RpcResponse()
                        {
                            Error = rpcError,
                            Result  = null
                        };
                        await serializer.Serialize(rpcResponse, context.Response.Body);
                    }
                }
            }
        }
    }
}
