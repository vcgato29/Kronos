﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Kronos.Core.Networking;
using Kronos.Core.Processing;
using Kronos.Core.Storage;
using Kronos.Server.Listening;
using NLog;
using NLog.Config;

namespace Kronos.Server
{
    public class Program
    {
        private static readonly ManualResetEventSlim _cancelEvent = new ManualResetEventSlim();

        public static void LoggerSetup(string nlogConfigPath)
        {
            var reader = XmlReader.Create(nlogConfigPath);
            var config = new XmlLoggingConfiguration(reader, null); //filename is not required.
            LogManager.Configuration = config;
        }

        public static void Main(string[] args)
        {
            int port = 5000;
            if (args.Length == 1)
            {
                int.TryParse(args[0], out port);
            }

            Task.WaitAll(StartAsync(port));
        }

        public static async Task StartAsync(int port, string nlogConfigPath = "NLog.config")
        {
            LoggerSetup(nlogConfigPath);
            PrintLogo();
            
            IPAddress localAddr = await EndpointUtils.GetIPAsync();

            ICleaner cleaner = new Cleaner();
            IStorage storage = new InMemoryStorage(cleaner);

            IRequestProcessor requestProcessor = new RequestProcessor(storage);
            IProcessor processor = new SocketProcessor();
            IListener server = new Listener(localAddr, port, processor, requestProcessor);

            server.Start();

            Console.CancelKeyPress += (sender, args) => _cancelEvent.Set();

            _cancelEvent.Wait();
            _cancelEvent.Reset();

            // dispose components
            storage.Dispose();
            server.Dispose();
        }

        private static void PrintLogo()
        {
            PrintLogoLine("");
            PrintLogoLine("  _  __  _____     ____    _   _    ____     _____ ");
            PrintLogoLine(@" | |/ / |  __ \   / __ \  | \ | |  / __ \   / ____|");
            PrintLogoLine(@" | ' /  | |__) | | |  | | |  \| | | |  | | | (___  ");
            PrintLogoLine(@" |  <   |  _  /  | |  | | | . ` | | |  | |  \___ \ ");
            PrintLogoLine(@" | . \  | | \ \  | |__| | | |\  | | |__| |  ____) |");
            PrintLogoLine(@" |_|\_\ |_|  \_\  \____/  |_| \_|  \____/  |_____/ ");
            PrintLogoLine("");
            PrintLogoLine("");
            PrintLogoLine("");
        }

        private static void PrintLogoLine(string line)
        {
            int centerPosition = (Console.WindowWidth - line.Length) / 2;
            if(centerPosition > 0) // if it's Docker console, it might be less than zero
            {
                Console.SetCursorPosition(centerPosition, Console.CursorTop);
            }

            Console.WriteLine(line);
        }

        public static void Stop()
        {
            _cancelEvent.Set();
        }
    }
}
