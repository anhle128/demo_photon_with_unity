using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using LMLiblary.Model;

public class StarCollectorClient : MonoBehaviour, IPhotonPeerListener
{

    public static PhotonPeer connection;
    public static bool connected = false;
    public static long playerID;

    public string ServerIP = "10.0.0.115:5055";
    public string AppName = "StarCollectorServer";

    public GameObject playerPref;
    public GameObject starPref;

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
                Debug.Log("CreateActor");
                ActorType actorType = (ActorType)eventData.Parameters[ 0 ];
                long actorID = (long)eventData.Parameters[ 1 ];
                float posX = (float)eventData.Parameters[ 2 ];
                float posY = (float)eventData.Parameters[ 3 ];
                GameObject actor = null;
                switch (actorType) 
                {
                    case ActorType.Player:
                        long ownerID = (long)eventData.Parameters[4];
                        Debug.Log("ownerID " + ownerID);
                        actor = (GameObject)Instantiate(playerPref, new Vector3(posX, 1.5f, posY), Quaternion.identity);
                        actor.GetComponent<OwnerInfo>().SetOwnerID(ownerID);
                        break;
                    case ActorType.Star:
                        actor = (GameObject)Instantiate(starPref, new Vector3(posX, 1.5f, posY), Quaternion.identity);
                        break;
                }
                actor.GetComponent<GameActor>().SetActorID(actorID);
                //if(eventData.Parameters.ContainsKey((byte)ActorType.Player))
                //{
                //    Debug.Log("vao day");
                //    Debug.Log(eventData.Parameters.Count);
                //    foreach (var item in eventData.Parameters)
                //    {
                //        Debug.Log(string.Format("key {0} - value {1}",item.Key,item.Value));
                //    }
                //    //List<MPlayer> listPlayer = eventData.Parameters[(byte)ActorType.Player] as List<MPlayer>;
                //    //Debug.Log("size player :" + listPlayer.Count);
                //}
                break;
            // destroy actor
            case AckEventType.DestroyActor:
                GameActor destroyActor = GameActor.dictActor[(long)eventData.Parameters[0]];
                if(destroyActor != null)
                {
                    destroyActor.Destruct();
                }
                break;
            // update actor
            case AckEventType.UpdateActor:
                GameActor updateActor = GameActor.dictActor[(long)eventData.Parameters[0]];
                float newPosX = (float)eventData.Parameters[ 1 ];
                float newPosY = (float)eventData.Parameters[ 2 ];
                updateActor.GetComponent<ObjPlayer>().UpdatePosition(new Vector3(newPosX, 0f, newPosY));
                break;
            // log chat messages
            case AckEventType.ChatMessage:
                Debug.Log( (string)eventData.Parameters[ 0 ] );
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
                Debug.Log("Connected, awaiting player ID...");
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
