using ProtoBuf;

[ProtoContract]
public class MPosition
{
    [ProtoMember(1)]
    public float x;
    [ProtoMember(2)]
    public float y;
}
