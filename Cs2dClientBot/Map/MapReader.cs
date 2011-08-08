using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cs2dClientBot.Map
{
    public class MapReader : BinaryReader
    {
        Stream mapStream;

        public MapReader(Stream file)
            : base(file, Encoding.ASCII)
        {
            this.mapStream = file;
        }

        public override String ReadString()
        {
            int currentPosition = (int)mapStream.Position;
            int enterPlace = 0;
            for (int i = 0; i < 100; i++)
            {
                if (this.ReadChar() == 0x0A)
                {
                    enterPlace = i;
                    break;
                }
            }
            string line = "";
            mapStream.Position = currentPosition;
            line = new string(this.ReadChars(enterPlace - 1));
            this.ReadByte();
            this.ReadByte();
            return line;
        }
    }
}
