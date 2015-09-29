using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Client : MonoBehaviour {

	public static string serverIp = "127.0.0.1";
	//public static string serverIp = "162.243.141.8";

	public static int serverPort = 8888;

	private static int bufferSize = 1024;

	private int reliableChannelId;
	private int socketId;


	private int socketPort = 8887;


	private int maxConnections = 10;


	private int connectionId;


	void Start () 
	{
		NetworkTransport.Init();
		ConnectionConfig config = new ConnectionConfig();
		reliableChannelId = config.AddChannel(QosType.Reliable);

		HostTopology topology = new HostTopology(config, maxConnections);

		socketId = NetworkTransport.AddHost(topology, socketPort);
		Debug.Log("Socket Open. SocketId is: " + socketId + " ChannelID is: " + reliableChannelId);

		ConnectToServer();
	}

	public void SendSocketMessage() 
	{
		byte error;
		byte[] buffer = new byte[1024];
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Serialize(stream, "HelloServer");
		
		NetworkTransport.Send(socketId, connectionId, reliableChannelId, buffer, bufferSize, out error);

		if(error != 0)
			Debug.Log("Couldn't send message");

	}

	public void ConnectToServer() 
	{
		byte error;
		connectionId = NetworkTransport.Connect(socketId, serverIp, socketPort, 0, out error);

		if(error != 0)
			Debug.Log("Couldn't connect to server: "+ serverIp );


		Debug.Log("Connected to server: "+ serverIp +" ConnectionId: " + connectionId);
	}
	
	// Update is called once per frame
	void Update () 
	{
		int recHostId;
		int recConnectionId;
		int recChannelId;
		byte[] recBuffer = new byte[1024];
		int dataSize;
		byte error;


		NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);

		if(error != 0)
			Debug.Log("Couldn't receive message");

		switch (recNetworkEvent) {
		case NetworkEventType.Nothing:
			break;
		case NetworkEventType.ConnectEvent:
			Debug.Log("incoming connection event received - HOST ID: " + recHostId + " CONNECTION ID: " + recConnectionId + " CHANNEL ID: " + recChannelId);
			break;
		case NetworkEventType.DataEvent:
			Stream stream = new MemoryStream(recBuffer);
			BinaryFormatter formatter = new BinaryFormatter();
			string message = formatter.Deserialize(stream) as string;
			Debug.Log("incoming message event received: " + message);
			break;
		case NetworkEventType.DisconnectEvent:
			Debug.Log("remote client event disconnected");
			break;


		}
	}
}
