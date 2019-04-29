using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPserver : MonoBehaviour
{
    private IPEndPoint remoteEndPoint;
    private UdpClient udpClient;

    private int PORT;

    //--------------------------------------------------------
    public void initialUDP(string ipAddress, int port)
    {
        // Initialize UDP protocal
        string IP = ipAddress;
        PORT = port;
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), PORT);
        udpClient = new UdpClient(PORT);

        Debug.Log("UDPsend is initialized");
    }

    public void UDPSendMessage(string message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            udpClient.Send(data, data.Length, remoteEndPoint);
        }
        catch (SocketException error)
        {
            Debug.Log(error);
        }
    }

    private void OnDestroy()
    {
        udpClient.Close();
    }
}