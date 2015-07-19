using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode] // makes GUI show in edit mode
public class Networking : MonoBehaviour
{
    private string gameName = "TheVenomEventTestName"; // make the name authentic to reduce chance of error

    private bool refreshing = false;
    private HostData[] hostData = new HostData[0]; // a list of all current hosts

    public GameObject playerPrefab; // your player

    public bool create = false;
    public bool joining = false;
    
    public string serverName = "";
    public string serverInfo = "";
    public string serverPass = "";
    
    public string playerName = "";
    public string clientPass = "";
    
    public Vector2 scrollPosition = Vector2.zero;

    private void Start()
    {
        this.playerName = PlayerPrefs.GetString("Player Name"); // loads your previosuly used player names
    }

    private void OnGUI()
    {
        if (!Network.isClient && !Network.isServer)
        {
            // if you arent the server or a client
            if (!this.create && !this.joining)
            {
                if (GUI.Button(new Rect(Screen.width/2f - 50f, Screen.height/2f, 100, 20), "Create Game"))
                {
                    this.create = true;
                }

                if (GUI.Button(new Rect(Screen.width/2f - 50f, Screen.height/2f + 30f, 100, 20), "Find Game"))
                {
                    this.joining = true;
                    this.RefreshHostList();
                }
            }

            if (this.create)
            {

                if (GUI.Button(new Rect(Screen.width/2f - 50f, Screen.height/3f + 110f, 100, 50), "Create"))
                {
                    this.StartServer();
                }

                GUI.Label(new Rect(Screen.width/2f - 110f, Screen.height/3f, 100, 20), "Server Name:");
                GUI.Label(new Rect(Screen.width/2f + 40f, Screen.height/3f, 100, 20), "Password:");
                GUI.Label(new Rect(Screen.width/2f - 30f, Screen.height/2f + 90, 100, 20), "Server Info:");

                this.serverName = GUI.TextField(new Rect(Screen.width/2f - 120f, Screen.height/3 + 30, 100, 20),
                    this.serverName, 12);
                this.serverPass = GUI.PasswordField(new Rect(Screen.width/2f + 20f, Screen.height/3 + 30, 100, 20),
                    this.serverPass, "*"[0], 12);
                this.serverInfo = GUI.TextArea(new Rect(Screen.width/2f - 70f, Screen.height/2 + 120, 150, 40),
                    this.serverInfo, 35);

                if (GUI.Button(new Rect(Screen.width/1.2f, Screen.height/20f, 100, 20f), "Back"))
                {
                    this.create = false;
                }
            }

            if (this.joining)
            {
                if (this.hostData.Any())
                {
                    this.scrollPosition = GUI.BeginScrollView(
                        new Rect(Screen.width/4f, Screen.height/6f, Screen.width/1.5f, Screen.height/2f),
                        this.scrollPosition,
                        new Rect(0, 0, 300, 1000/this.hostData.Length*30));

                    GUI.Label(new Rect(30, 0, 100, 20), "Game Name");
                    GUI.Label(new Rect(350, 0, 100, 20), "Server Info");
                    GUI.Label(new Rect(590, 0, 100, 20), "Player Count");
                    GUI.Label(new Rect(700, 0, 100, 20), "Password");

                    for (var i = 0; i < this.hostData.Length; i++)
                    {
                        GUI.Label(new Rect(0, 30 + i*30, 200, 22), this.hostData[i].gameName);
                        GUI.Label(new Rect(160, 30 + i*30, 500, 22), this.hostData[i].comment);
                        GUI.Label(new Rect(610, 30 + i*30, 100, 20),
                            this.hostData[i].connectedPlayers + " / " + this.hostData[i].playerLimit);

                        if (this.hostData[i].passwordProtected)
                        {
                            this.clientPass = GUI.PasswordField(new Rect(680, 30 + i*30, 100, 25), this.clientPass,
                                "*"[0], 12);
                        }

                        if (GUI.Button(new Rect(800, 30 + i*30, 100, 25), "Join"))
                        {
                            Network.Connect(this.hostData[i], this.clientPass);
                            Application.LoadLevel("Level");
                        }
                    }

                    GUI.EndScrollView();
                }

                if (!this.hostData.Any())
                {
                    GUI.Label(new Rect(Screen.width/2f - 50f, Screen.height/3f, 200, 25), "No Games Found");

                    if (GUI.Button(new Rect(Screen.width/2f - 50f, Screen.height/3f + 30f, 105, 25), "Refresh"))
                    {
                        this.RefreshHostList();
                    }
                }

                if (GUI.Button(new Rect(Screen.width/1.2f, Screen.height/20f, 100, 20), "Back"))
                {
                    this.joining = false;
                }
            }

            if (GUI.Button(new Rect(Screen.width/20f, Screen.height/20f, 100, 20), "Quit"))
            {
                Application.Quit();
            }

            GUI.Label(new Rect(Screen.width/2f - 35f, Screen.height/1.2f - 30f, 100, 20), "Your Name:");
            this.playerName = GUI.TextField(new Rect(Screen.width/2f - 50f, Screen.height/1.2f, 100, 20),
                this.playerName, 12);
        }
    }

    private void Update()
    {
        if (this.refreshing)
        {
            if (MasterServer.PollHostList().Length > 0)
            {
                this.refreshing = false;
                this.hostData = MasterServer.PollHostList();
            }
        }
    }

    private void StartServer()
    {
        if (this.serverPass != "")
        {
            Network.incomingPassword = this.serverPass;
        }

        Network.InitializeServer(15, 25001, !Network.HavePublicAddress());
        MasterServer.RegisterHost(this.gameName, this.serverName, this.serverInfo);
    }

    private void OnServerInitialized()
    {
        DontDestroyOnLoad(this.transform.gameObject);

        Application.LoadLevel("Level");

        this.LobbySpawn();
    }

    private void OnConnectedToServer()
    {
        this.LobbySpawn();
    }

    private IEnumerator LobbySpawn()
    {
        yield return new WaitForSeconds(0.1f);

        var made =
            Network.Instantiate(this.playerPrefab, this.transform.position, this.transform.rotation, 0) as GameObject;
        made.GetComponent<PlayerControl>().playerName = this.playerName;

        PlayerPrefs.SetString("Player Name", this.playerName);

        if (Network.isClient)
        {
            Destroy(this);
        }
    }

    private void RefreshHostList()
    {
        MasterServer.ClearHostList();
        MasterServer.RequestHostList(this.gameName);
        this.refreshing = true;
    }
}