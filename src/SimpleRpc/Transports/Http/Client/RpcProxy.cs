using SimpleRpc.Transports.Abstractions.Client;
using System;
using System.Reflection;

namespace SimpleRpc.Transports.Http.Client
{
    public class RpcProxy : DispatchProxy
    {
        private BaseClientTransport? _transport;

        public RpcProxy()
        { }


        public static T Create<T>(BaseClientTransport<T> transport)
            where T : class
        {
            if (transport == null)
                throw new ArgumentNullException(nameof(transport));

            object proxy = Create<T, RpcProxy>();
            var routableProxy = (RpcProxy)proxy;

            routableProxy._transport = transport;

            return (T)proxy;
        }

        /// <inheritdoc/>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (_transport == null)
                throw new InvalidOperationException("Proxy transport is NULL");

            if (targetMethod == null)
            {
                return null;
            }

            return _transport.Invoke(targetMethod, args);
        }
    }
}
