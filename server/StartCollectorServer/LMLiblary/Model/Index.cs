using ProtoBuf;

[ProtoContract]
public class Index
{
    [ProtoMember(1)]
    public int x;
    [ProtoMember(2)]
    public int y;
}
