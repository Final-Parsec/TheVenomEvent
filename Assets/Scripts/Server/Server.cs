using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Server : MonoBehaviour {

	private static int bufferSize = 1024;

	private int reliableChannelId;
	private int socketId;
	private int socketPort = 8888;


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
			Debug.Log("incoming connection event received");
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
