using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

[ProtoContract]
public class Movement
{
    [ProtoMember(1)]
    public long actorID;
    [ProtoMember(2)]
    public float posX;
    [ProtoMember(3)]
    public float posY;
}
