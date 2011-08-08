using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cs2dClientBot
{
    static public class MapHasher
    {
        static int[] table ={33,97,67,122,48,106,75,53,102,33};
        

        static public byte[] MapHash(String skey)
        {
            byte[] hash = new byte[skey.Length];
            int i;
            int j = 9;

            for (i = 0; i < skey.Length; i++)
            {
                hash[i] = (byte)((table[i] + skey[j])%256);
                j--;
            }

            return hash;
        }
    }
}
