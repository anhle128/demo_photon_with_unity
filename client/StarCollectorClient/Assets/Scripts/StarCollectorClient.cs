using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using LMLiblary.Model;
using LMLiblary.General;

public class StarCollectorClient : MonoBehaviour, IPhotonPeerListener
{

    public static PhotonPeer connection;
    public static bool connected = false;
    public static long playerID;

    public string ServerIP = "10.0.0.115:5055";
    public string AppName = "StarCollectorServer";

    public GameObject playerPref;
    public GameObject creepPref;

	// Use this for initialization
	void Start () 
    {
        connection = new PhotonPeer(this, ConnectionProtocol.Udp);
        connection.Connect(ServerIP, AppName);

        StartCoroutine(DoService());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDestroy()
    {
        // explicitly disconnect if the client game object is destroyed
        if (connected)
            connection.Disconnect();
    }

    // update peer 20 times per second
    IEnumerator DoService()
    {
        while (true)
        {
            connection.Service();
            yield return new WaitForSeconds(0.05f);
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
        switch ((AckEventType)eventData.Code)
        {
            // store player ID
            case AckEventType.AssignPlayerID:
                long playerId = (long)eventData.Parameters[ 0 ];
                playerID = playerId;
                Debug.Log("player id: " + playerId);
                break;
            // create actor
            case AckEventType.CreateActor:
                break;
            case AckEventType.DestroyActor:
                break;
            // update actor
            case AckEventType.UpdateActor:
                break;
            // log chat messages
            case AckEventType.ChatMessage:
                break;
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
       // log status change
        Debug.Log( "Status change: " + statusCode.ToString() );
        switch (statusCode)
        {
            case StatusCode.Connect:
                break;
            case StatusCode.Disconnect:
            case StatusCode.DisconnectByServer:
            case StatusCode.DisconnectByServerLogic:
            case StatusCode.DisconnectByServerUserLimit:
            case StatusCode.Exception:
            case StatusCode.ExceptionOnConnect:
            case StatusCode.SecurityExceptionOnConnect:
            case StatusCode.TimeoutDisconnect:
                StopAllCoroutines();
                connected = false;
                break;
        }
    }
    #endregion
}
