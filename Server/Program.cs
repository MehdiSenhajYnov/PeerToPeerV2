using System;
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
            Console.WriteLine("Starting Server On Port 8888 ...");
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);
            server.Start(8888);

            listener.ConnectionRequestEvent += request =>
            {
                if(server.ConnectedPeersCount < 10)
                {
                    request.Accept();
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
                peer.Send(writer, DeliveryMethod.ReliableOrdered);          // Send with reliability
                if (clients.Count > 1)
                {
                    NetDataWriter writer2 = new NetDataWriter();                 // Create writer class
                    writer2.Put("ConnectTo:" + clients[1].EndPoint.Address.ToString() + ":" + clients[1].EndPoint.Port.ToString() + ":" + clients[0].EndPoint.Port.ToString());                                // Put some string
                    clients[0].Send(writer2, DeliveryMethod.ReliableOrdered);          // Send with reliability
                    writer2.Reset();
                    writer2.Put("ConnectTo:" + clients[0].EndPoint.Address.ToString() + ":" + clients[0].EndPoint.Port.ToString() + ":" + clients[1].EndPoint.Port.ToString());                                // Put some string
                    clients[1].Send(writer2, DeliveryMethod.ReliableOrdered);          // Send
                }
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