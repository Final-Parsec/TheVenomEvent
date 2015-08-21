////////NetworkManager Script:

using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]

public class NetworkManager : MonoBehaviour {

	public static string serverIp = "127.0.0.1";
	//public static string serverIp = "162.243.141.8";


	public Transform player;
	HostData[] hostData;
	public string chosenGameName = "";
	public NetworkPlayer myNetworkPlayer;

    void Start()
    {
        Network.Connect(serverIp, 2003);
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
            PlayerControl playerControl = myPlayer.GetComponent<PlayerControl>();
			playerControl.inControl = true;
			CameraFollow cameraFollow = (Instantiate (Resources.Load ("RuntimePrefabs/MainCamera")) as GameObject).GetComponent<CameraFollow>();
			cameraFollow.Player = playerControl.transform;
			cameraFollow.gameObject.tag = "MainCamera";
        }
    }

}
