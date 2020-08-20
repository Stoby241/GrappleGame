﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void ServerConnectionReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void TransformUpdate(int fromClient, Packet packet)
    {
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        Server.clients[fromClient].player.SetTransform(position, rotation);
    }
}
