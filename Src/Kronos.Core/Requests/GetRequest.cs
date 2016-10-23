﻿using Kronos.Core.Serialization;
using Kronos.Core.StatusCodes;
using Kronos.Core.Storage;
using ProtoBuf;
using XGain.Sockets;

namespace Kronos.Core.Requests
{
    [ProtoContract]
    public class GetRequest : Request
    {
        public override RequestType RequestType { get; set; } = RequestType.Get;

        [ProtoMember(1)]
        public string Key { get; set; }

        // used by reflection
        public GetRequest()
        {
        }

        public GetRequest(string key)
        {
            Key = key;
        }

        public override void ProcessAndSendResponse(ISocket socket, IStorage storage)
        {
            byte[] requestedObject = storage.TryGet(Key) ?? SerializationUtils.Serialize(RequestStatusCode.NotFound);
            socket.Send(SerializationUtils.SerializeToStreamWithLength(requestedObject));
        }

        protected override T PrepareResponse<T>(byte[] responseBytes)
        {
            if (responseBytes.Length == 2) // if size is equal to serialized RequestStatusCode
            {
                try
                {
                    // if server returned NotFound status code, return null
                    RequestStatusCode notFound = SerializationUtils.Deserialize<RequestStatusCode>(responseBytes);
                    if (notFound == RequestStatusCode.NotFound)
                    {
                        responseBytes = SerializationUtils.SerializeToStreamWithLength(new byte[] {0});
                    }
                }
                catch
                {
                }
            }

            return SerializationUtils.DeserializeWithLength<T>(responseBytes);
        }
    }
}
