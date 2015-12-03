using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class PhotonAckClient : MonoBehaviour, IPhotonPeerListener 
{
    public PhotonPeer peer;
    private bool connected = false;

    public long localPlayerID;

	// Use this for initialization
	void Start () 
    {
        peer = new PhotonPeer(this, ConnectionProtocol.Udp);
        bool isConnected = peer.Connect("10.0.0.115:5055", "StarCollectorServer");
        Debug.Log(isConnected);
        StartCoroutine(DoService());
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnGUI()
    {
        GUILayout.Label("Connected: " + connected.ToString());
        if (connected)
        {
            if (GUILayout.Button("Send Operation Request"))
            {
                // send a message to the server
                peer.OpCustom(0, new Dictionary<byte, object>(), true);
            }
        }
    }

    IEnumerator DoService() 
    {
        while (true) 
        {
            peer.Service();
            yield return new WaitForSeconds(.1f);
        }
    }

    #region IPhotonPeerListener Members
    public void DebugReturn(DebugLevel level, string message)
    {
        // log message to console
        Debug.Log(message);
    }

    public void OnEvent(EventData eventData)
    {
        //server raised an event
        Debug.Log("Received event - type: " + eventData.Code.ToString());

        if((AckEventType)eventData.Code == AckEventType.AssignPlayerID)
        {
            this.localPlayerID = (long)eventData.Parameters[(byte)EventParameter.PlayerID];
            Debug.Log("Received player ID: " + localPlayerID);
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        //server sent operation response
        Debug.Log("Received op response - type: " +
        operationResponse.OperationCode.ToString());
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        // connected to Photon server
        if (statusCode == StatusCode.Connect)
        {
            connected = true;
        }
        // log status change
        Debug.Log("Status change: " + statusCode.ToString());
    }
    #endregion
}
