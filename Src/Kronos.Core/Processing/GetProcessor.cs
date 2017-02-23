﻿using Kronos.Core.Networking;
using Kronos.Core.Serialization;
using Kronos.Core.Storage;

namespace Kronos.Core.Processing
{
    public class GetProcessor : CommandProcessor<GetRequest, byte[]>
    {
        public override byte[] Process(GetRequest request, IStorage storage)
        {
            byte[] response;
            if (!storage.TryGet(request.Key, out response))
            {
                response = SerializationUtils.Serialize(RequestStatusCode.NotFound);
            }

            return Reply(response);
        }
    }
}
