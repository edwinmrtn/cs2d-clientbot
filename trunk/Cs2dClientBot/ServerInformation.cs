using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Cs2dClientBot
{
    /// <summary>
    /// Just a container for a "server" object, used by ServerList class
    /// </summary>
    public class ServerInformation
    {
        IPAddress ip;
        private int port;
        private string name;
        private string map;
        private int currentClients;
        private int maxClients;

        public ServerInformation(IPAddress ip, int port)
        {
            this.ip = ip; this.port = port;
        }

        public IPAddress getIp()
        {
            return ip;
        }
        public int getPort()
        {
            return port;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Map
        {
            get { return map; }
            set { map = value; }
        }

        public int CurrentClients
        {
            get { return currentClients; }
            set { currentClients = value; }
        }

        public int MaxClients
        {
            get { return maxClients; }
            set { maxClients = value; }
        }

        /// <summary>
        /// Method used to fill the datagridview
        /// </summary>
        /// <returns></returns>
        public string[] ToStringArray() 
        {
            string[] data = { this.name, this.Map, this.currentClients.ToString() + "/" + this.maxClients.ToString(), this.ip.ToString(), this.port.ToString() }; 
            return data;
        }
    }
}
