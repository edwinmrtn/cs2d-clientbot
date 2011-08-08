using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Cs2dClientBot.Map;

namespace Cs2dClientBot
{
    /// <summary>
    /// This class determines what must happen when packets are received
    /// but also provides methods to actually send packets.
    /// </summary>
    public class PacketHandler
    {
        private short packetNumber;
        private ConnectionHandler ch;
        private BotLogics botLogics;
        private Form1 form;
        private PlayerObject local_player;
        private Client cl;
        private int own_id;
        private PlayerObject[] players;
        delegate void LogTextDelegate(string newText);
        private String currentMapName;
        private MapParser mappie;

        private delegate int KnownHandler(PacketStream ps);
        private delegate int UnknownHandler(PacketStream ps);


        private KnownHandler[] known_table;

        private short packetNumberExpected;

        public PacketHandler(ConnectionHandler ch, Form1 f, PlayerObject local, PlayerObject[] pl, Client client)
        {
            this.packetNumber = 0;
            this.ch = ch;
            this.form = f;
            this.local_player = local;
            players = pl;
            known_table = new KnownHandler[255];
            cl = client;           
            
            known_table[1] = handshake;
            known_table[2] = failed_handshake;
            known_table[7] = fire;
            known_table[8] = advancedfire;
            known_table[9] = weaponchange;
            known_table[10] = positionUpdate;
            known_table[11] = positionUpdate;
            known_table[12] = rotationUpdate;
            known_table[13] = positionRotationUpdate;
            known_table[14] = positionRotationUpdate;
            known_table[15] = setpos;
            known_table[16] = reload;
            known_table[17] = hit;
            known_table[19] = killmsg;
            known_table[20] = teamchange;
            known_table[21] = spawnmsg;
            known_table[22] = roundstart;
            known_table[23] = weaponbuy;
            known_table[24] = weapondrop;
            known_table[25] = weaponpickup;
            known_table[26] = usepress;
            known_table[27] = projectile;
            known_table[28] = spray;
            known_table[30] = bomb;
            known_table[32] = specpos;
            known_table[33] = spawnitem;

            known_table[41] = unknown1;

            known_table[43] = vote;
            known_table[52] = flare;
            known_table[238] = newname;
            known_table[240] = chatmsg;
            known_table[247] = pinglist;
            known_table[248] = playerjoin;
            known_table[249] = pingstuff;
            known_table[252] = joinroutine_known;
            known_table[253] = playerleave;



        }

        int unknown1(PacketStream ps)
        {            // Dont know!!
            ps.ReadByte();
            ps.ReadByte();
            return 1;
        }
        

        int handshake(PacketStream ps)
        {
            short packetNumConfirmed = ps.ReadShort();

            return 1; // need to check return value
        }

        int failed_handshake(PacketStream ps)
        {
            LogConsole("Reliable packet not received by server!");
            return 1;
        }

        int fire(PacketStream ps)
        {            
            byte id = ps.ReadByte();
            if (id == own_id)
            {
                botLogics.OnFire();
            }
            return 1;    
        }

        int advancedfire(PacketStream ps)
        {           
            byte id = ps.ReadByte();
            byte status = ps.ReadByte();
            players[id].WeaponStatus = status;
            return 1;    
        }

        int weaponchange(PacketStream ps)
        {             
            byte id = ps.ReadByte();
            byte wpn_id = ps.ReadByte();
            byte wpn_status = ps.ReadByte(); // unknown
            //LogConsole(String.Format("{0} changed to weapon {1}", players[id].Name, wpn_id));
            players[id].Currentweapon = wpn_id;
            players[id].WeaponStatus = wpn_status;
            return 1;    
        }

        int positionUpdate(PacketStream ps)
        {
            
            byte id = ps.ReadByte();
            short px = ps.ReadShort();
            short py = ps.ReadShort();
            PlayerObject tmp = players[id];
            tmp.X = px; tmp.Y = py;
            return 1;                
        }

        int rotationUpdate(PacketStream ps)
        {
            byte id = ps.ReadByte();
            float rotation = ps.ReadFloat();
            players[id].Rotation = rotation;

            return 1;
        }

        int positionRotationUpdate(PacketStream ps)
        {
            byte id = ps.ReadByte();
            short x = ps.ReadShort();
            short y = ps.ReadShort();
            float rot = ps.ReadFloat();

            if (id >= 0 && id <= 32)
            {
                PlayerObject tmp = players[id];
                tmp.Id = id;
                tmp.X = x;
                tmp.Y = y;
                tmp.Rotation = rot;
                return 1;
            }
            return 0;
        }

        int setpos(PacketStream ps)
        {
            byte id = ps.ReadByte();
            short x = ps.ReadShort();
            short y = ps.ReadShort();
            players[id].X = x;
            players[id].Y = y;
            return 1;
        }

        int reload(PacketStream ps)
        {
            
            byte id = ps.ReadByte();
            byte status = ps.ReadByte();

            if (status != 1 && id == own_id)
                botLogics.OnReloadFinish();

            return 1;                
        }

        int hit(PacketStream ps)
        {             
            byte victim_id = ps.ReadByte();
            byte source_id = ps.ReadByte();
            byte health = ps.ReadByte();
            byte uk0 = ps.ReadByte();

            PlayerObject tmp = players[victim_id];
            tmp.Health = health;
            return 1;    
        }

        int killmsg(PacketStream ps)
        {
            byte victim = ps.ReadByte();
            byte source = ps.ReadByte();
            byte wpn_id = ps.ReadByte();
            short sx = ps.ReadShort();
            short sy = ps.ReadShort();

            PlayerObject tmp = players[victim];
            tmp.Health = 0;

            if (victim == own_id)
                botLogics.OnDeath();

            if (source == own_id)
                botLogics.OnKill(victim);
            return 1;
        }

        int teamchange(PacketStream ps)
        {
           
            byte id = ps.ReadByte();
            byte new_team = ps.ReadByte();
            byte new_skin = ps.ReadByte();

            PlayerObject tmp = players[id];
            
            LogConsole(String.Format("{0} joins team {1} with skin {2}", tmp.Name, new_team, new_skin));
            
            if (new_team == 1 || new_team == 2)
            {
                tmp.Team = new_team;
                tmp.Skin = new_skin;
            }

            return 1;
                
        }

        int spawnmsg(PacketStream ps)
        {
            byte id = ps.ReadByte();

            short spawnx = ps.ReadShort();
            short spawny = ps.ReadShort();

           // float rot = ps.ReadFloat();

            ps.ReadByte();
            ps.ReadByte();
            ps.ReadByte();
            ps.ReadByte();



            byte wpns = ps.ReadByte(); // 50?

            for (int j = 0; j < wpns; j++)
            {
                ps.ReadByte();
                ps.ReadByte();
            }
            //byte uk = ps.ReadByte(); // 0
            //byte uk2 = ps.ReadByte();
            //short money = ps.ReadShort(); // 200
            //byte numweapons = ps.ReadByte(); // 1

            //LogConsole(String.Format("{0} spawned at {1}/{2}", id, spawnx, spawny));

            if (id > 0 && id < 33)
            {
                PlayerObject tmp = players[id];
                tmp.X = spawnx;
                tmp.Y = spawny;
                return 1;
            }
            else
                return 0;
            //tmp.Currentweapon = wpn;
            //tmp.Money = money;

            //if (id == own_id)
            //{
            //LogConsole(String.Format("{0} spawned with: {1} / {2} / {3} / {4} / {5} {6}", tmp.Name, spawnx, spawny,wpn,rot,wpn,uk,uk2));
            //}
            
        }

        int roundstart(PacketStream ps)
        {
            ps.ReadByte();//unknown
            int count = ps.ReadByte();
            for (int i = 0; i < count; i++)
            {
                byte id = ps.ReadByte();

                if (id == own_id)
                    botLogics.OnRoundStart();

                short sx = ps.ReadShort();
                short sy = ps.ReadShort();
                float rot = ps.ReadFloat();
                byte wpn_id = ps.ReadByte();
                /*if (team == 1)
                    team = 2;
                else if (team == 2)
                    team = 1;*/
                byte uk2 = ps.ReadByte();
                byte uk3 = ps.ReadByte();


                if (id > 0 && id < 33)
                {
                    PlayerObject tmp = players[id];
                    tmp.Id = id;
                    //LogConsole(String.Format("mass spawn {0} wpn:{1} uk:{2} uk:{3}", tmp.Name, wpn_id, uk2, uk3));

                    tmp.X = sx; tmp.Y = sy; tmp.Rotation = rot; tmp.Currentweapon = wpn_id;
                    tmp.Health = 100;
                }
                else
                    return 0;
                
            }

            short money = ps.ReadShort();

            byte wpnCount = ps.ReadByte();
            for (int j = 0; j < wpnCount; j++)
            {
                ps.ReadByte();
            }

            return 1;
        }
                    

        int weaponbuy(PacketStream ps)
        {
            byte player_id = ps.ReadByte();
            byte wpn_id = ps.ReadByte();
            if (player_id == own_id)
            {
                botLogics.OnWeaponBuy(wpn_id);
            }
            short money = ps.ReadShort();
            byte uk0 = ps.ReadByte(); // 0

            players[player_id].Money = money;

            //LogConsole("Weaponbuy: " + wpn_id + " " + money);

            return 1;               
        }

        int weapondrop(PacketStream ps)
        {
            byte id = ps.ReadByte();
            byte wpn_id = ps.ReadByte();
            short ammo_in = ps.ReadShort();
            short ammo = ps.ReadShort();
            byte uk0 = ps.ReadByte();//mode?
            byte uk1 = ps.ReadByte();//mode?
            byte uk2 = ps.ReadByte();//mode?
            int tilex = ps.ReadInt();
            int tiley = ps.ReadInt();
            ps.ReadByte();


            botLogics.OnWeaponDrop((int)wpn_id, tilex, tiley, ammo, ammo_in);

            return 1;           
        }

        int weaponpickup(PacketStream ps)
        {
            byte id = ps.ReadByte();
            byte uk0 = ps.ReadByte(); 
            byte uk1 = ps.ReadByte(); // 2 0
            byte wpn_id = ps.ReadByte();
            short ammon_in = ps.ReadShort();
            short ammo = ps.ReadShort();

            if (id == own_id)
            {
                botLogics.OnWeaponPickup(wpn_id, ammo, ammon_in);
            }
            ps.ReadByte();
            return 1;
        }

        int usepress(PacketStream ps)
        {
            //LogConsole("Using or sth? " + ps);
            byte id = ps.ReadByte();
            byte uk2 = ps.ReadByte();
            ps.ReadByte(); ps.ReadByte(); ps.ReadByte(); ps.ReadByte();
            short x = ps.ReadShort();
            short y = ps.ReadShort();
            //LogConsole("Defuse bomb: " + ps.ToString());
            return 1;
        }

        int projectile(PacketStream ps)
        {
            byte player_id = ps.ReadByte();//one who throws the nade
            byte wpn_id = ps.ReadByte();
            short x_origin = ps.ReadShort();
            short y_origin = ps.ReadShort();
            float angle = ps.ReadFloat();
            byte uk0 = ps.ReadByte();
            ps.ReadByte(); // discard 0
            return 1;
        }

        int spray(PacketStream ps)
        {
            
            int tt = ps.ReadByte();
            byte id = ps.ReadByte();
            short x = ps.ReadShort();
            short y = ps.ReadShort();
            byte c = ps.ReadByte();
            //LogConsole(tt + " " + players[id].Name + " sprayed at " + x + " " + y + " with " + c);
            ps.SkipAll(); // i dont wanna see there
            return 1;
                
        }

        int bomb(PacketStream ps)
        {
            byte id = ps.ReadByte();
            byte typ = ps.ReadByte();
            ps.SkipAll(); // needs to be fixed
            return 1;
        }

        int specpos(PacketStream ps)
        {
            ps.ReadByte(); ps.ReadByte(); ps.ReadByte();
            return 1;
        }

        int spawnitem(PacketStream ps)
        {
            byte wpn_id = ps.ReadByte();
            short tilex = ps.ReadShort();
            short tiley = ps.ReadShort();
            short uk1 = ps.ReadShort();
            return 1;
        }

        int vote(PacketStream ps)
        {
            byte typ = ps.ReadByte();
            string data = ps.ReadString();
            botLogics.OnVote(data);           
            return 1;
        }

        int flare(PacketStream ps)
        {
            ps.Skip(12);
            return 1;
        }

        int newname(PacketStream ps)
        {
            byte id = ps.ReadByte();
            string new_name = ps.ReadString();
            players[id].Name = new_name;
            return 1;
        }

        int chatmsg(PacketStream ps)
        {
            byte id = ps.ReadByte();
            byte uk2 = ps.ReadByte();
            byte strlen = ps.ReadByte();
            ps.ReadByte();
            string txt = ps.ReadString(strlen);

            if (id != 0 && players[id] != null)
                LogConsole(String.Format("[P] {0}: {1}", players[id].Name, txt));
            else
                LogConsole(String.Format("[U]: {0}", txt));
            return 1;
        }

        int pinglist(PacketStream ps)
        {
            byte count = ps.ReadByte();
            for (int i = 0; i < count; i++)
            {
                byte id = ps.ReadByte();
                ushort ping = (ushort)ps.ReadShort();
            }
            return 1;
        }

        int playerjoin(PacketStream ps)
        {
            byte id = ps.ReadByte();
            PlayerObject temp = players[id];
            temp.Name = ps.ReadString();
            ps.ReadByte();
            LogConsole(String.Format("[P] {0} connected!", temp.Name));
            return 1;
        }

        int pingstuff(PacketStream ps)
        {
            uint value = (uint)ps.ReadInt();
            send_ping_249(value);
            return 1;
        }

        int joinroutine_known(PacketStream ps)
        {
            int type2 = ps.ReadByte();
            if (type2 == 0)
            {
                String serverKey = ps.ReadString();
                send_join_packet_252_1(local_player, serverKey);
            }
            else if (type2 == 2)
            {
                byte state = ps.ReadByte();
                if (state == 0)
                {
                    own_id = ps.ReadByte();
                    String mapName = ps.ReadString();
                    String skey = ps.ReadString();
                    send_map_confirmation(mapName, skey);
                }
                else
                {
                    if (state == 1)
                        LogConsole("Error: wrong password!");
                    else if (state == 3)
                        LogConsole("Error: server is full!");
                    else if (state == 4)
                        LogConsole("Error: banned!");
                    else if (state == 23)
                        LogConsole("Error: maximum number of clients reached!");
                    else if (state == 22)
                        LogConsole("Error: invalid precon send to server!");
                    else
                        LogConsole("Error on joining: " + state);

                    cl.Close();
                }
            }
            else if (type2 == 3)
            {

            }
            else if (type2 == 4)
            {
                ps.ReadByte();
                send_map_name_252_05();
            }
            else if (type2 == 6)
            {
                ps.SkipAll();//server data
            }
            else if (type2 == 7)
            {
                //Player data
                int type3 = ps.ReadByte();
                if (type3 == 1)
                {
                    int onlinePlayer = ps.ReadByte();
                    for (int i = 0; i < onlinePlayer; i++)
                    {
                        PlayerObject temp;

                        byte id = ps.ReadByte();
                        temp = players[id];

                        temp.Id = id;
                        temp.Name = StringHelper.DecodePlayerName(ps.ReadString());

                        string lulz = StringHelper.DecodePlayerName(ps.ReadString());

                        lulz += 'a';

                        //ps.ReadByte();//uk
                        temp.Team = ps.ReadByte();
                        ps.ReadByte(); ps.ReadByte(); //uks
                        temp.Score = ps.ReadShort();
                        temp.Death = ps.ReadShort();
                        temp.X = ps.ReadShort();
                        ps.ReadShort();//uk tile_x
                        temp.Y = ps.ReadShort();
                        ps.ReadShort(); // uk tile_y?
                        ps.ReadShort();// rotation??
                        temp.Health = ps.ReadByte();
                        temp.Armor = ps.ReadByte();
                        ps.ReadByte();//uk
                        temp.Currentweapon = ps.ReadByte();
                        ps.ReadByte();

                        players[temp.Id] = temp;
                    }

                    LogConsole("Own player: " + players[own_id].ToString());
                }
                else if (type3 == 2)
                {
                    //hostage data
                    ps.SkipAll();
                }
                else if (type3 == 3)
                {
                    // item data
                    //ps.SkipAll();
                    int count = ps.ReadByte();
                    for (int i = 0; i < count; i++)
                    {
                        ps.ReadByte();
                        ps.ReadByte();
                        byte wpn_id = ps.ReadByte();
                        short tilex = ps.ReadShort();
                        short tiley = ps.ReadShort();
                        short ammoin = ps.ReadShort();
                        short ammo = ps.ReadShort();

                        botLogics.OnWeaponDrop(wpn_id, tilex, tiley, ammo, ammoin);
                    }
                }
                else if (type3 == 4) //entity data
                {
                    ps.SkipAll();
                }
                else if (type3 == 5) //DynamicObjectData
                {
                    ps.SkipAll();
                }
                else if (type3 == 6) // ProjectileData
                {
                    ps.SkipAll();
                }
                else if (type3 == 7) //DynamicObjectImageData
                {
                    ps.SkipAll();
                }
                else if (type3 == 8)
                {
                    ps.SkipAll();
                }
                else if (type3 == 200) // final ack
                {
                    LogConsole("Connected!");
                    ps.ReadString();
                    ps.ReadByte(); ps.ReadByte(); ps.ReadByte();
                    send_unknown_packet_28();
                    //send_spec_pos_32(440, 660);
                }
            }
            else if (type2 == 50) // okay wtf?
                ps.SkipAll();
            return 1;
        }

        int playerleave(PacketStream ps)
        {
            byte id = ps.ReadByte();
            byte reason = ps.ReadByte();// discard reason
            PlayerObject tmp = players[id];

            string textReason;

            switch (reason)
            {
                case 0:
                    textReason = "normal";
                    break;
                case 2:
                    textReason = "kicked";
                    break;
                case 6:
                    textReason = "banned";
                    break;
                case 13:
                    textReason = "voted";
                    break;
                case 15:
                    textReason = "speedhacker";
                    break;
                default:
                    textReason = "unknown";
                    break;                    
            }
            LogConsole(String.Format("{0} left the server! reason:{1} {2}", tmp.Name,reason, textReason));
            players[id] = new PlayerObject(); // make a new one/overwrite the old one
            players[id].Id = 0;

            if (id == own_id)
                cl.Close();

            return 1;
        }


       

        public PlayerObject getLocalPlayer()
        {
            return players[own_id];
        }
        public int getLocalId()
        {
            return own_id;
        }

        public void AddLogicHandler(BotLogics bl)
        {
            botLogics = bl;
        }

        public short PacketNumber
        {
            get { return packetNumber; }
            set { packetNumber = value; }
        }

        public void LogConsole(String s)
        {
            if (form.ConsoleField.InvokeRequired)
                form.ConsoleField.Invoke(new LogTextDelegate(this.LogConsole), s);
            else
            {
                form.ConsoleField.AppendText(s + "\n");
                form.ConsoleField.ScrollToCaret();
            }
        }

        #region sends

        public void send_weaponchange_9(int wpnind)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(9);
            ps.WriteByte(wpnind);
            ps.WriteByte(0);
            ch.SendStream(ps,true);
        }

        public void send_random_stuff()
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);

            char lala = 'a';

           
            for (int j = 0; j < 400; j++)
            {
                string tosend = j.ToString();
                
                ps.WriteByte(240);
                ps.WriteByte(1);
                //ps.WriteByte(1);
                //ps.WriteByte(local_player.Team); // team
                ps.WriteByte(tosend.Length);
                ps.WriteByte(0);
                ps.WritePureString(tosend);
                /*lala++;

                if (lala > 'z')
                    lala = 'a';*/
            }

            ps.WriteByte(7);
            
            ch.SendStream(ps, true);

        }

        public void send_vote(int id)
        {
            PacketStream ps = new PacketStream();            
            ps.WriteShort(1); //dont matter            
            ps.WriteByte(43);
            ps.WriteByte(1);
            ps.WriteString(id.ToString());               
            
            ch.SendStream(ps,true);
        }
        public void send_buy_23(int wpn_id)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(23);
            ps.WriteByte(wpn_id);
            ps.WriteByte(0);
            ch.SendStream(ps,true);
        }
        public void send_reload_16()
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(16);
            ps.WriteByte(1);
            ch.SendStream(ps,true);
        }

        public void send_leave_253()
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(253);
            ps.WriteByte(0);
            ch.SendStream(ps,true);
        }

        public void send_spray_28()
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(28);
            ps.WriteByte(00);
            ps.WriteByte(0x20);
            ps.WriteByte(5);
            ps.WriteByte(0x20);
            ps.WriteByte(6);
            ps.WriteByte(3);
            ch.SendStream(ps,true);
        }

        public void send_fire_7()
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(7);
            ch.SendStream(ps,true);
        }

        public void send_move_10(short newX, short newY)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(10);
            ps.WriteShort(newX);
            ps.WriteShort(newY);
            ch.SendStream(ps,false);
        }

        public void send_rotate_12(float newRotation)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(12);
            ps.WriteFloat(newRotation);
            ch.SendStream(ps,false);
        }

        public void send_spec_pos_32(uint x, uint y)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(32);
            ps.WriteInt(x);
            ps.WriteInt(y);
            ch.SendStream(ps);
        }

        public void send_unknown_packet_28()
        {
            #region bigbyte
            byte[] lulz = {
(byte)packetNumber, 0x00, 
0x1c, 0x02, 0xe1, 0x02, 0x00, 0x04, 0x00, 0x00, 
0x78, 0x9c, 0x63, 0x60, 0xc0, 0x00, 0x9c, 0x6a, 
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c, 
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c,
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c, 
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c, 
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c,
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c, 
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c, 
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c,
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c, 
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c, 
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c,
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c, 
0x6a, 0x6a, 0x1a, 0xdb, 0x27, 0xf4, 0x44, 0x8b, 
0x8b, 0x73, 0x40, 0xa5, 0xc5, 0x42, 0x63, 0xb3, 
0x4b, 0x82, 0xc4, 0xb9, 0xd8, 0x59, 0x99, 0x18, 
0x18, 0x04, 0x64, 0x65, 0x23, 0x0b, 0xdb, 0x7a, 
0xe7, 0xce, 0x0e, 0x4f, 0x17, 0x81, 0xba, 0x3b, 
0x34, 0x3e, 0x28, 0x40, 0x8a, 0x9d, 0x8d, 0x5d, 
0x48, 0x9a, 0x8b, 0x9d, 0x85, 0x81, 0x95, 0x81, 
0x27, 0xa1, 0xba, 0x7f, 0xc2, 0xcc, 0x16, 0x5d, 
0x4e, 0x70, 0x68, 0x28, 0x45, 0x64, 0x65, 0xa4, 
0xc4, 0x05, 0x85, 0x45, 0x46, 0xc4, 0x26, 0x46, 
0x86, 0xfb, 0x29, 0xf2, 0x32, 0x30, 0x70, 0x49, 
0x55, 0x4d, 0x9f, 0x3b, 0x39, 0x55, 0x10, 0x28, 
0xcd, 0x2e, 0x1a, 0x97, 0xd4, 0x78, 0xe2, 0xe0, 
0xde, 0x9d, 0x5b, 0xd7, 0xae, 0x5c, 0x3c, 0x7b, 
0xc6, 0xcc, 0x99, 0x93, 0x0b, 0xe5, 0x80, 0x5e, 
0xd3, 0x9f, 0xbe, 0x62, 0x5a, 0x7f, 0x02, 0x50, 
0xa9, 0x90, 0x6a, 0x52, 0x68, 0xdb, 0x93, 0x1b, 
0x97, 0xce, 0x1c, 0x3b, 0xb0, 0x6b, 0xcb, 0xba, 
0x65, 0xf3, 0xe7, 0xac, 0xd9, 0xd3, 0x2d, 0xcf, 
0xc0, 0xad, 0x50, 0xb8, 0x7c, 0xcd, 0x8a, 0x4e, 
0x45, 0x06, 0x06, 0xe5, 0xb0, 0x60, 0xa5, 0xf6, 
0xcf, 0xcf, 0x1e, 0xdc, 0xbd, 0x79, 0xe5, 0xec, 
0xf1, 0x03, 0x5b, 0x97, 0x4e, 0x9f, 0xb6, 0xe3, 
0x62, 0x91, 0x80, 0x5c, 0x90, 0xc6, 0xe2, 0x63, 
0xdb, 0x3a, 0x64, 0x19, 0x18, 0x02, 0x93, 0xc4, 
0x04, 0x27, 0xfc, 0x07, 0x81, 0x7f, 0x3f, 0xbf, 
0xbe, 0x7b, 0x70, 0x78, 0xe9, 0x94, 0x45, 0x37, 
0xb6, 0x68, 0x32, 0x73, 0xf0, 0x75, 0xdc, 0xdb, 
0xbc, 0x34, 0x8a, 0x81, 0x21, 0x22, 0x96, 0x95, 
0x27, 0x7b, 0xc6, 0x8c, 0xe9, 0xd3, 0xa7, 0x4e, 
0x01, 0x82, 0xbe, 0xae, 0xbe, 0x29, 0x73, 0x4f, 
0x5c, 0x4f, 0x64, 0x67, 0x60, 0x4b, 0x7b, 0x7c, 
0xee, 0x70, 0x21, 0x17, 0xa3, 0x86, 0x0a, 0x03, 
0x03, 0xbf, 0x98, 0xa8, 0x88, 0x88, 0x08, 0x10, 
0x8b, 0x8a, 0x8a, 0xe9, 0x56, 0xcd, 0x39, 0xf4, 
0xa4, 0x81, 0x8f, 0x81, 0x35, 0xe5, 0xde, 0xa5, 
0x83, 0x99, 0x82, 0xac, 0xb2, 0x12, 0x68, 0x91, 
0x1f, 0x3b, 0xfd, 0xf0, 0xab, 0x49, 0x02, 0x0c, 
0x5c, 0xb9, 0x37, 0xcf, 0x1d, 0x6a, 0x10, 0x60, 
0x10, 0x11, 0x40, 0x95, 0x67, 0x09, 0x9d, 0x7e, 
0xec, 0xe5, 0x04, 0x41, 0x06, 0x81, 0xfa, 0xfb, 
0x17, 0x0e, 0x74, 0x49, 0x71, 0xf1, 0x73, 0xa3, 
0xca, 0xf3, 0xa5, 0x2c, 0x38, 0xff, 0xb6, 0x47, 
0x56, 0x5c, 0x63, 0xc1, 0x83, 0x53, 0x7b, 0x5b, 
0xd5, 0x95, 0x38, 0x39, 0x18, 0x58, 0xa5, 0x15, 
0x21, 0x40, 0x49, 0x49, 0x51, 0x35, 0xa2, 0x75, 
0xe3, 0xed, 0x17, 0x05, 0x92, 0x41, 0x71, 0xe7, 
0xef, 0x9e, 0x3e, 0xdf, 0x2c, 0xc0, 0xc6, 0xc6, 
0xc2, 0x20, 0x58, 0xb9, 0x72, 0xc9, 0xa2, 0x79, 
0xb3, 0xa7, 0x4f, 0x99, 0xd8, 0xdb, 0xd9, 0x36, 
0x71, 0xd9, 0xa1, 0xb7, 0x57, 0xc3, 0x19, 0x05, 
0x1a, 0x7e, 0x3c, 0xbc, 0x70, 0xbb, 0x4a, 0x9b, 
0x97, 0x89, 0x91, 0x41, 0xa8, 0xfd, 0xf9, 0xed, 
0x2b, 0xe7, 0x4e, 0x1c, 0xde, 0xb5, 0x79, 0xdd, 
0xca, 0xa5, 0x6b, 0xf6, 0xdc, 0xfc, 0xba, 0x4e, 
0x8e, 0xd5, 0xf0, 0xf8, 0xff, 0x67, 0x0f, 0x2e, 
0x47, 0x0b, 0xb2, 0x33, 0x32, 0x32, 0x08, 0xb4, 
0xbf, 0xba, 0x73, 0xfd, 0xf2, 0xd9, 0x13, 0x87, 
0xf7, 0x6d, 0xdf, 0xb6, 0xfb, 0xc4, 0xdd, 0x6f, 
0xd7, 0x52, 0x38, 0xd5, 0xd7, 0xfd, 0xff, 0xff, 
0xe1, 0xed, 0x01, 0x65, 0x26, 0x50, 0x2a, 0x13, 
0x9c, 0x08, 0x0a, 0xbe, 0x5f, 0xdf, 0x3e, 0xbd, 
0x79, 0xf5, 0xe2, 0xcd, 0xd7, 0xef, 0x67, 0x4a, 
0x84, 0x78, 0x2a, 0xdf, 0x03, 0x45, 0x3e, 0x37, 
0x2b, 0x24, 0x4b, 0x03, 0xe5, 0x05, 0x9a, 0x2e, 
0x9d, 0x39, 0x7d, 0xea, 0xf8, 0xd1, 0xc3, 0x87, 
0x0e, 0x1c, 0x3c, 0xb8, 0xa1, 0x3b, 0x4c, 0x84, 
0x41, 0x74, 0xf6, 0xff, 0xff, 0x7f, 0xfe, 0xbf, 
0xcf, 0x93, 0x8a, 0x01, 0xf9, 0x9e, 0x4d, 0x4e, 
0x5d, 0x45, 0x45, 0x45, 0x19, 0x04, 0x54, 0x54, 
0x64, 0x85, 0x81, 0x89, 0x5f, 0xa0, 0xf1, 0x37, 
0x50, 0xff, 0x83, 0x2c, 0x6c, 0x09, 0x1d, 0x1c, 
0x88, 0x71, 0x67, 0xfe, 0xff, 0xff, 0xba, 0x29, 
0x00, 0x7b, 0x3e, 0x01, 0x02, 0x89, 0xec, 0xe5, 
0x87, 0x96, 0xa5, 0x88, 0xe3, 0x92, 0x06, 0x46, 
0x99, 0xaa, 0xbf, 0x96, 0x08, 0x13, 0x6e, 0x79, 
0x06, 0x16, 0x2e, 0x66, 0x3c, 0xb2, 0xc4, 0x03, 
0x00, 0x47, 0x10, 0x14, 0xe6,0xaa, 0xbf, 0x96, 0x08, 0x13, 0x6e, 0x79,0xaa, 0xbf, 0x96, 0x08, 0x13, 0x6e, 0x79,0xaa, 0xbf, 0x96, 0x08, 0x13, 0x6e, 0x79 };
            #endregion

            PacketStream ps = new PacketStream(lulz,lulz.Length);
            ch.SendStream(ps,true);
            
        }

        public void send_drop_weapon_24(byte wpn_id, short ammo, short ammoin)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(24);
            ps.WriteByte(wpn_id);
            ps.WriteShort(ammo);
            ps.WriteShort(ammoin);
            ps.WriteByte(0);
            ch.SendStream(ps, true);
        }

        public void send_team_change_20(byte team, byte skin)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(20);
            ps.WriteByte(team);
            ps.WriteByte(skin);
            ch.SendStream(ps,true);
        }

        public void send_chat_message_240(string txt)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(240);
            ps.WriteByte(1);
            //ps.WriteByte(1);
            //ps.WriteByte(local_player.Team); // team
            ps.WriteByte(txt.Length);
            ps.WriteByte(0);
            ps.WritePureString(txt);
            ch.SendStream(ps,true);
        }

        private string generateNumber(char a)
        {
            int b = (int)a;
            char s = b.ToString()[0];
            int c = (int)s;         
            return c.ToString();            
        }

        private string generateAuthCode(String ServerKey)
        {
            String s_reversed = StringHelper.ReverseString(ServerKey);
            String part1 = s_reversed.Substring(3, 5);
            String md5_hash = StringHelper.MD5(part1);

            String auth_code = "";
            if (s_reversed[8] % 2 != 0)
                auth_code += "z";
            else
                auth_code += "x";
            if (s_reversed[4] % 5 == 0)
                auth_code += "!O";
            if (s_reversed[7] % 9 == 1)
                auth_code += "l";

            auth_code += md5_hash.Substring(2, 8) + s_reversed.Substring(1, 1) + generateNumber(s_reversed[8]);

            return auth_code;
        }

        public void send_join_confirmation3()
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber); ps.WriteByte(3); ps.WriteByte(0x6d); ps.WriteByte(0x5d); ps.WriteByte(0xFC); ps.WriteByte(0);            
            ch.SendStream(ps,true);
        }

        public void send_join_packet_252_1(PlayerObject p, String ServerKey)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(0xfc);
            ps.WriteByte(1);
            ps.WriteString(p.Name);
            ps.WriteString(p.Password);
            ps.WriteByte(9); ps.WriteByte(0xc8); ps.WriteByte(0x95); ps.WriteByte(0x9e); ps.WriteByte(0x99); ps.WriteByte(0xaa); ps.WriteByte(0x9b); ps.WriteByte(0xa0); ps.WriteByte(0x9c); ps.WriteByte(0xa4); // dunno what this is
            ps.WriteShort(0x77); // version
            ps.WriteInt((uint)p.Usgn); // usgn id
            ps.WriteString(p.Spraylogo);
            ps.WriteString(generateAuthCode(ServerKey));
            ps.WriteByte(0);
            ps.WriteString("w028cefe3ac30ab30ab5268978955263db2736128");
            ps.WriteByte(0);
            ch.SendStream(ps,true);
        }

        public void send_ping_249(uint val)
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(249);
            ps.WriteInt(val);
            ch.SendStream(ps,false);
        }

        public void send_map_name_252_05()
        {
            PacketStream ps = new PacketStream();
            ps.WriteShort(packetNumber);
            ps.WriteByte(0xfc);
            ps.WriteByte(0x05);
            ps.WriteString(currentMapName);
            ch.SendStream(ps,true);
        }

        public void send_map_confirmation(String mapName, String serverKey)
        {
            currentMapName = mapName;

            try
            {
                mappie = new MapParser(mapName);
                mappie.Parse();
                String fullMapDirectory = Directory.GetCurrentDirectory() + "\\maps\\" + mapName + ".map";
                LogConsole("Map directory: " + fullMapDirectory);
                BinaryReader binReader = new BinaryReader(
                            new FileStream(fullMapDirectory, FileMode.Open));

                botLogics.AddPathFinder();


                byte[] bMap = binReader.ReadBytes(Convert.ToInt32(binReader.BaseStream.Length));
                int sum = 0;
                for (int i = 0; i < bMap.Length; i++)
                {
                    sum += (byte)bMap[i];
                }

                binReader.Close();

                PacketStream ps = new PacketStream();
                ps.WriteShort(packetNumber); ps.WriteByte(0xfc); ps.WriteByte(0x03);
                ps.WriteString(bMap.Length.ToString() + "|" + sum.ToString());
                byte[] hash = MapHasher.MapHash(serverKey);

                ps.WriteByte(10);
                for (int i = 0; i < 10; i++)
                {
                    ps.WriteByte(hash[i]);
                }
                ps.WriteByte(6);
                ch.SendStream(ps,true);
            }
            catch (Exception ex)
            {
                LogConsole(ex.Message);
            }
        }
        #endregion

        public void process_packet(PacketStream ps,int pktNum)
        {
            if (pktNum % 2 == 0) // if the packet is an even number, it means the server send a reliable packet, we have to send a acknowledge packet back
            {                
                PacketStream h_shake = new PacketStream();
                h_shake.WriteShort(packetNumber); // own packet number
                h_shake.WriteByte(1);
                h_shake.WriteShort(pktNum); // their packet number
                ch.SendStream(h_shake,false);
            }

            while (!ps.AtEnd())
            {

                int type1 = ps.ReadByte();


                if (known_table[type1] != null)
                {
                    known_table[type1](ps);                    
                }               
                else
                {
                    LogConsole("unknown packet: type: " + type1 + "  packet: " + ps);
                    ps.SkipAll();
                }
            }

            //if (!ps.AtEnd())
              //  process_packet(ps, pktNum);
        }

        public MapParser getMap
        {
            get { return mappie; }
        }
    }    
}