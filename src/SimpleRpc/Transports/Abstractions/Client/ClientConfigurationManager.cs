﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleRpc.Transports.Abstractions.Client
{
    internal interface IClientConfigurationManager
    {
        BaseClientTransport Get(string clientName);
    }

    internal class ClientConfigurationManager : IClientConfigurationManager
    {
        private readonly ConcurrentDictionary<string, BaseClientTransport> _cache = new ConcurrentDictionary<string, BaseClientTransport>(StringComparer.OrdinalIgnoreCase);

        public ClientConfigurationManager(IEnumerable<ClientConfiguration> clientConfigurations)
        {
            foreach (var clientConfiguration in clientConfigurations)
            {
                if (!_cache.TryAdd(clientConfiguration.Name, clientConfiguration.Transport))
                {
                    throw new Exception($"Cant added client transport named {clientConfiguration.Name}, maybe it's already registered");
                }
            }
        }

        public BaseClientTransport Get(string clientName)
        {
            if (!_cache.TryGetValue(clientName, out var clientTransport))
            {
                throw new Exception($"Cant resolve client transport, make sure that you registered rpc client named {clientName}");
            }

            return clientTransport;
        }
    }
}
