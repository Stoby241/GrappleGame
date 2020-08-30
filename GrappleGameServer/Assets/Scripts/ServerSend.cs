﻿using System.Collections;
using System.Collections.Generic;
using SharedFiles.Utility;
using UnityEngine;

public static class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="toClient">The client to send the packet the packet to.</param>
    /// <param name="packet">The packet to send to the client.</param>
    private static void SendTcpData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="toClient">The client to send the packet the packet to.</param>
    /// <param name="packet">The packet to send to the client.</param>
    private static void SendUdpData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="packet">The packet to send.</param>
    private static void SendTcpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="exceptClient">The client to NOT send the data to.</param>
    /// <param name="packet">The packet to send.</param>
    private static void SendTcpDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="packet">The packet to send.</param>
    private static void SendUdpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="exceptClient">The client to NOT send the data to.</param>
    /// <param name="packet">The packet to send.</param>
    private static void SendUdpDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }

    #region Packets

    /// <summary>Sends a welcome message to the given client.</summary>
    public static void ServerConnection(int toClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.serverConnection))
        {
            packet.Write(toClient);

            SendTcpData(toClient, packet);
        }
    }
    public static void GameEnterRejected(int toClient, string message)
    {
        using (Packet packet = new Packet((int)ServerPackets.gameEnterRejected))
        {
            packet.Write(message);
            
            SendTcpData(toClient, packet);
        }
    }
    public static void LobbyChange(int toClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.lobbyChange))
        {
            packet.Write(ServerManager.instance.currentLobbyPreFabName);
            
            SendTcpData(toClient, packet);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="toClient">The client that should spawn the player.</param>
    /// <param name="player">The player to spawn.</param>
    public static void PlayerEnter(int toClient, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerEnter))
        {
            packet.Write(player.client.id);
            packet.Write(player.username);

            SendTcpData(player.client.id, packet);
        }
    }

    public static void PlayerLeave(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerLeave))
        {
            packet.Write(player.client.id);

            SendTcpDataToAll(player.client.id, packet);
        }
    }

    public static void ClientTransformUpdate(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.clientTransformUpdate))
        {
            packet.Write(player.client.id);
            packet.Write(player.trooper.transform.position);
            packet.Write(player.trooper.transform.rotation);
            packet.Write(player.trooper.velocity);
            packet.Write(player.trooper.grounded);

            SendUdpDataToAll(player.client.id, packet);
        }
    }
    
    public static void ClientGrappleUpdate(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.clientGrappleUpdate))
        {
            packet.Write(player.client.id);
            packet.Write(player.trooper.isGrappling);
            packet.Write(player.trooper.grapplePoint);
            packet.Write(player.trooper.distanceFromGrapple);
            
            SendUdpDataToAll(player.client.id, packet);
        }
    }
    #endregion
}
