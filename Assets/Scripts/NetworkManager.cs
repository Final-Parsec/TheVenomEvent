////////NetworkManager Script:

using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]

public class NetworkManager : MonoBehaviour {
	
	public Transform player;
	string registeredName = "somekindofuniquename";
	float refreshRequestLength = 3.0f;
	HostData[] hostData;
	public string chosenGameName = "";
	public NetworkPlayer myNetworkPlayer;

    void Start()
    {
        Network.Connect("127.0.0.1", 2003);
        //Network.Connect("162.243.141.8", 2003);
    }
	
	void OnConnectedToServer() {
        this.myNetworkPlayer = Network.player;
	    this.GetComponent<NetworkView>().RPC("MakePlayer", RPCMode.Server, this.myNetworkPlayer, 1);
	}

    [RPC]
    void MakePlayer(NetworkPlayer thisPlayer, int teamId)
    {
        // Faking it to make it. See VenomHost.MakePlayer
    }

    [RPC]
    void SetupPlayerControl(NetworkViewID playerId)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        var myPlayer = players.FirstOrDefault(thisPlayer => thisPlayer.GetComponent<NetworkView>().viewID == playerId);
        if (myPlayer != null)
        {
            myPlayer.GetComponent<PlayerControl>().inControl = true;
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
	
	public IEnumerator RefreshHostList ()
	{
	    MasterServer.ipAddress = "127.0.0.1";
	    MasterServer.port = 2003;
        
        MasterServer.RequestHostList ("A");
		float timeEnd = Time.time + refreshRequestLength;
		while (Time.time < timeEnd) {
			hostData = MasterServer.PollHostList();
			yield return new WaitForEndOfFrame();
		}
	}
	
    ////public void OnGUI(){
    ////    if (Network.isClient || Network.isServer) {
    ////        return;
    ////    }
    ////    if(chosenGameName == ""){
    ////        GUI.Label(new Rect(Screen.width/2 - Screen.width/10, Screen.height/2 - Screen.height/20, Screen.width/5,Screen.height/20), "Game Name");
    ////    }
    ////    chosenGameName = GUI.TextField(new Rect(Screen.width/2 - Screen.width/10, Screen.height/2 - Screen.height/20, Screen.width/5,Screen.height/20), chosenGameName, 25);
    ////    if (GUI.Button (new Rect (Screen.width/2 - Screen.width/10, Screen.height/2, Screen.width/5,Screen.height/10), "Start New Server")) {
    ////        StartServer();
    ////    }
    ////    if (GUI.Button (new Rect (Screen.width/2 - Screen.width/10, Screen.height/2 + Screen.height/10, Screen.width/5,Screen.height/10), "Find Servers")) {
    ////        StartCoroutine(RefreshHostList());
    ////    }
    ////    if (hostData != null) {
    ////        for(int i = 0; i < hostData.Length; i++){
    ////            if(GUI.Button (new Rect (Screen.width/2 - Screen.width/10, Screen.height/2 + ((Screen.height/20)*(i+4)), Screen.width/5,Screen.height/20), hostData[i].gameName)) {
    ////                Network.Connect(hostData[i]);
    ////            }
    ////        }
    ////    }
    ////}
}
