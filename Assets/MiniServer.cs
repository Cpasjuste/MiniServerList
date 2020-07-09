using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MiniServer : MonoBehaviour
{
    //public string address;
    public int port;

    private TcpListener serverSocket;
    private Thread listenerThread;

    MiniHostData miniHostData = new MiniHostData
    {
        name = "Test Server",
        comment = "Test Comment"
    };

    void Start()
    {
        listenerThread = new Thread(ServerThread)
        {
            IsBackground = true
        };
        listenerThread.Start();
    }

    private void ServerThread()
    {
        try
        {
            serverSocket = new TcpListener(IPAddress.Any, 8052);
            serverSocket.Start();
            Debug.Log("MiniServer::ServerThread: listening on " + serverSocket.LocalEndpoint.ToString());

            while (true)
            {
                TcpClient socket = serverSocket.AcceptTcpClient();
                Debug.Log("MiniServer::ServerThread: new client: " + socket.Client.LocalEndPoint.ToString());
                Thread thread = new Thread(() => ClientThread(socket))
                {
                    IsBackground = true
                };
                thread.Start();
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniServer::ServerThread: SocketException " + socketException.ToString());
        }
    }

    private void ClientThread(TcpClient socket)
    {
        Debug.Log("MiniServer::ClientThread: new client thread: " + socket.Client.LocalEndPoint.ToString());

        string message = MiniCommon.Read(socket);
        if (message == "list")
        {
            Debug.Log("MiniServer::ClientThread: requesting server list");
            string serialized = MiniCommon.Serialize(miniHostData);
            MiniCommon.Write(socket, serialized);
        }

        socket.Close();
    }

    private void OnDestroy()
    {
        Debug.Log("MiniServer::OnDestroy: exiting... ");

        serverSocket.Server.Close();
        serverSocket.Server.Dispose();
        serverSocket.Stop();
    }
}
