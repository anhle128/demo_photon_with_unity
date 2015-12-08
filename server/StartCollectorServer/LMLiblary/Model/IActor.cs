using ProtoBuf;

[ProtoContract]
public class IActor
{
    [ProtoMember(1)]
    public long peerID; // the id Peer that owns this actor, or NULL if actor is owned by server
    [ProtoMember(2)]
    public long actorID; // the ID of this actor instance
    [ProtoMember(3)]
    public byte actorType; // the type of this actor (player, star, etc)
    [ProtoMember(4)]
    public float posX; // the world X position of this actor
    [ProtoMember(5)]
    public float posY; // the world Y position of this actor
    [ProtoMember(6)]
    public Index index; // the world Y position of this actor
}
