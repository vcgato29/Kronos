﻿using System;
using Kronos.Core.Serialization;

namespace Kronos.Core.Requests
{
    public class RequestMapper : IRequestMapper
    {
        public Request ProcessRequest(byte[] requestBytes, RequestType type)
        {
            Request request;
            switch (type)
            {
                case RequestType.Insert:
                    request = SerializationUtils.Deserialize<InsertRequest>(requestBytes);
                    break;
                case RequestType.Get:
                    request = SerializationUtils.Deserialize<GetRequest>(requestBytes);
                    break;
                case RequestType.Delete:
                    request = SerializationUtils.Deserialize<DeleteRequest>(requestBytes);
                    break;
                case RequestType.Count:
                    request = SerializationUtils.Deserialize<CountRequest>(requestBytes);
                    break;
                case RequestType.Contains:
                    request = SerializationUtils.Deserialize<ContainsRequest>(requestBytes);
                    break;
                default:
                    throw new InvalidOperationException($"Cannot find processor for type {type}");
            }

            return request;
        }
    }
}
