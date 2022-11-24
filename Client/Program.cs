﻿using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace MyNetworking
{
    class Program
    {
        public static void Main(string[] args)
        {
            NetPeer? ServerPeer = null;
            Console.WriteLine("Starting ...");
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager client = new NetManager(listener);
            client.Start();
            client.Connect("20.13.17.73", 8888, "");

            listener.ConnectionRequestEvent += request =>
            {
                if(client.ConnectedPeersCount < 10)
                    request.AcceptIfKey("SomeConnectionKey");
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("We got connection: {0}", peer.EndPoint); // Show peer ip
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put("Hello client!");                                // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                if (ServerPeer == null) { ServerPeer = fromPeer ;}
                Console.WriteLine("We got: {0}", dataReader.GetString(100));
                dataReader.Recycle();
            };
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    client.PollEvents();
                    Thread.Sleep(15);
                }
            }).Start();
            
            if (ServerPeer == null) 
            {
                Console.WriteLine("Waiting for server...");
            }
            while (ServerPeer == null)
            {
                Thread.Sleep(500);
            }

            Console.WriteLine("Connected to server!");

            string? input = Console.ReadLine();
            while (input != "quit")
            {
                
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put(input);                                // Put some string
                ServerPeer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
                Thread.Sleep(15);
                input = Console.ReadLine();
            }

            client.Stop();
        }
    }
}