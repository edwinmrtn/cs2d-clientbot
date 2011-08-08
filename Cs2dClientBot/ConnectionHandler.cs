using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Cs2dClientBot
{
    public class ConnectionHandler
    {

        private Socket clientSocket;        
        private PacketHandler ph;      
        private byte[] buffer;

        private List<PacketStream> important_received;

        private Dictionary<short, PacketStream> important_send;

        /// <summary>
        /// This constructor sets up the socket, and connects to the selected server
        /// </summary>
        /// <param name="Ipadd"></param>
        /// <param name="port"></param>
        public ConnectionHandler(IPAddress Ipadd, int port)
        {
            buffer = new byte[2048];            
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            clientSocket.Connect(Ipadd, port);

            important_received = new List<PacketStream>();
            important_send = new Dictionary<short,PacketStream>();
        }

        public List<PacketStream> ImportantReceived
        {
            get { return important_received; }
        }

        public Dictionary<short,PacketStream> ImportantSend
        {
            get { return important_send; }
        }

        public void ReceiveMessages()
        {
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
        }

        public void AddHandler(PacketHandler ph)
        {
            this.ph = ph;
        }      

        public void SendStream(PacketStream ps)
        {
            clientSocket.BeginSend(ps.toArray(), 0, ps.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            if (ps.getType() != 1)
                ph.PacketNumber++;
        }

        public void Close()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

       /* private void OnDisconnect(IAsyncResult ar)
        {
            clientSocket.EndDisconnect(ar);
            clientSocket.Close();
        }*/
        
        public void SendStream(PacketStream ps, bool important) 
        {
            if (important)
            {
                ph.PacketNumber += 2;
                ps.setPacketNumber(ph.PacketNumber);

                /*important_send.Add(ph.PacketNumber, ps);*/
               // important_send.Add(ps);
            }
            else
            {
                short tempPacketnum = (short)(ph.PacketNumber + 1);
                ps.setPacketNumber(tempPacketnum);
            }
            
            clientSocket.BeginSend(ps.toArray(), 0, ps.Length, SocketFlags.None, new AsyncCallback(OnSend), null);            
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException ex)
            {
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int length = clientSocket.EndReceive(ar);
                PacketStream ps = new PacketStream(buffer, length);
                int pktNumber = ps.ReadShort();

                /*if (pktNumber % 2 == 0)
                    important_received.Add(ps);*/
                ph.process_packet(ps, pktNumber);
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
            catch (SocketException ex)
            {
                ph.LogConsole("Error in socket: " + ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
            }
        }
    }
}