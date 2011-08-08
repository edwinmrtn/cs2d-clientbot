using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cs2dClientBot
{
    static class StringHelper
    {
        /// <summary>
        /// Receives string and returns the string with its letters reversed.
        /// </summary>
        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// <summary>
        /// Receives a string and returns the md5 hash of the string/// 
        /// </summary>
        /// <param name="password"></param>
        /// <returns>md5 hash of the string</returns>
        public static string MD5(string password)
        {
            byte[] textBytes = System.Text.Encoding.Default.GetBytes(password);
            try
            {
                System.Security.Cryptography.MD5CryptoServiceProvider cryptHandler;
                cryptHandler = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] hash = cryptHandler.ComputeHash(textBytes);
                string ret = "";
                foreach (byte a in hash)
                {
                    if (a < 16)
                        ret += "0" + a.ToString("x");
                    else
                        ret += a.ToString("x");
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Decodes a string based on a weird algorithm
        /// </summary>
        /// <param name="shiz"></param>
        /// <returns>decoded string</returns>
        public static string DecodePlayerName(string shiz)
        {
            char[] new_shizzle = new char[shiz.Length];
            for (int i = 0; i < shiz.Length; i++)
            {
                if (i % 3 == 0)
                    new_shizzle[i] = (char)(shiz[i] - 110);
                else if( (i %3)==1)
                    new_shizzle[i] = (char)(shiz[i] - 97);
                else
                    new_shizzle[i] = (char)(shiz[i] - 109);
            }
            return new String(new_shizzle);
        }
    }
}
