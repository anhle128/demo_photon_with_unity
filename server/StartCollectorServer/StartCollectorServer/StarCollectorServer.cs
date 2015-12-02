using Photon.SocketServer;
using StartCollectorServer.Peer;
using System;
using System.Collections.Generic;
using System.Linq;

public class StarCollectorServer : ApplicationBase
{

    protected override PeerBase CreatePeer(InitRequest initRequest)
    {
        return new StarCollectorPeer(initRequest.Protocol, initRequest.PhotonPeer);
    }

    protected override void Setup()
    {
        StarCollectorGame.Instance = new StarCollectorGame();
        StarCollectorGame.Instance.Startup();
    }

    protected override void TearDown()
    {
        StarCollectorGame.Instance.ShutDown();
    }
}
