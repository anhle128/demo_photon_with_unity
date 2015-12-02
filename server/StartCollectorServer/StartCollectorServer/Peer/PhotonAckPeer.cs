using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using PhotonHostRuntimeInterfaces;
namespace StartCollectorServer.Peer
{
    public class PhotonAckPeer : PeerBase
    {
        private static long lastAssignedPlayerID = long.MinValue;
        private static object lockPlayerID = new object();

        public long playerID;

        public PhotonAckPeer(IRpcProtocol protocol, IPhotonPeer unmanagedPeer)
            : base(protocol, unmanagedPeer)
        {
            lock (lockPlayerID) 
            {
                this.playerID = lastAssignedPlayerID;
                lastAssignedPlayerID++;
            }
            PhotonAckGame.Instance.PeerConnected(this);

            EventData evn = new EventData((byte)AckEventType.AssignPlayerID)
            { 
                Parameters = new Dictionary<byte, object>() 
                { 
                    { (byte)EventParameter.PlayerID, this.playerID } 
                } 
            };
            Console.WriteLine("PhotonAckPeer PlayerID: " + this.playerID);
            this.SendEvent(evn,new SendParameters());
            Console.WriteLine("PhotonAckPeer SendEvent");
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            PhotonAckGame.Instance.PeerDisconnected(this);
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            PhotonAckGame.Instance.OnOperationRequest(this, operationRequest, sendParameters);
        }
    }
}
