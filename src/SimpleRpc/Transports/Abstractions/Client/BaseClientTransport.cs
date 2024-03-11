using Fasterflect;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleRpc.Transports.Abstractions.Client
{
    public abstract class BaseClientTransport : IClientTransport
    {
        private ConcurrentDictionary<MethodInfo, MethodModelCache> _metadata = new ConcurrentDictionary<MethodInfo, MethodModelCache>();

        public abstract object HandleSync(RpcRequest rpcRequest);

        public abstract Task HandleAsync(RpcRequest rpcRequest);

        public abstract Task<T> HandleAsyncWithResult<T>(RpcRequest rpcRequest);

        public object Invoke(MethodInfo targetMethod, object?[]? args)
        {
            MethodModelCache methodModel = _metadata.GetOrAdd(targetMethod, (key) =>
            {
                return new MethodModelCache(key);
            });

            var rpcRequest = new RpcRequest
            { 
                Method = methodModel.Model,
                Parameters = args
            };

            if (methodModel.IsAsync)
            {
                //Task<T>
                if (methodModel.ReturnType != typeof(void))
                {
                    return this.CallMethod(
                      new[] { methodModel.ReturnType },
                      nameof(HandleAsyncWithResult),
                      rpcRequest);
                }
                else
                {
                    //Task
                    return HandleAsync(rpcRequest);
                }
            }
            else
            {
                return HandleSync(rpcRequest);
            }
        }
    }

    public abstract class BaseClientTransport<TService> : BaseClientTransport
    {
     
    }
}
