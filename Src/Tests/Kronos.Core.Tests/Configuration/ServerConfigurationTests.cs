﻿using System;
using Kronos.Core.Configuration;
using Kronos.Core.Model;
using Xunit;

namespace Kronos.Core.Tests.Configuration
{
    public class ServerConfigurationTests
    {
        [Fact]
        public void CanGetNodesConfiguration()
        {
            NodeConfiguration nodeConfiguraton = new NodeConfiguration("10.10.10.10", 80);
            ServerConfiguration configuration = new ServerConfiguration() { NodesConfiguration = new[] { nodeConfiguraton } };
            CachedObject cachedObject = new CachedObject("key", new byte[5], DateTime.MaxValue);

            NodeConfiguration nodeConfiguration = configuration.GetNodeForStream(cachedObject);

            Assert.NotNull(nodeConfiguration);
        }
    }
}