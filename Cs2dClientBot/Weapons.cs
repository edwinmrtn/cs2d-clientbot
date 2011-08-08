using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cs2dClientBot
{

    public struct Weapon
    {
        public int ammoIn, range, ammo, price, id, reload, rate;
        public string name;


        public Weapon(int id, string name, int rate, int reload, int ammo, int ammoin, int price, int range)
        {
            this.ammoIn = ammoin; this.ammo = ammo; this.price = price; this.id = id; this.name = name;
            this.range = range; this.reload = reload; this.rate = rate;
        }       

    }

    public class Item
    {
        public int wpn_id, tilex, tiley;
        public short ammo, ammo_in;

        public Item(int wpn_id, int tilex, int tiley, short ammo, short ammoin)
        {
            this.wpn_id = wpn_id; this.tilex = tilex; this.tiley = tiley; this.ammo = ammo; this.ammo_in = ammoin;
        }

        public override bool Equals(object obj)
        {
            if (obj is Item)
            {
                Item it = obj as Item;
                return it.tilex == this.tilex && it.tiley == this.tiley && it.wpn_id == this.wpn_id;
            }
            else
                return false;
        }
    }

    public static class Weapons
    {
        public static Weapon[] wpns = new Weapon[64];       

        static Weapons()
        {
             wpns[1] = new Weapon(1, "USP",9, 220, 100, 12, 500, 300);
            wpns[2] = new Weapon(2, "Glock",9, 220, 120, 20, 400, 250);
            wpns[3] = new Weapon(3, "Deagle", 15, 210, 35, 7, 650, 300);
            wpns[4] = new Weapon(4, "P228", 9, 270, 53, 13, 650, 300);
            wpns[5] = new Weapon(5, "Elite", 8, 250, 120, 15, 1000, 300);

            wpns[30] = new Weapon(30, "AK-47", 3, 275, 90, 30, 2500, 300);
            wpns[32] = new Weapon(32, "M4A1", 3, 220, 90, 30, 3100, 300);

            wpns[57] = new Weapon(57, "Kevlar", 0, 0, 0, 0, 650, 0);
            wpns[58] = new Weapon(58, "Kevlar+Helm", 0, 0, 0, 0, 1000, 0);
            wpns[61] = new Weapon(61, "Primary Ammo", 0, 0, 0, 0, 50, 0);
            wpns[62] = new Weapon(62, "Secondary Ammo", 0, 0, 0, 0, 50, 0);
        }       
    }
}
