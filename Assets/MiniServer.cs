using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class MiniServer : MonoBehaviour {

    public int port = 8089;
    public bool testing = false;

    [SerializeField]
    public List<MiniHostData> serverList = new List<MiniHostData>();

    private TcpListener serverSocket;
    private Thread listenerThread;

    void Start()
    {
        if (testing)
        {
            for (int i = 0; i < 100; i++)
            {
                serverList.Add(new MiniHostData
                {
                    name = "Test Server " + i,
                    password = "abcdefgh",
                    country = "FR",
                    map = "superMap",
                    ip = "127.0.0.1",
                    port = 8090,
                    timePerMap = 1000,
                    timePerRound = 100,
                    playerNow = 5,
                    playerMax = 10,
                    version = 1
                });
            }
        }

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
            serverSocket = new TcpListener(IPAddress.Any, port);
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
            if (socketException.ErrorCode != 10004) // WSACancelBlockingCall (thread close)
                Debug.LogError("MiniServer::ServerThread: SocketException " + socketException.ErrorCode);
        }
    }

    private void ClientThread(TcpClient socket)
    {
        Debug.Log("MiniServer::ClientThread: new client thread: " + socket.Client.LocalEndPoint.ToString());

        string cmd = MiniCommon.Read(socket);
        if (cmd == "list")
        {
            Debug.Log("MiniServer::ClientThread: requesting server list");
            string s = MiniCommon.Serialize(serverList);
            if (!MiniCommon.Write(socket, s))
            {
                Debug.LogError("MiniServer::ClientThread: could not send server list");
            }
        }
        else if (cmd == "register")
        {
            Debug.Log("MiniServer::ClientThread: requesting client registration");
            string s = MiniCommon.Read(socket);
            if (!string.IsNullOrEmpty(s))
            {
                MiniHostData hostData = (MiniHostData)MiniCommon.Deserialize(s);
                if (hostData != null)
                {
                    MiniHostData registredHost = serverList.Find(h => h.ip == hostData.ip && h.port == hostData.port);
                    if (registredHost != null)
                    {
                        Debug.Log("MiniServer::ClientThread: client registration already done, updating host data for address " + hostData.ip);
                        serverList[serverList.IndexOf(registredHost)] = hostData;
                    }
                    else
                    {
                        serverList.Add(hostData);
                        Debug.Log("MiniServer::ClientThread: registred host: " + hostData.ip);
                    }
                }
                else
                {
                    Debug.LogError("MiniServer::ClientThread: could not deserialize host data");
                }
            }
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
