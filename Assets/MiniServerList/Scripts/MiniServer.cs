using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class MiniServer : MonoBehaviour {

    public int listeningPort = 8089;
    public int hostCheckTimeout = 60;

    [SerializeField]
    public List<MiniHostData> serverList = new List<MiniHostData>();

    private TcpListener serverSocket;
    private Thread listenerThread;
    private Mutex mutex = new Mutex();
    private System.Timers.Timer timer;

    private void Start()
    {
        // server main thread
        listenerThread = new Thread(ServerThread)
        {
            IsBackground = true
        };
        listenerThread.Start();

        // create a timer for hosts checking
        timer = new System.Timers.Timer(hostCheckTimeout * 1000);
        timer.Elapsed += OnCheckHost;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    // check for host avaibility, update server list if needed
    private void OnCheckHost(System.Object source, System.Timers.ElapsedEventArgs e)
    {
        // make a copy of server list for threaded access
        List<MiniHostData> list = new List<MiniHostData>(serverList);

        foreach (MiniHostData hostData in list)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(hostData.ip, hostData.port);
                    //Debug.Log("MiniServer::OnCheckHost: host is up: " + hostData.ip);
                }
                catch (System.Exception)
                {
                    // server down, remove from server list
                    Debug.Log("MiniServer::OnCheckHost: host seems down, removing: " + hostData.ip);
                    mutex.WaitOne();
                    serverList.Remove(hostData);
                    mutex.ReleaseMutex();
                }
            }
        }
    }

    private void ServerThread()
    {
        try
        {
            serverSocket = new TcpListener(IPAddress.Any, listeningPort);
            serverSocket.Start();
            Debug.Log("MiniServer::ServerThread: listening on " + serverSocket.LocalEndpoint.ToString());

            while (true)
            {
                TcpClient socket = serverSocket.AcceptTcpClient();
                Thread thread = new Thread(() => ClientCommand(socket))
                {
                    IsBackground = true
                };
                thread.Start();
            }
        }
        catch (SocketException socketException)
        {
            if (socketException.ErrorCode != 10004) // WSACancelBlockingCall (thread close)
            {
                Debug.LogError("MiniServer::ServerThread: SocketException " + socketException.ErrorCode);
            }
        }
    }

    private void ClientCommand(TcpClient socket)
    {
        //Debug.Log("MiniServer::ClientCommand: new client thread: " + socket.Client.LocalEndPoint.ToString());

        string cmd = MiniUtility.Read(socket);
        if (cmd == "list")
        {
            //Debug.Log("MiniServer::ClientCommand: server list requested");
            mutex.WaitOne();
            string s = MiniUtility.Serialize(serverList);
            mutex.ReleaseMutex();
            if (!MiniUtility.Write(socket, s))
            {
                Debug.LogError("MiniServer::ClientCommand: could not send server list to client: "
                    + socket.Client.LocalEndPoint.ToString());
            }
        }
        else if (cmd == "register")
        {
            //Debug.Log("MiniServer::ClientCommand: requesting client registration");
            string s = MiniUtility.Read(socket);
            if (!string.IsNullOrEmpty(s))
            {
                MiniHostData hostData = (MiniHostData)MiniUtility.Deserialize(s);
                if (hostData != null)
                {
                    mutex.WaitOne();
                    MiniHostData registredHost = serverList.Find(h => h.ip == hostData.ip && h.port == hostData.port);
                    if (registredHost != null)
                    {
                        Debug.Log("MiniServer::ClientCommand: updating host data: " + hostData.ip);
                        serverList[serverList.IndexOf(registredHost)] = hostData;
                    }
                    else
                    {
                        Debug.Log("MiniServer::ClientCommand: registred host: " + hostData.ip);
                        serverList.Add(hostData);
                    }
                    mutex.ReleaseMutex();
                }
                else
                {
                    Debug.LogError("MiniServer::ClientCommand: could not deserialize host data");
                }
            }
        }
        else if (cmd == "unregister")
        {
            //Debug.Log("MiniServer::ClientCommand: requesting host deletion");
            string s = MiniUtility.Read(socket);
            if (!string.IsNullOrEmpty(s))
            {
                MiniHostData hostData = (MiniHostData)MiniUtility.Deserialize(s);
                if (hostData != null)
                {
                    mutex.WaitOne();
                    MiniHostData registredHost = serverList.Find(h => h.ip == hostData.ip && h.port == hostData.port);
                    if (registredHost != null)
                    {
                        Debug.Log("MiniServer::ClientCommand: removing host: " + hostData.ip);
                        serverList.Remove(registredHost);
                    }
                    else
                    {
                        Debug.Log("MiniServer::ClientCommand: could not remove host, host not found: " + hostData.ip);
                    }
                    mutex.ReleaseMutex();
                }
                else
                {
                    Debug.LogError("MiniServer::ClientCommand: could not deserialize host data");
                }
            }
        }

        socket.Close();
    }

    private void OnDestroy()
    {
        Debug.Log("MiniServer::OnDestroy: cleanup... ");

        serverSocket.Server.Close();
        serverSocket.Server.Dispose();
        serverSocket.Stop();

        timer.Stop();
        timer.Dispose();

        mutex.Dispose();
    }
}
