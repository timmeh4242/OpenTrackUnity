using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System;

public class GyroBehaviour : MonoBehaviour
{
    public Vector3 RawEulers => GyroTarget.eulerAngles;
    public Vector3 CalibratedEulers => GyroTarget.localEulerAngles;

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
        UpdateGyro();

        if(udpClient != null)
        {
            var byteArray = BuildMessage(CalibratedEulers);
            udpClient.Send(byteArray, byteArray.Length);
        }
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