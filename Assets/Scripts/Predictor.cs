using UnityEngine;
using System.Collections;

public class Predictor : MonoBehaviour {

	public Transform observedTransform;
	public PlayerControl receiver;
	public float pingMargin = 0.5f; //ping top-margin
	
	private float clientPing;
	private NetworkState[] serverStateBuffer= new NetworkState[20];

	public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		Vector3 pos = observedTransform.position;
		Quaternion rot = observedTransform.rotation;
		
		if (stream.isWriting) {
			//Debug.Log("Server is writing");
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
		}
		else {
			//This code takes care of the local client!
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			receiver.serverPosition = pos;
			receiver.serverRotation = rot;
			//Smoothly correct clients position
			receiver.lerpToTarget();
			
			//Take care of data for interpolating remote objects movements
			// Shift up the buffer
			for ( int i = serverStateBuffer.Length - 1; i >= 1; i-- ) {
				serverStateBuffer[i] = serverStateBuffer[i-1];
			}
			//Override the first element with the latest server info
			serverStateBuffer[0] = new NetworkState((float)info.timestamp, pos, rot);
		}
	}


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
