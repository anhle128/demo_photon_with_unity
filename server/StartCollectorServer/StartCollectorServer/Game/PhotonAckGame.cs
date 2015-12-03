using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class PhotonAckGame
{
    public static PhotonAckGame Instance;

    public List<PeerBase> Connections;

    public void Startup() 
    {
        Connections = new List<PeerBase>();
    }

    public void ShutDown() 
    {
        foreach (var peer in Connections)
        {
            peer.Disconnect();
        }
    }

    public void PeerConnected(PeerBase peer) 
    {
        lock(Connections)
        {
            Connections.Add(peer);
        }
    }

    public void PeerDisconnected(PeerBase peer) 
    {
        lock (Connections) 
        {
            Connections.Remove(peer);
        }
    }

    public void OnOperationRequest(PeerBase src, OperationRequest  request, SendParameters sendParams)
    {
        // send ack to peer
        src.SendOperationResponse(new OperationResponse(
        (byte)AckOperationTypes.Ack), sendParams);
    }
}
