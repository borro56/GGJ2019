using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable 618

public class NetMessengerClient
{
    public NetworkConnection Connection { get; set; }
}

public abstract class NetMessenger : NetMessenger<NetMessengerClient> { }

public abstract class NetMessenger<T> : MonoBehaviour where T : NetMessengerClient, new()
{
    public bool IsClient { get; private set; }
    public bool IsServer { get; private set; }

    readonly List<T> connectionsData = new List<T>();

    public IReadOnlyList<T> ConnectionsData
    {
        get
        {
            for (var i = 0; i < connectionsData.Count; i++)
                if (!ClientsConnections.Contains(connectionsData[i].Connection))
                    connectionsData.RemoveAt(i--);

            for (var i = 0; i < ClientsConnections.Count; i++)
                GetConnectionData(ClientsConnections[i]);

            return connectionsData;
        }
    }
    
    protected abstract short OpCodeOffset { get; }

    public ReadOnlyCollection<NetworkConnection> ClientsConnections => NetworkServer.connections;
    public NetworkConnection ConnectionToServer => NetworkManager.singleton.client.connection;

    protected virtual short[] ClientMessagesToSend => null;
    protected virtual short[] ServerMessagesToSend => null;

    protected virtual void Update()
    {
        bool started = false;

        if (!IsClient && NetworkManager.singleton != null && NetworkManager.singleton.client != null &&
            NetworkManager.singleton.client.isConnected)
        {
            IsClient = true;
            OnStartClient();
            started = true;
        }

        if (!IsServer && NetworkServer.active)
        {
            IsServer = true;
            OnStartServer();
            started = true;
        }

        if (started) OnStartSync();
    }

    void OnEnable()
    {
        Update();
    }

    void OnMessageReceived(NetworkMessage msg)
    {
        if (msg.conn == ConnectionToServer)
        {
            OnMessageFromServer((short) (msg.msgType - 1 - MsgType.Highest - OpCodeOffset), msg);
        }
        else if (ClientsConnections.Contains(msg.conn))
        {
            var cnnData = GetConnectionData(msg.conn);
            OnMessageFromClient(cnnData, (short) (msg.msgType - 1 - MsgType.Highest - OpCodeOffset), msg);
        }
        else
        {
            throw new Exception("Unknown connection message");
        }
    }

    public void RegisterHandlersServer(params short[] msgIds)
    {
        foreach (var id in msgIds)
            RegisterHandlerServer(id);
    }

    public void RegisterHandlersClient(params short[] msgIds)
    {
        foreach (var id in msgIds)
            RegisterHandlerClient(id);
    }

    public void RegisterHandlerServer(short msgId)
    {
        msgId += (short)(MsgType.Highest + 1 + OpCodeOffset);
        if (IsServer) NetworkServer.RegisterHandler(msgId, OnMessageReceived);
        else throw new Exception("Cannot register server handlers when not being server");
    }

    public void RegisterHandlerClient(short msgId)
    {
        msgId += (short)(MsgType.Highest + 1 + OpCodeOffset);
        if (IsClient) NetworkManager.singleton.client.RegisterHandler(msgId, OnMessageReceived);
        else throw new Exception("Cannot register client handlers when not being client");
    }

    public void SendToClients(short msgId, MessageBase msg)
    {
        msgId += (short)(MsgType.Highest + 1 + OpCodeOffset);
        if (IsServer) NetworkServer.SendToAll(msgId, msg);
        else throw new Exception("Cannot send message to client when not being server");
    }

    public void SendToClient(NetMessengerClient client, short msgId, MessageBase msg)
    {
        msgId += (short)(MsgType.Highest + 1 + OpCodeOffset);
        if (IsServer) NetworkServer.SendToClient(client.Connection.connectionId, msgId, msg);
        else throw new Exception("Cannot send message to client when not being server");
    }

    public void SendToClient(NetworkConnection client, short msgId, MessageBase msg)
    {
        msgId += (short)(MsgType.Highest + 1 + OpCodeOffset);
        if (IsServer) NetworkServer.SendToClient(client.connectionId, msgId, msg);
        else throw new Exception("Cannot send message to client when not being server");
    }

    public void SendToServer(short msgId, MessageBase msg)
    {
        msgId += (short)(MsgType.Highest + 1 + OpCodeOffset);
        if (IsClient) NetworkManager.singleton.client.Send(msgId, msg);
        else throw new Exception("Cannot send message to server when not being client");
    }

    public T GetConnectionData(NetworkConnection cnn)
    {
        var cnnData = connectionsData.Find(c => c.Connection == cnn);
        if (cnnData != null) return cnnData;

        cnnData = new T {Connection = cnn};
        connectionsData.Add(cnnData);
        return cnnData;
    }

    public float GetServerDelay(int timeStamp)
    {
        if (IsServer) return 0;
        return GetDelay(ConnectionToServer, timeStamp);
    }

    public float GetDelay(NetworkConnection cnn, int timeStamp)
    {
        //If no host id is the local client
        if (cnn.hostId < 0) return 0;
        byte error;
        var ms = NetworkTransport.GetRemoteDelayTimeMS(cnn.hostId, cnn.connectionId, timeStamp, out error);
        return ms / 1000f;
    }

    protected virtual void OnStartServer()
    {
        if (ClientMessagesToSend != null)
            RegisterHandlersServer(ClientMessagesToSend);
    }
    protected virtual void OnStartClient()
    {
        if (ServerMessagesToSend != null)
            RegisterHandlersClient(ServerMessagesToSend);
    }
    protected virtual void OnStartSync() { }
    
    protected virtual void OnMessageFromServer(short msgType, NetworkMessage msg) { }
    protected virtual void OnMessageFromClient(T cnn, short msgType, NetworkMessage msg) { }
}