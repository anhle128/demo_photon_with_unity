using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Actor
{
    public PeerBase owner; // the Peer that owns this actor, or NULL if actor is owned by server
    public long actorID; // the ID of this actor instance
    public byte actorType; // the type of this actor (player, star, etc)
    public float posX; // the world X position of this actor
    public float posY; // the world Y position of this actor
    public float radius; // the collision radius of this actor
}
