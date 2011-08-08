using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cs2dClientBot
{
    /// <summary>
    /// Based on 3rr0r's bb_udp.py
    /// 
    /// </summary>
    public class PacketStream
    {
        static Encoding enc = Encoding.GetEncoding("Latin1");
        private List<byte> buf;
        private int cursor;
        private byte[] byteData;
        private int length;
        
        /// <summary>
        /// This constructor is used to generate packets
        /// </summary>
        public PacketStream()
        {
            buf = new List<byte>();
        }

        /// <summary>
        /// This packet is used to read packets
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="length"></param>
        public PacketStream(byte[] byteData, int length)
        {
            cursor = 0;
            this.byteData = byteData;
            this.length = length;
        }

        public void setPacketNumber(short value) // mind the endians
        {
            byte[] b = System.BitConverter.GetBytes(value);
            
            if (buf != null)
            {
                buf[0] = b[0];
                buf[1] = b[1];
            }
            else
            {
                byteData[0] = b[0];
                byteData[1] = b[1];
            }
        }

        public short getPacketNumber()
        {
            if (byteData != null)
                return BitConverter.ToInt16(byteData, 0);
            else if (buf != null)
                return BitConverter.ToInt16(buf.ToArray(), 0);
            else
                return 0;
        }

        public int Length
        {
            get
            {
                if (length == 0)
                    return buf.Count;
                else
                    return length;
            }
        }

        #region WriteStuff


        public void WriteByte(int b)
        {
            if (b < 0) { b = 0; }
            else if (b > 255) { b = 255; }
            buf.Add((byte)b);
        }

        public void WriteByte(byte b)
        {
            buf.Add((byte)b);
        }
        public void WriteShort(int s)
        {
            byte[] b = System.BitConverter.GetBytes((short)s);
            buf.AddRange(b);
        }
        public void WriteInt(uint i)
        {
            byte[] b = System.BitConverter.GetBytes(i);
            buf.AddRange(b);
        }
        public void WriteFloat(float f)
        {
            byte[] b = System.BitConverter.GetBytes(f);
            buf.AddRange(b);
        }
        public void WritePureString(string s)
        {
            byte[] b = enc.GetBytes(s);
            buf.AddRange(b);
        }
        public void WriteLine(string s)
        {
            byte[] b = enc.GetBytes(s + "\r\n");
            buf.AddRange(b);
        }
        public void WriteString(string s)
        {
            this.WriteByte(s.Length);
            this.WritePureString(s);
        }

        #endregion

        #region ReadStuff

        public bool AtEnd()
        {
            if (this.cursor < this.length)
                return false;
            else
                return true;
        }

        public void SkipAll()
        {
            this.cursor = this.length;
        }

        public void Skip(int count)
        {
            this.cursor += count;
        }

        public short ReadShort()
        {
            short ret = BitConverter.ToInt16(byteData, cursor);
            cursor += 2;
            return ret;
        }
        public int ReadInt()
        {
            int ret = BitConverter.ToInt32(byteData, cursor);
            cursor += 4;
            return ret;
        }
        public float ReadFloat()
        {
            float ret = BitConverter.ToSingle(byteData, cursor);
            cursor += sizeof(float);
            return ret;
        }
        public byte ReadByte()
        {
            byte ret = byteData[cursor];
            cursor += 1;
            return ret;
        }

        public string ReadLine()
        {
            string ret = "";
            for (int i = cursor; i <= this.length; i++)
            {
                if (byteData[i] == (byte)'\r')
                {
                    ret += enc.GetString(byteData, cursor, i - cursor);
                    cursor += (ret.Length + 2);
                    break;
                }
            }
            return ret;
        }

        public string ReadString(int count)
        {
            string ret = enc.GetString(byteData, cursor, count);
            cursor += count;
            return ret;
        }

        public string ReadString()
        {
            int count = (int)ReadByte();
            /*  if (count == 0)
                  return "";*/
            string ret = enc.GetString(byteData, cursor, count);
            cursor += count;
            return ret;
        }
        #endregion

        public byte getType()
        {
            if (byteData != null)
                return byteData[2];
            else
                return buf[2];
        }

        public void ResetCursor()
        {
            cursor = 0;
        }
        public byte[] toArray()
        {
            if (buf != null)
                return buf.ToArray();
            else
                return byteData;
        }

        public override String ToString()
        {
            String s = "";
            if (byteData != null)
            {
                //return enc.GetString(byteData);
                for (int i = 0; i < this.length; i++)
                {
                    s += byteData[i] + " ";
                }
            }
            else
            {
                for (int i = 0; i < buf.Count; i++)
                {
                    s += buf[i] + " ";
                }
                // return enc.GetString(buf.ToArray());
            }
            return s;
        }

        public static PacketStream Copy(PacketStream s)
        {
            return (PacketStream)s.MemberwiseClone();
        }
    }
}
