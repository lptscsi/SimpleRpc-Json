using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleRpc.Serialization;
using SimpleRpc.Transports.Abstractions.Client;
using SimpleRpc.Transports.Abstractions.Server;
using SimpleRpc.Transports.Http.Client;
using System;
using System.Net.Http;

namespace SimpleRpc.Transports
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleRpcClient<TService>(
            this IServiceCollection services,
            string clientName,
            HttpClientTransportOptions<TService> options)
            where TService : class
        {
            if (string.IsNullOrEmpty(clientName))
            {
                throw new ArgumentNullException(nameof(clientName));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.AddHttpClient(clientName, (client) => {
                var url = new Uri(options.Url);
                client.BaseAddress = url;

                if (options.DefaultRequestHeaders != null)
                {
                    foreach (var header in options.DefaultRequestHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                client.DefaultRequestHeaders.Add(Constants.Other.ApplicationName, options.ApplicationName);
                client.DefaultRequestHeaders.ConnectionClose = false;
                client.DefaultRequestHeaders.Host = url.Host;
            });

            services.TryAddSingleton<IClientConfigurationManager, ClientConfigurationManager>();
            services.AddSingleton(typeof(ClientConfiguration), (sp)=>
            {
                IMessageSerializer serializer = SerializationHelper.Json;
                return new ClientConfiguration()
                {
                    Name = clientName,
                    Transport = new HttpClientTransport<TService>(clientName, serializer, sp.GetRequiredService<IHttpClientFactory>())
                };
            });

            return services;
        }

        public static IServiceCollection AddSimpleRpcProxy<T>(this IServiceCollection services, string clientName)
            where T : class
        {
            if (string.IsNullOrEmpty(clientName))
            {
                throw new ArgumentNullException(nameof(clientName));
            }

            services.TryAddSingleton<T>(sp => {
                BaseClientTransport clientTransport = sp.GetService<IClientConfigurationManager>().Get(clientName);
                return RpcProxy.Create<T>((BaseClientTransport<T>)clientTransport);
             });

            return services;
        }

        public static IServiceCollection AddSimpleRpcServer<T>(
            this IServiceCollection services,
            IServerTransportOptions<T> serverTransportOptions) 
            where T : class, IServerTransport, new()
        {
            if (serverTransportOptions == null)
            {
                throw new ArgumentNullException(nameof(serverTransportOptions));
            }

            var serverTransport = new T();

            services.AddSingleton<IServerTransport>(serverTransport);
            serverTransport.ConfigureServices(services, serverTransportOptions);

            return services;
        }
    }
}
