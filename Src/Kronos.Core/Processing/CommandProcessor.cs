﻿using System.Net.Sockets;
using System.Threading.Tasks;
using Kronos.Core.Networking;
using Kronos.Core.Requests;
using Kronos.Core.Serialization;
using Kronos.Core.Storage;

namespace Kronos.Core.Processing
{
    public abstract class CommandProcessor<TRequest, TResponse> where TRequest : IRequest
    {
        public abstract RequestType Type { get; }

        public abstract void Handle(ref TRequest request, IStorage storage, Socket client);

        public async Task<TResponse> ExecuteAsync(TRequest request, IConnection service)
        {
            byte[] response = await Task.Run(() => service.Send(request)).ConfigureAwait(false);

            TResponse results = PrepareResponse<TResponse>(response);

            return results;
        }

        public async Task<TResponse[]> ExecuteAsync(TRequest request, IConnection[] services)
        {
            int count = services.Length;
            Task<TResponse>[] responses = new Task<TResponse>[count];
            for (int i = 0; i < count; i++)
            {
                IConnection connection = services[i];
                responses[i] = ExecuteAsync(request, connection);
            }

            return await Task.WhenAll(responses);
        }

        protected void Reply(TResponse response, Socket client)
        {
            byte[] data = SerializationUtils.SerializeToStreamWithLength(response);

            SocketUtils.SendAll(client, data);
        }

        protected virtual T PrepareResponse<T>(byte[] responseBytes)
        {
            T results = SerializationUtils.Deserialize<T>(responseBytes);
            return results;
        }
    }
}