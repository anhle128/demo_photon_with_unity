using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using LMLiblary.Model;
using LMLiblary.General;
using System.Linq;

public class StarCollectorClient : MonoBehaviour, IPhotonPeerListener
{

    public static PhotonPeer connection;
    public static bool connected = false;
    public static long playerID;

    public string ServerIP = "10.0.0.115:5055";
    public string AppName = "StarCollectorServer";

    public GameActor playerPref;
    public GameActor creepPref;



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
        Debug.Log((AckEventType)eventData.Code);
        //server raised an event
        switch ((AckEventType)eventData.Code)
        {
            // store player ID
            case AckEventType.AssignPlayerID:
                IActor player = GeneralFunc.Deserialize<IActor>(eventData.Parameters[0] as byte[]);
                GameActor playerCreated = Instantiate(playerPref, new Vector3(player.posX, 0.5f, player.posY), playerPref.transform.rotation) as GameActor;
                playerCreated.SetActor(player);
                playerCreated.GetComponent<OwnerInfo>().SetOwnerID(player.peerID);
                StarCollectorClient.playerID = player.peerID;
                playerCreated.GetComponent<CheckRegion>().Load();
                break;
            // create actor
            case AckEventType.CreateActor:
                if(eventData.Parameters.ContainsKey((byte)Parameter.Creep))
                {
                    List<IActor> listAllCreep = GeneralFunc.Deserialize<List<IActor>>(eventData.Parameters[(byte)Parameter.Creep] as byte[]);

                    List<IActor> listSameCreep = GetListSameActor(listAllCreep, GameActor.listCreep);

                    List<IActor> listNewCreep = listAllCreep.Except(listSameCreep).ToList();

                    List<GameActor> listDeleteCreep = GameActor.listCreep.Where(a => !listAllCreep.Any(b => b.actorID == a.actor.actorID)).ToList();

                    foreach (var creepDelete in listDeleteCreep)
                    {
                        creepDelete.Destruct();
                    }

                    foreach (var creep in listNewCreep)
                    {
                        GameActor creepCreated = Instantiate(creepPref, new Vector3(creep.posX, 0.5f, creep.posY), playerPref.transform.rotation) as GameActor;
                        creepCreated.SetActor(creep);
                        creepCreated.GetComponent<CheckRegion>().Load();
                    }
                }
                if (eventData.Parameters.ContainsKey((byte)Parameter.Regions))
                {
                    InterestRegions dataInterestRegion = GeneralFunc.Deserialize<InterestRegions>(eventData.Parameters[(byte)Parameter.Regions] as byte[]);

                    if (dataInterestRegion.playerPosition != null)
                        Debug.Log(string.Format("player position -------------- x: {0} - y: {1}", dataInterestRegion.playerPosition.x, dataInterestRegion.playerPosition.y));

                    foreach (var data in dataInterestRegion.listRegion)
                    {
                        Debug.Log(string.Format("x: {0} - y: {1}",data.x,data.y));
                    }
                }
                break;
            case AckEventType.DestroyActor:
                if (eventData.Parameters.ContainsKey((byte)Parameter.Data))
                {
                    IActor playerDestroy = GeneralFunc.Deserialize<IActor>(eventData.Parameters[(byte)Parameter.Data] as byte[]);
                    GameActor gameActorDestroy = GameActor.listPlayer.Where(a => a.actor.actorID == playerDestroy.actorID).FirstOrDefault();
                    if(gameActorDestroy != null)
                    {
                        gameActorDestroy.Destruct();
                    }
                }
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

    private List<IActor> GetListSameActor(List<IActor> listActor, List<GameActor> listGameActor) 
    {
        var result = from a in listActor
                                  join b in listGameActor on a.actorID equals b.actor.actorID
                                  select a;
        return result.ToList();
    }

    //private List<IActor> GetListNewActor(List<IActor> listActor, List<GameActor> listGameActor) 
    //{

    //    return listResult;
    //}
}
