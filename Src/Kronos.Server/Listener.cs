﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kronos.Core.Configuration;
using Kronos.Core.Messages;
using Kronos.Core.Networking;
using Kronos.Core.Processing;
using NLog;

namespace Kronos.Server
{
    public class Listener : IListener
    {
        private readonly TcpListener _listener;
        private readonly ISocketProcessor _processor;
        private readonly IRequestProcessor _requestProcessor;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Auth _auth;

        public Listener(SettingsArgs settings, ISocketProcessor processor, IRequestProcessor requestProcessor)
        {
            _auth = Auth.FromCfg(new AuthConfig { Login = settings.Login, Password = settings.Password });
            _listener = new TcpListener(IPAddress.Any, settings.Port);
            _processor = processor;
            _requestProcessor = requestProcessor;

            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
        }

        public void Start()
        {
            Logger.Info("Starting server");
            _listener.Start();
            string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            Logger.Info($"Server started on {_listener.LocalEndpoint}. Kronos version {version}");

            CancellationToken token = _cancel.Token;

            Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    Socket socket = null;
                    try
                    {
                        socket = await _listener.AcceptSocketAsync().ConfigureAwait(false);
                        SocketUtils.Prepare(socket);
                        ProcessSocketConnection(socket);
                    }
                    catch (ObjectDisposedException)
                    {
                        Logger.Info("TCP listener is disposed");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Exception during accepting new request {ex}");
                    }
                    finally
                    {
                        socket?.Shutdown(SocketShutdown.Send);
                    }
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void Stop()
        {
            Logger.Info("Stopping server");
            _cancel.Cancel();

            if (_listener.Server.Connected)
            {
                Logger.Info("Server is connected, shutting down");
                try
                {
                    _listener.Server.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException ex)
                {
                    Logger.Error($"Error on shutting down server socket {ex}");
                }
            }
            
            _listener.Stop();
            _listener.Server.Dispose();

            Logger.Info("Server is down");
        }

        public void Dispose()
        {
            Logger.Info("Stopping TCP/IP server");
            Stop();
        }

        private void ProcessSocketConnection(Socket client)
        {
            try
            {
                Request request = _processor.ReceiveRequest(client);

                Logger.Debug($"Processing new request {request.Type}");
                Response response = _requestProcessor.Handle(request, _auth);

                _processor.SendResponse(client, response);
                Logger.Debug("Processing finished");
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception on processing: {ex}");
            }
        }
    }
}
