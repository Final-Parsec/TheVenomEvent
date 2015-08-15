using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode] 
public class VenomHost : MonoBehaviour
{
    private static readonly IConfigurationProvider ConfigurationProvider = new HardCodedConfigurationProvider();
    private static readonly IMapData MapData = new HardCodedMapData();
	private List<PlayerControl> players = new List<PlayerControl>();

    public Transform player;
    

    void Start()
    {
        Debug.Log("Start");
        this.StartServer();
    }

    void OnConnectedToServer()
    {
        Debug.Log("OnConnectedToServer");
    }

    void OnServerInitialized()
    {
        Debug.Log("The server has started!");
    }

	void OnPlayerDisconnected(NetworkPlayer disconnectedPlayer){
		PlayerControl found= null;
		foreach (PlayerControl player in players) {
			if (player.owner == disconnectedPlayer) {
				found = player;
				Network.RemoveRPCs(player.netView.viewID);
				Network.Destroy(player.gameObject);
			}
		}
		if (found) {
			players.Remove(found);
		}
	}

    private void StartServer()
    {
        Debug.Log("StartServer");
        Network.InitializeServer(
            VenomHost.ConfigurationProvider.MaximumNumberOfConnections,
            VenomHost.ConfigurationProvider.Port,
            !Network.HavePublicAddress());
        MasterServer.dedicatedServer = true;
        MasterServer.RegisterHost(
            "A",
            VenomHost.ConfigurationProvider.Name);
    }

    [RPC]
    void MakePlayer(NetworkPlayer thisPlayer, int teamId)
    {

        Debug.Log("MakePlayer RPC");
        var newPlayer = Network.Instantiate(
            this.player,
            VenomHost.MapData.GetSpawnPosition(teamId),
            Quaternion.Euler(Vector3.zero),
            0) as Transform;

		PlayerControl playerControler = newPlayer.GetComponent<PlayerControl>();
		playerControler.owner = thisPlayer;
		players.Add(playerControler);

        this.GetComponent<NetworkView>().RPC(
            "SetupPlayerControl",
            thisPlayer,
            newPlayer.GetComponent<NetworkView>().viewID);
    }

    [RPC]
    void SetupPlayerControl(NetworkViewID playerId)
    {
        // Fake it, bruh.
    }
}