﻿using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

namespace MyNetworkingServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            List<NetPeer> clients = new List<NetPeer>();
            Console.WriteLine("Starting ...");
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);
            server.Start(9050);

            listener.ConnectionRequestEvent += request =>
            {
                if(server.ConnectedPeersCount < 10)
                {
                    request.AcceptIfKey("SomeConnectionKey");
                }   
                else
                {
                    request.Reject();
                }
            };



            listener.PeerConnectedEvent += peer =>
            {
                clients.Add(peer);
                Console.WriteLine("We got connection: {0}", peer.EndPoint); // Show peer ip
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put("Hello client!");                                // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                Console.WriteLine("We got: {0}", dataReader.GetString(100));
                dataReader.Recycle();
            };

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    server.PollEvents();
                    Thread.Sleep(15);
                }
            }).Start();
            
            string? input = Console.ReadLine();
            while (input != "quit")
            {
                if (clients == null || clients.Count <= 0) 
                {
                    Console.WriteLine("Waiting for clients...");
                    Thread.Sleep(500);
                    continue;
                }
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put(input);                                // Put some string
                foreach (NetPeer clientPeer in clients)
                {
                    clientPeer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
                }
                Thread.Sleep(15);
                input = Console.ReadLine();
            }
            server.Stop();
        }
    }
}