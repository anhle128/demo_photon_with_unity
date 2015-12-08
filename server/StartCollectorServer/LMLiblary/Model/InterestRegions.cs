using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class InterestRegions
{
    [ProtoMember(1)]
    public List<MPosition> listRegion;
    [ProtoMember(2)]
    public MPosition playerPosition;
}
