using System;
using System.Linq;
using System.Text;
using GameSever.Serializable;

namespace GameSever.Core
{
    [Serializable]
    class ProtocolBytes : ProtocolBase
    {
        public byte[] bytes;
        private int start;
        private int end;

        public ProtocolBytes()
        {
            start = 0;
            end = 0;
        }

        public override ProtocolBase Decode(byte[] readBuffer, int start, int length)
        {
            ProtocolBytes protocolBytes = new ProtocolBytes();
            protocolBytes.bytes = new byte[length];
            Array.Copy(readBuffer, start, protocolBytes.bytes,0,length);
            return protocolBytes;
        }
        public override byte[] Encode()
        {
            return bytes;
        }
        public override string GetName()
        {
            return GetString(0);
        }
        public override string GetDesc()
        {
            string str = "";
            if (bytes == null) return str;
            for (int i = 0;i<bytes.Length;i++)
            {
                int b = (int)bytes[i];
                str += b.ToString() + "";
            }
            return str;
        }
        public void AddString(string str)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(str);
            Int32 len = strBytes.Length;
            byte[] lenBytes = BitConverter.GetBytes(len);
            if (bytes == null)
                bytes = lenBytes.Concat(strBytes).ToArray();
            else
                bytes = bytes.Concat(lenBytes).Concat(strBytes).ToArray();

        }
        public string GetString(ref int start , ref int end)
        {
            if (bytes == null)
                return "";
            if (bytes.Length < start + sizeof(Int32))
                return "";
            Int32 strLen = BitConverter.ToInt32(bytes, start);
            if (bytes.Length < start + sizeof(Int32) + strLen)
                return "";
            string str = Encoding.UTF8.GetString(bytes,start + sizeof(Int32),strLen);
            end = start + sizeof(Int32) + strLen;
            start = end;
            return str;
        }

        public string GetString()
        {
            return GetString(ref start, ref end);
        }

        public string GetString(int start)
        {
            int end = 0;
            return GetString( ref start,ref end);
        }
        public void AddInt(int num)
        {
            byte[] numBytes = BitConverter.GetBytes(num);
            if (bytes == null)
                bytes = numBytes;
            else
                bytes = bytes.Concat(numBytes).ToArray();
        }

        public int GetInt(ref int start, ref int end)
        {
            if (bytes == null)
                return 0;
            if (bytes.Length < start + sizeof(Int32))
                return 0;
            end = start + sizeof(Int32);
            int value = BitConverter.ToInt32(bytes, start);
            start = end;
            return value;
        }

        public int GetInt(int start)
        {
            int end = 0;
            return GetInt(ref start, ref end);
        }

        public int GetInt()
        {
            return GetInt( ref start, ref end);
        }

        public void AddFloat(float num)
        {
            byte[] numBytes = BitConverter.GetBytes(num);
            if (bytes == null)
                bytes = numBytes;
            else
                bytes = bytes.Concat(numBytes).ToArray();
        }

        public float GetFloat(int start, ref int end)
        {
            if (bytes == null)
                return 0;
            if (bytes.Length < start + sizeof(float))
                return 0;
            end = start + sizeof(float);
            return BitConverter.ToSingle(bytes, start);
        }

        public float GetFloat(int start)
        {
            int end = 0;
            return GetFloat(start, ref end);
        }

        //public bool serializeObjToStr(Object obj, out string serializedStr)
        //{
        //    bool serializeOk = false;
        //    try
        //    {
        //        MemoryStream memoryStream = new MemoryStream();
        //        BinaryFormatter binaryFormatter = new BinaryFormatter();
        //        binaryFormatter.Serialize(memoryStream, obj);
        //        serializedStr = System.Convert.ToBase64String(memoryStream.ToArray());

        //        serializeOk = true;
        //    }
        //    catch
        //    {
        //        serializeOk = false;
        //    }

        //    return serializeOk;
        //}

        //public static byte[] GetBytes<TStruct>(TStruct subCmd) where TStruct : struct
        //{
        //    int structSize = Marshal.SizeOf(typeof(TStruct));
        //    byte[] buffer = new byte[structSize];
        //    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        //    Marshal.StructureToPtr(subCmd, handle.AddrOfPinnedObject(), false);
        //    handle.Free();
        //    return buffer;
        //}
    }
}
