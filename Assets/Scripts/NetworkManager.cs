////////NetworkManager Script:

using UnityEngine;
using System.Collections;
[ExecuteInEditMode]

public class NetworkManager : MonoBehaviour {
	
	public Transform player;
	string registeredName = "somekindofuniquename";
	float refreshRequestLength = 3.0f;
	HostData[] hostData;
	public string chosenGameName = "";
	public NetworkPlayer myPlayer;
	
	private void StartServer(){
		Network.InitializeServer (16, Random.Range(2000,2500),!Network.HavePublicAddress());
		MasterServer.RegisterHost(registeredName,chosenGameName);
	}
	
	void OnServerInitialized(){ 
		if(Network.isServer){
			myPlayer = Network.player;
			makePlayer(myPlayer);
		}
	}
	
	void OnConnectedToServer() {
		myPlayer = Network.player;
		GetComponent<UnityEngine.NetworkView>().RPC("makePlayer", RPCMode.Server, myPlayer);
	}
	
	[RPC]
	void makePlayer(NetworkPlayer thisPlayer){
		Transform newPlayer = Network.Instantiate (player, transform.position, transform.rotation, 0) as Transform;
		if (thisPlayer != myPlayer) {
			GetComponent<UnityEngine.NetworkView>().RPC ("enableCamera", thisPlayer, newPlayer.GetComponent<UnityEngine.NetworkView>().viewID);
		}
		else{
			enableCamera(newPlayer.GetComponent<UnityEngine.NetworkView>().viewID);
		}
	}
	
	[RPC]
	void enableCamera(NetworkViewID playerID){
		GameObject[] players;
		players = GameObject.FindGameObjectsWithTag ("Player");
		foreach(GameObject thisPlayer in players){
			if(thisPlayer.GetComponent<UnityEngine.NetworkView>().viewID == playerID){
				thisPlayer.GetComponent<PlayerControl>().inControl = true;
				Transform myCamera = thisPlayer.transform.Find("Camera");
				myCamera.GetComponent<Camera>().enabled = true;
				myCamera.GetComponent<Camera>().GetComponent<AudioListener>().enabled = true;
				break;
			}
		}
	}
	
	public IEnumerator RefreshHostList (){
		MasterServer.RequestHostList (registeredName);
		float timeEnd = Time.time + refreshRequestLength;
		while (Time.time < timeEnd) {
			hostData = MasterServer.PollHostList();
			yield return new WaitForEndOfFrame();
		}
	}
	
	public void OnGUI(){
		if (Network.isClient || Network.isServer) {
			return;
		}
		if(chosenGameName == ""){
			GUI.Label(new Rect(Screen.width/2 - Screen.width/10, Screen.height/2 - Screen.height/20, Screen.width/5,Screen.height/20), "Game Name");
		}
		chosenGameName = GUI.TextField(new Rect(Screen.width/2 - Screen.width/10, Screen.height/2 - Screen.height/20, Screen.width/5,Screen.height/20), chosenGameName, 25);
		if (GUI.Button (new Rect (Screen.width/2 - Screen.width/10, Screen.height/2, Screen.width/5,Screen.height/10), "Start New Server")) {
			StartServer();
		}
		if (GUI.Button (new Rect (Screen.width/2 - Screen.width/10, Screen.height/2 + Screen.height/10, Screen.width/5,Screen.height/10), "Find Servers")) {
			StartCoroutine(RefreshHostList());
		}
		if (hostData != null) {
			for(int i = 0; i < hostData.Length; i++){
				if(GUI.Button (new Rect (Screen.width/2 - Screen.width/10, Screen.height/2 + ((Screen.height/20)*(i+4)), Screen.width/5,Screen.height/20), hostData[i].gameName)) {
					Network.Connect(hostData[i]);
				}
			}
		}
	}
}
