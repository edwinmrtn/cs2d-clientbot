using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cs2dClientBot.Map
{
    public class MapParser
    {
        string mapName;
        MapReader r;
        static string maxHeader = "Unreal Software's Counter-Strike 2D Map File (max)";
        static string oldHeader = "Unreal Software's Counter-Strike 2D Map File";
        static string mapCheck = "ed.erawtfoslaernu";
        string mapHeader;
        public string version;
        string code;
        string tileset;
        byte loaded;
        int mx;
        int my;
        string background;
        int backgroundx;
        int backgroundy;
        byte red;
        byte green;
        byte blue;
        string check;
        byte[] tilemode;
        byte[,] tilemap;
        byte[,] tilemodemap;
        int numberOfEntities;     


        public MapParser(string mapName)
        {
            try
            {
                this.mapName = mapName;
                FileStream readStream = new FileStream(Directory.GetCurrentDirectory() + "\\maps\\" + mapName + ".map", FileMode.Open);
                r = new MapReader(readStream);
            }
            catch (FileNotFoundException e)
            {
                throw new Exception("map not found!");
            }
        }

        public byte[,] Tilemap
        {
            get { return tilemap; }
        }

        public byte[,] Tilemodemap
        {
            get { return tilemodemap; }
        }

        public int mapWidth
        {
            get { return mx + 1; }
        }

        public int mapHeight
        {
            get { return my + 1; }
        }

        public string Tileset
        {
            get { return tileset; }
        }

        public string Background
        {
            get
            {
                if (!background.Equals(""))
                    return background;
                else
                    return "sand1.jpg";
            }
        }
        public bool Parse()
        {
            if (!CheckHeader())
                return false;
            CodedInformation();
            if (!MapCheck())
                return false;
            getTiles();                
            r.Close();
            return true;           
        }

        public int TileCount
        {
            get { return (int)loaded; }
        }

        private bool CheckHeader()
        {
            mapHeader = r.ReadString();
            if (mapHeader.Equals(oldHeader))
            {
                for (int i = 1; i <= 9; i++)
                    r.ReadByte();
                for (int i = 1; i <= 10; i++)
                    r.ReadInt32();
                for (int i = 1; i <= 10; i++)
                    r.ReadString();
                version = "legacy";
            }
            else if (mapHeader.Equals(maxHeader))
            {
                for (int i = 1; i <= 10; i++)
                    r.ReadString();
                version = "max";
            }
            else
            {
                return false;
            }
            return true;
        }

        private void CodedInformation()
        {
            code = r.ReadString();
            tileset = r.ReadString();
            loaded = r.ReadByte();
            mx = r.ReadInt32();
            my = r.ReadInt32();
            background = r.ReadString();
            backgroundx = r.ReadInt32();
            backgroundy = r.ReadInt32();
            red = r.ReadByte();
            green = r.ReadByte();
            blue = r.ReadByte();
        }

        private bool MapCheck()
        {
            check = r.ReadString();
            if (!check.Equals(mapCheck))
                return false;
            else
            {
                tilemode = new byte[loaded];
                for (int i = 0; i < loaded; i++)
                    tilemode[i] = r.ReadByte();
            }
            return true;
        }

        private void getTiles()
        {
            r.ReadByte();
            tilemap = new byte[mx + 1, my + 1];
            tilemodemap = new byte[mx + 1, my + 1];
            for (int x = 0; x <= mx; x++)
            {
                for (int y = 0; y <= my; y++)
                {
                    byte _b_ = r.ReadByte();
                    if (_b_ > loaded)
                        _b_ = 0;
                    tilemap[x, y] = _b_; // inversed
                    byte _m_ = tilemode[_b_];
                    tilemodemap[x, y] = _m_; // inversed
                }
            }
        }

        /*private void getEntities()
        {
            numberOfEntities = r.ReadInt32();
            entities = new Entity[numberOfEntities];
            for (int i = 0; i < numberOfEntities; i++)
            {
                string name = r.ReadString();
                byte type = r.ReadByte();
                int x = r.ReadInt32();
                int y = r.ReadInt32();
                string trig = r.ReadString();
                entity_arg[] _args_ = new entity_arg[10];
                for (int j = 0; j < 10; j++)
                {
                    entity_arg a = new entity_arg(r.ReadInt32(), r.ReadString());
                    _args_[j] = a;
                }
                Entity ent = new Entity(name, type, x, y, trig, _args_);
                entities[i] = ent;
            }
        }*/

        /*public void printMap()
        {
            TextWriter tw = new StreamWriter("C:\\Users\\Michiel\\Desktop\\Cs2d 17\\maps\\" + mapName + "_mapogram.txt");
            for (int y = 0; y <= my; y++)
            {
                for (int x = 0; x <= mx; x++)
                {
                    byte b = tilemodemap[x, y];
                    if (b == 1 || b == 2 || b == 3 || b == 4)
                        tw.Write("#");
                    else
                        tw.Write(" ");
                }
                tw.WriteLine();
            }
            tw.WriteLine();

            foreach (Entity en in entities)
            {
                tw.Write("name :" + en.name + " trig :" + en.trig + " x: " + en.x + " y: " + en.y);
                tw.WriteLine();
            }
            tw.Close();
        }*/

    }
}
