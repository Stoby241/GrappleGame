﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SharedFiles.Utility;
using UnityEngine;

public static class Server
{
    public static int MaxPlayers { get; private set; }
    private const int clientSocketsOverlap = 10;
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int fromClient, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static string ip;

    /// <summary>Starts the server.</summary>
    /// <param name="maxPlayers">The maximum players that can be connected simultaneously.</param>
    /// <param name="port">The port to start the server on.</param>
    public static void Start(int maxPlayers, int port)
    {
        ip = IPManager.GetIPAddress();
        
        Debug.Log(ip);
        
        MaxPlayers = maxPlayers;
        Port = port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UdpReceiveCallback, null);

        Debug.Log($"Server started on port {Port}.");
    }

    /// <summary>Handles new TCP connections.</summary>
    private static void TcpConnectCallback(IAsyncResult result)
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
        Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers + clientSocketsOverlap; i++)
        {
            if (clients[i].tcp.socket != null) continue;
            clients[i].tcp.Connect(client);
            return;
        }

        Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    /// <summary>Receives incoming UDP data.</summary>
    private static void UdpReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
            udpListener.BeginReceive(UdpReceiveCallback, null);

            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt();

                if (clientId == 0)
                {
                    return;
                }

                if (clients[clientId].udp.endPoint == null)
                {
                    // If this is a new connection
                    clients[clientId].udp.Connect(clientEndPoint);
                    return;
                }

                if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                {
                    // Ensures that the client is not being impersonated by another by sending a false clientID
                    clients[clientId].udp.HandleData(packet);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving UDP data: {ex}");
        }
    }

    /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
    /// <param name="clientEndPoint">The endpoint to send the packet to.</param>
    /// <param name="packet">The packet to send.</param>
    public static void SendUdpData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint != null)
            {
                udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending data to {clientEndPoint} via UDP: {ex}");
        }
    }

    /// <summary>Initializes all necessary server data.</summary>
    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers + clientSocketsOverlap; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.gameEnterRequest, ServerHandle.GameEnterReqest },
            { (int)ClientPackets.trooperTransformUpdate, ServerHandle.TrooperTransformUpdate },
            { (int)ClientPackets.trooperGrappleUpdate, ServerHandle.TrooperGrappleUpdate }
        };
        Debug.Log("Initialized packets.");
    }
    
    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
