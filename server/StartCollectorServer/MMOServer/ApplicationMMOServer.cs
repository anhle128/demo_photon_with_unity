using MMOServer.Arena;
using MMOServer.Peer;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;

public class ApplicationMMOServer : ApplicationBase
{
    protected override PeerBase CreatePeer(InitRequest initRequest)
    {
        return new ActorPeer(initRequest.Protocol, initRequest.PhotonPeer);
    }

    protected override void Setup()
    {
        ServerArena.Instance = new ServerArena();
        ServerArena.Instance.Startup();
    }

    protected override void TearDown()
    {
        ServerArena.Instance.ShutDown();
    }
}
