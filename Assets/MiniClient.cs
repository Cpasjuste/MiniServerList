using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MiniClient : MonoBehaviour
{
    public string address;
    public int port;

    void Start()
    {
    }

    void Update()
    {
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 300, 40), "Client: GetServerList"))
        {
            GetServerList();
        }
    }

    void GetServerList()
    {
        try
        {
            TcpClient socket = new TcpClient(address, port);
            MiniCommon.Write(socket, "list");

            string serialized = MiniCommon.Read(socket);
            MiniHostData host = (MiniHostData)MiniCommon.Deserialize(serialized);
            Debug.Log("MiniClient: game found: " + host.name);

        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniClient::GetServerList: Socket exception: " + socketException);
        }
    }
}
