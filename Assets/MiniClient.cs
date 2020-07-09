using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class MiniClient : MonoBehaviour {

    public string serverAddress = "127.0.0.1";
    public int serverPort = 8089;
    public bool testing = false;

    void OnGUI()
    {
        if (testing)
        {
            System.Random rand = new System.Random();
            double uid = rand.NextDouble();

            if (GUI.Button(new Rect(10, 10, 200, 30), "Client: Register"))
            {
                MiniHostData hostData = new MiniHostData
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

                if (Register(serverAddress, serverPort, hostData))
                {
                    Debug.Log("MiniClient: registration success");
                }
                else
                {
                    Debug.LogError("MiniClient: registration failed");
                }
            }

            if (GUI.Button(new Rect(10, 50, 200, 30), "Client: GetServerList"))
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

            if (MiniCommon.Write(socket, "list"))
            {
                string s = MiniCommon.Read(socket);
                if (!string.IsNullOrEmpty(s))
                {
                    serverList = (List<MiniHostData>)MiniCommon.Deserialize(s);
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

            if (MiniCommon.Write(socket, "register"))
            {
                string s = MiniCommon.Serialize(hostData);
                if (!string.IsNullOrEmpty(s))
                {
                    success = MiniCommon.Write(socket, s);
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
}
