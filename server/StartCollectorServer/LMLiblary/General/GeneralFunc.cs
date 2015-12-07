using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LMLiblary.General
{
    public class GeneralFunc
    {
        public static byte[] Serialize<T>(T data)
        {
            byte[] result;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, data);
                result = stream.ToArray();
            }
            return result;
        }

        public static T Deserialize<T>(byte[] arrBytes)
        {
            T dataResult = default(T);
            using (var stream = new MemoryStream(arrBytes))
            {
                dataResult = Serializer.Deserialize<T>(stream);
            }
            return dataResult;
        }
    }
}
