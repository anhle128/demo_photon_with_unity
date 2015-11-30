using Photon.SocketServer;
using StartCollectorServer.Peer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StartCollectorServer
{
    public class StarCollectorServer : ApplicationBase
    {

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return new PhotonAckPeer(initRequest.Protocol, initRequest.PhotonPeer);
        }

        protected override void Setup()
        {
            throw new NotImplementedException();
        }

        protected override void TearDown()
        {
            throw new NotImplementedException();
        }
    }
}
