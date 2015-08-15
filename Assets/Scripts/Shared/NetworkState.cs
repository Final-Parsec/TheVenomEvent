using UnityEngine;
using System.Collections;

public class NetworkState{
	public float timestamp;
	public Vector3 position;
	public Quaternion rotation;
	
	public NetworkState() {
		timestamp = 0.0f;
		position = Vector3.zero;
		rotation = Quaternion.identity;
	}
	
	public NetworkState(float time, Vector3 position, Quaternion rotation) {
		timestamp = time;
		this.position = position;
		this.rotation = rotation;
	}
}
