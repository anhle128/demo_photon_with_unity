using ProtoBuf;
using System;
using System.Collections.Generic;

namespace LMLiblary.Model
{
    [ProtoContract]
    public class MPlayer
    {
        [ProtoMember(1)]
        public string name;
        [ProtoMember(2)]
        public int id;

        public byte[] Serialize() 
        {
            return General.GeneralFunc.Serialize(this);
        }

        public MPlayer Deserialize(byte[] arrBytes) 
        {
            return General.GeneralFunc.Deserialize<MPlayer>(arrBytes);
        }
    }
}
