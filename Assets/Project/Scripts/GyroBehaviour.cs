using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System;

public class GyroBehaviour : MonoBehaviour
{
    public Transform GyroRoot;
    public Transform GyroTarget;
    Gyroscope gyro => Input.gyro;

    UdpClient udpClient;

    [SerializeField] string OPENTRACK_IP = "127.0.0.1";
    [SerializeField] int OPENTRACK_PORT = 4242;

    void OnDisable()
    {
        udpClient?.Close();
    }

    public void UpdateIP(string ip)
    {
        OPENTRACK_IP = ip;
    }

    public void ConnectClient()
    {
        udpClient?.Close();

        var ipEndPoint = new IPEndPoint(IPAddress.Parse(OPENTRACK_IP), OPENTRACK_PORT);
        udpClient = new UdpClient();
        udpClient.Connect(ipEndPoint);
    }

    public void Calibrate()
    {
        GyroRoot.rotation = GyroTarget.rotation;
        UpdateGyro();
    }

    void Update()
    {
        if (udpClient == null)
            return;

        UpdateGyro();

        var byteArray = BuildMessage(GyroTarget.localEulerAngles);
        udpClient.Send(byteArray, byteArray.Length);
    }

    void UpdateGyro()
    {
        if (gyro.enabled)
        {
            GyroTarget.rotation = gyro.attitude;
        }
    }

    private byte[] BuildMessage(Vector3 orientation)
    {
        return new double[]
        {
                0,
                0,
                0,
                (double)orientation.x,
                (double)orientation.y,
                (double)orientation.z
        }.SelectMany(BitConverter.GetBytes).ToArray();
    }
}