using MMOServer.Arena;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMOServer.Peer
{
    public class ActorPeer : PeerBase
    {
        private static long lastAssignedID = long.MinValue;
        private static object allocateIDLock = new object();
        public long playerID;

        private System.Random rand = new Random();
        private IRpcProtocol rpcProtocol;
        private IPhotonPeer photonPeer;

        protected ActorPeer(InitRequest initRequest)
            : base(initRequest)
        {
            
        }

        public ActorPeer(IRpcProtocol protocol, IPhotonPeer unmanagedPeer)
            : base(protocol, unmanagedPeer)
        {
            #region Assigned ID
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
            #endregion

            #region Join to Arena
            lock (ServerArena.Instance)
            {
                ServerArena.Instance.Enter(this);
            }
            #endregion
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            lock (ServerArena.Instance)
            {
                ServerArena.Instance.Exit(this);
            }
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            ServerArena.Instance.OnOperationRequest(this, operationRequest, sendParameters);
        }
                                       
    }
}
