using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

namespace Cs2dClientBot
{
    /// <summary>
    /// Serverlist class
    /// </summary>
    public class ServerList
    {
        Socket serverlistSocket;
        List<ServerInformation> serverList;
        DataGridView dgv;
        System.Timers.Timer sTimer;
        byte[] buffer;
        Form1 form1;

        delegate void d_serverToGrid(ServerInformation svi);
        delegate void LogTextDelegate(string newText);

        /// <summary>
        /// Constructor, sets up the socket, initializes the data buffer, sets handle to the datagridview and creates a list to hold servers
        /// </summary>
        /// <param name="dgv"></param>
        public ServerList(Form1 form1,DataGridView dgv)
        {
            serverlistSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            buffer = new byte[1024];
            this.dgv = dgv;
            serverList = new List<ServerInformation>();
            sTimer = new System.Timers.Timer(2000);
            sTimer.Elapsed += new System.Timers.ElapsedEventHandler(sTimer_Elapsed);
            sTimer.Enabled = true;
            this.form1 = form1;
        }

        void sTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            sTimer.Enabled = false;


            for (int i = 0; i < serverList.Count; i++)
            {
                if (serverList[i].Name == null)
                    RequestServerInformation(serverList[i].getIp(), (ushort)serverList[i].getPort());
            }
        }

        public void LogConsole(String s)
        {
            if (form1.ConsoleField.InvokeRequired)
                form1.ConsoleField.Invoke(new LogTextDelegate(this.LogConsole), s);
            else
            {
                form1.ConsoleField.AppendText(s + "\n");
                form1.ConsoleField.ScrollToCaret();
            }
        }

        private void AddServerToGrid(ServerInformation svi)
        {
            if (dgv.InvokeRequired)
                dgv.Invoke(new d_serverToGrid(this.AddServerToGrid), svi);
            else
                dgv.Rows.Add(svi.ToStringArray());
        }

        public List<ServerInformation> getServerList()
        {
            return serverList;
        }

        private void ReceiveMessages()
        {
            try
            {
                EndPoint ep = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
                serverlistSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref ep, new AsyncCallback(OnReceive), null);
            }
            catch (SocketException ex)
            {
                LogConsole(ex.Message);
            }
        }

        /// <summary>
        /// Begins sending server request packet to usgn.de
        /// </summary>
        public void GetServers()
        {
            IPAddress[] ips = Dns.GetHostAddresses("www.usgn.de");
            IPEndPoint ipeUsgn = new IPEndPoint(ips[0], 36963);
            EndPoint epUsgn = (EndPoint)ipeUsgn;

            PacketStream ps = new PacketStream();
            ps.WriteShort(1);
            ps.WriteByte(0x14); ps.WriteByte(1);

            serverlistSocket.BeginSendTo(ps.toArray(), 0, ps.Length, SocketFlags.None, epUsgn, new AsyncCallback(OnSend), null);
            ReceiveMessages();
        }


        /// <summary>
        /// Constructs the list of servers, based on the packet received from usgn.de
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        private int OnUsgnServerPacket(PacketStream ps)
        {
            short num_servers = ps.ReadShort();
            for (int i = 0; i < num_servers; i++)
            {
                byte[] ipdata = new byte[4];
                for (int j = 0;j < 4; j++) { ipdata[3-j] = ps.ReadByte(); }

                IPAddress ipa = new IPAddress(ipdata);
                ushort port = (ushort)ps.ReadShort();
                RequestServerInformation(ipa, port);
                ServerInformation svi = new ServerInformation(ipa,port);
                serverList.Add(svi);
             
            }
            return num_servers;
        }

        /// <summary>
        /// function used to request information from a server
        /// </summary>
        /// <param name="ipa"></param>
        /// <param name="port"></param>
        private void RequestServerInformation(IPAddress ipa, ushort port)
        {
            byte[] lulz = { 0x01, 0x00, 0x03, 0x8D, 0xA2, 0xFB , 0x01, 0x73, 0x00 };
            EndPoint epoint = (EndPoint)(new IPEndPoint(ipa, (int)port));
                
            serverlistSocket.BeginSendTo(lulz,0,lulz.Length,SocketFlags.None,epoint,new AsyncCallback(OnSend),null);
        }

        private void OnSend(IAsyncResult ar)
        {
            serverlistSocket.EndSend(ar);
        }

        /// <summary>
        /// Function determines what happens when information about a server is returned
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="svi"></param>
        /// <returns></returns>
        private int OnServerInformationReceive(PacketStream ps,ServerInformation svi)
        {
            byte cmd = ps.ReadByte();
            if (cmd == 1)
            {
                ps.ReadByte(); // dunno?                
                svi.Name = ps.ReadString();
                svi.Map = ps.ReadString();
                svi.CurrentClients = ps.ReadByte();
                svi.MaxClients = ps.ReadByte();
                byte gamemode = ps.ReadByte();

                AddServerToGrid(svi);
            }
            else if (cmd == 5)
            {
                //player info
            }
            return 0;
        }


        /// <summary>
        /// This function parses the packet
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            int length = serverlistSocket.EndReceiveFrom(ar, ref ep);
            IPEndPoint ipe = (IPEndPoint)ep;
            PacketStream ps = new PacketStream(buffer, length);
            short pkt = ps.ReadShort();
            byte type = ps.ReadByte();
            int retn;
            switch (type)
            {
                case 0x14:
                    retn = OnUsgnServerPacket(ps);
                    break;
                case 0xfb:
                    ServerInformation svi = GetServerFromList(ep);
                    if(svi != null)
                        OnServerInformationReceive(ps,svi);
                    break;
                default:
                    break;
            }
            ReceiveMessages(); // start receivin' the shizzle again
        }

        private ServerInformation GetServerFromList(EndPoint ep)// Better to use dictionary for this
        {
            IPEndPoint ipe = (IPEndPoint)ep;
            foreach(ServerInformation sv in serverList)
            {          
                
                if (ipe.Address.Equals(sv.getIp()) && ipe.Port== sv.getPort())
                    return sv;
            }
            return null;
        }
    }
}
