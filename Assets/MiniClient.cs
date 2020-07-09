using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class MiniClient : MonoBehaviour {

    public string serverAddress = "127.0.0.1";
    public int serverPort = 8089;
    public bool testing = false;

    private MiniHostData testHostData;

    private void Start()
    {
        if (testing)
        {
            testHostData = new MiniHostData
            {
                name = "Test Server",
                password = "abcdefgh",
                country = "FR",
                map = "superMap",
                ip = "127.0.0.1",
                port = 8090,
                timePerMap = 1000,
                timePerRound = 100,
                playerNow = Random.Range(0, 10),
                playerMax = 10,
                version = 1
            };
        }
    }

    private void OnGUI()
    {
        if (testing)
        {
            if (GUI.Button(new Rect(10, 10, 200, 30), "Client: Register"))
            {
                // update playerNow to simulate a host update on server
                testHostData.playerNow = Random.Range(0, 10);

                if (Register(serverAddress, serverPort, testHostData))
                {
                    Debug.Log("MiniClient: registration success");
                }
                else
                {
                    Debug.LogError("MiniClient: registration failed");
                }
            }

            if (GUI.Button(new Rect(10, 50, 200, 30), "Client: UnRegister"))
            {
                if (UnRegister(serverAddress, serverPort, testHostData))
                {
                    Debug.Log("MiniClient: unregistration success");
                }
                else
                {
                    Debug.LogError("MiniClient: unregistration failed");
                }
            }

            if (GUI.Button(new Rect(10, 90, 200, 30), "Client: GetServerList"))
            {
                List<MiniHostData> serverList = GetServerList(serverAddress, serverPort);
                Debug.Log("MiniClient: servers found: " + serverList.Count);
                if (serverList.Count > 0)
                {
                    Debug.Log("MiniClient: server #1: " + serverList[0].name);
                }
            }
        }
    }

    public static List<MiniHostData> GetServerList(string serverAddress, int serverPort)
    {
        List<MiniHostData> serverList = new List<MiniHostData>();
        TcpClient socket = null;

        try
        {
            socket = new TcpClient(serverAddress, serverPort);

            if (MiniUtility.Write(socket, "list"))
            {
                string s = MiniUtility.Read(socket);
                if (!string.IsNullOrEmpty(s))
                {
                    serverList = (List<MiniHostData>)MiniUtility.Deserialize(s);
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniClient::GetServerList: Socket exception: " + socketException);
        }

        if (socket != null && socket.Connected)
        {
            socket.Close();
        }

        return serverList;
    }

    public static bool Register(string serverAddress, int serverPort, MiniHostData hostData)
    {
        TcpClient socket = null;
        bool success = false;

        try
        {
            socket = new TcpClient(serverAddress, serverPort);

            if (MiniUtility.Write(socket, "register"))
            {
                string s = MiniUtility.Serialize(hostData);
                if (!string.IsNullOrEmpty(s))
                {
                    success = MiniUtility.Write(socket, s);
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniClient::Register: Socket exception: " + socketException);
        }

        if (socket != null && socket.Connected)
        {
            socket.Close();
        }

        return success;
    }

    public static bool UnRegister(string serverAddress, int serverPort, MiniHostData hostData)
    {
        TcpClient socket = null;
        bool success = false;

        try
        {
            socket = new TcpClient(serverAddress, serverPort);

            if (MiniUtility.Write(socket, "unregister"))
            {
                string s = MiniUtility.Serialize(hostData);
                if (!string.IsNullOrEmpty(s))
                {
                    success = MiniUtility.Write(socket, s);
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniClient::UnRegister: Socket exception: " + socketException);
        }

        if (socket != null && socket.Connected)
        {
            socket.Close();
        }

        return success;
    }
}
