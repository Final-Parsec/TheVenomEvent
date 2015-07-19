using UnityEngine;

public class NetworkView : MonoBehaviour
{
    void Awake()
    {
        if (!this.GetComponent<UnityEngine.NetworkView>().isMine)
        { 
            this.GetComponentInChildren<Camera>().enabled = false;
            this.GetComponent<CameraFollow>().enabled = false;
            this.GetComponent<PlayerControl>().enabled = false;
        } 
    }
}