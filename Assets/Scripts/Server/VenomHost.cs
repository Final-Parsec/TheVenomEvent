using UnityEngine;

[ExecuteInEditMode] 
public class VenomHost : MonoBehaviour
{
    private static readonly IConfigurationProvider ConfigurationProvider = new HardCodedConfigurationProvider();
    private static readonly IMapData MapData = new HardCodedMapData();

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