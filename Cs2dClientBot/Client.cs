using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

namespace Cs2dClientBot
{
    public class Client
    {
        ConnectionHandler chandler;
        PacketHandler ph;
        PlayerObject[] players;
        PlayerObject local_player;
        ServerList svl;
        BotLogics bl;
        private Thread _logicThread;
        Form1 form;

        // initializes everything and sends join request to server
        public Client(IPAddress ipAddress, int port, string name, string password, int usgn_id, string sprayname,Form1 form)
        {
            chandler = new ConnectionHandler(ipAddress,port);
            local_player = new PlayerObject();
            players = new PlayerObject[32];
            for (int i = 0; i < 32; i++)
                players[i] = new PlayerObject();

            local_player.Name = name;
            local_player.Password = password;
            local_player.Usgn = usgn_id;
            local_player.Spraylogo = sprayname;

            ph = new PacketHandler(chandler, form, local_player, players,this);
            bl = new BotLogics(ph, players, form.getClientList());
            _logicThread = new Thread(new ThreadStart(bl.Run));
            _logicThread.Start();
            chandler.AddHandler(ph);
            ph.AddLogicHandler(bl);
            chandler.ReceiveMessages();
            ph.send_join_confirmation3();
            this.form = form;
        }

        public PlayerObject[] getPlayers()
        {
            return players;
        }

        public PacketHandler getPacketHandler()
        {
            return ph;
        }

        public BotLogics getBotLogics()
        {
            return bl;
        }

        public void Close()
        {
            //ph.send_leave_253(); // leave server

            _logicThread.Abort();
            _logicThread.Join();
            chandler.Close();            
            ph = null;
            chandler = null;

            form.getClientList().Remove(this);
        }
    }
}
