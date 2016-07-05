using MMOServer.Arena;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;

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
