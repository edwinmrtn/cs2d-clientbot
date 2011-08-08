using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cs2dClientBot
{
    /// <summary>
    /// Container for a player object, holds all data of the player
    /// </summary>
    public class PlayerObject
    {
        private String name,password,spraylogo;
        private int id,usgn;
        private short x, y,packetnumber,score,death,money,ping,ammo,ammoin;   
        private byte team,health, armor, currentweapon, skin,weapon_status;      
        private float rot;        

        public PlayerObject()
        {
        }

        public PlayerObject(int id)
        {
            this.id = id;
        }

        public short Ping
        {
            get { return ping; }
            set { ping = value; }
        }

        public short Ammo
        {
            get { return ammo; }
            set { ammo = value; }
        }
        public short AmmoIn
        {
            get { return ammoin; }
            set { ammoin = value; }
        }

        public short Packetnumber
        {
            get { return packetnumber; }
            set { packetnumber = value; }
        }

        public String Name
        {
            get { return this.name; }
            set { name = value; }
        }

        public String Spraylogo
        {
            get { return this.spraylogo; }
            set { spraylogo = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int Usgn
        {
            get { return usgn; }
            set { usgn = value; }
        }

        public String Password
        {
            get { return password; }
            set { password = value; }
        }

        public byte Team
        {
            get { return team; }
            set { team = value; }
        }

        public short X
        {
            get { return x; }
            set { x = value; }
        }

        public short Y
        {
            get { return y; }
            set { y = value; }
        }

        public float Rotation
        {
            get { return rot; }
            set { rot = value; }
        }

        public byte Health
        {
            get { return health; }
            set { health = value; }
        }
        public byte Armor
        {
            get { return armor; }
            set { armor = value; }
        }

        public byte Currentweapon
        {
            get { return currentweapon; }
            set { currentweapon = value; }
        }

         public short Score
         {
             get{ return score;}
             set{score = value;}
         }
         public short Death
         {
             get{ return death;}
             set{ death = value;}
         }

         public byte Skin
         {
             get { return skin; }
             set { skin = value; }
         }

         public short Money
         {
             get{return money;}
             set{money = value;}
         }

         public byte WeaponStatus
         {
             get { return weapon_status; }
             set { weapon_status = value; }
         }

         public override string ToString()
         {
             return Name + " " + Id;
         }
    }
}
