using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using GameSever.Data;
using System.Reflection;

namespace GameSever.Serializable
{
    class Serial
    {
        public byte[] Encode(object data)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, data);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("[Serializable] Encode 序列化:" + e.Message);
            }
            return stream.ToArray();
        }
        public object Decode(byte[] bytes, int start, int len)
        {
            byte[] readBuff = new byte[len];
            Array.Copy(bytes, start, readBuff, 0, len);
            object message = new object();
            //message.data = new MessageData.Data();
            MemoryStream stream = new MemoryStream(readBuff);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new UBinder();
                message = formatter.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("[Serializable] Decode 反序列化:" + e.Message);
            }
            return message;
        }

        internal class UBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                return ass.GetType(typeName);
            }
        }
    }
}
