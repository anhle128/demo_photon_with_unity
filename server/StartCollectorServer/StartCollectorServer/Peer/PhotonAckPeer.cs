using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using PhotonHostRuntimeInterfaces;
namespace StartCollectorServer.Peer
{
    public class PhotonAckPeer : PeerBase
    {
        private IRpcProtocol rpcProtocol;
        private IPhotonPeer photonPeer;


        public PhotonAckPeer(IRpcProtocol protocol, IPhotonPeer unmanagedPeer)
            : base(protocol, unmanagedPeer)
        {
            
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            throw new NotImplementedException();
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            // send an "ack" to client
            OperationResponse response = new OperationResponse((byte)PhotonAckResponseTypes.Ack);
            this.SendOperationResponse(response, sendParameters);
        }
    }
}
