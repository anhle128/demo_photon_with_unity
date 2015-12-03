using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using PhotonHostRuntimeInterfaces;
public class StarCollectorPeer : PeerBase
{

    private static long lastAssignedID = long.MinValue;
    private static object allocateIDLock = new object();
    public long playerID;

    private System.Random rand = new Random();
    private IRpcProtocol rpcProtocol;
    private IPhotonPeer photonPeer;

    public StarCollectorPeer(InitRequest initRequest)
        : base(initRequest)
    {
        
    }

    public StarCollectorPeer(IRpcProtocol protocol, IPhotonPeer unmanagedPeer)
        : base(protocol, unmanagedPeer)
    {
        lock (StarCollectorGame.Instance)
        {
            StarCollectorGame.Instance.PeerJoined(this);
        }

        lock (allocateIDLock)
        {
            playerID = lastAssignedID;
            lastAssignedID++;
        }

        //notify player of their ID
        EventData evt = new EventData() 
        { 
            Code = (byte)AckEventType.AssignPlayerID,
            Parameters = new Dictionary<byte, object>() 
            { 
                { (byte)EventParameter.PlayerID, this.playerID } 
            } 
        };
        evt.Parameters[(byte)EventParameter.PlayerID] = playerID;
        this.SendEvent(evt, new SendParameters());
    }

    protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
    {
        lock (StarCollectorGame.Instance)
        {
            StarCollectorGame.Instance.PeerLeft(this);
        }
    }

    protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
    {
        StarCollectorGame.Instance.OnOperationRequest(this, operationRequest, sendParameters);
    }
}
