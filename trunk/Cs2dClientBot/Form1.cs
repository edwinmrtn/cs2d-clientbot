using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
//using XNA = Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
using Cs2dClientBot.Map;
using System.Threading;

namespace Cs2dClientBot
{
    public partial class Form1: Form//public partial class Form1 : XNAWinForm
    {
        
        ServerList svl;
        List<Client> clientList;
        //Texture2D dummyTexture;
        //XNA.Rectangle dummyRectangle;
        //private SpriteBatch spriteBatch;

        public Form1()
        {
            InitializeComponent();

            //this.DeviceResetting += new XNAWinForm.EmptyEventHandler(mWinForm_DeviceResetting);
            //this.DeviceReset += new XNAWinForm.GraphicsDeviceDelegate(mWinForm_DeviceReset);
            //this.OnFrameRender += new XNAWinForm.GraphicsDeviceDelegate(mWinForm_OnFrameRender);
            //this.OnFrameMove += new GraphicsDeviceDelegate(Form1_OnFrameMove);

            clientList = new List<Client>();
            
        }

        public void Log(string s)
        {
            ConsoleField.AppendText(s + "\n");
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                
                IPAddress ipa = IPAddress.Parse(IpInputField.Text);
                if (ipa == null)
                    Log("Error: enter valid ip");
                int port = Int32.Parse(PortInputField.Text);
                if (port == 0)
                    Log("Error: enter valid port");

                Log(String.Format("Connecting to {0}:{1}", ipa, port));

                if (NameInputField.Text == null || NameInputField.Text == "")
                    NameInputField.Text = "uPraTe6";
                if (SpraynameInputField.Text == null || SpraynameInputField.Text == "")
                    SpraynameInputField.Text = "unrealsoftware.bmp";

                Client new_client = new Client(ipa, port, NameInputField.Text, PortInputField.Text, 0, SpraynameInputField.Text, this);
                clientList.Add(new_client);                
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        public List<Client> getClientList()
        {
            return clientList;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (Client cl in clientList)
            {
                cl.Close();
               
            }
            clientList.Clear();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            foreach (Client cl in clientList)
            {
                cl.Close();
                //clientList.Remove(cl);
            }
            clientList.Clear();

        }

        private void SendCommandButton_Click(object sender, EventArgs e)
        {
            String text = CommandInputField.Text;
            //ph.send_chat_message_240(text);           
            foreach (Client cl in clientList)
                cl.getPacketHandler().send_chat_message_240(text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
                 
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        //void Form1_OnFrameMove(Microsoft.Xna.Framework.Graphics.GraphicsDevice pDevice)
        //{
           // mRotation += 0.1f;
           // this.mWorldMat = Matrix.CreateRotationY(mRotation);            
        //}

        /*void mWinForm_OnFrameRender(GraphicsDevice pDevice)
        {
            //pDevice.Clear(XNA.Graphics.Color.Beige);          
                       

            int x_offset = pDevice.Viewport.X;
            int y_offset = pDevice.Viewport.Y;

            
            if (clientList.Count == 0  || clientList.First() == null)
                return;

            spriteBatch.Begin();
               
            PacketHandler ph = clientList[0].getPacketHandler();

            if (ph != null)// draw the map
            {
                MapParser mappie = ph.getMap;
                if (mappie != null) // okay we have a handle to the map
                {
                    byte[,] tm = mappie.Tilemodemap;
                    if (tm != null) // okay we have a handle to the tilemodemap
                    {
                        int width = mappie.mapWidth;
                        int height = mappie.mapHeight;
                        XNA.Graphics.Color clwall = XNA.Graphics.Color.WhiteSmoke;
                        XNA.Graphics.Color clhalf_wall = XNA.Graphics.Color.Bisque;
                        
                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                if (tm[x, y] == 1)
                                    spriteBatch.Draw(dummyTexture, new XNA.Rectangle((x_offset +(x * 4)), y_offset + (int)(y * 4), 4, 4), clwall);
                                else if(tm[x,y] == 2)
                                    spriteBatch.Draw(dummyTexture, new XNA.Rectangle((x_offset + (x * 4)), y_offset + (int)(y * 4), 4, 4), clhalf_wall);
                            }
                        }


                        // draw all the players
                        PlayerObject[] players = clientList[0].getPlayers();
                        if (players != null)
                        {
                            for (int i = 0; i < 32; i++)
                            {
                                PlayerObject pl = players[i];
                                if (pl != null)
                                {
                                    XNA.Graphics.Color lal;
                                    if (pl.Team == 1)
                                        lal = XNA.Graphics.Color.Red;
                                    else
                                        lal = XNA.Graphics.Color.Blue;

                                    if (pl.Id == ph.getLocalPlayer().Id)
                                        lal = XNA.Graphics.Color.Green;

                                    if (pl.Id != 0)
                                    {
                                        if (pl.Health > 0)
                                        {
                                            spriteBatch.Draw(dummyTexture, new XNA.Rectangle(x_offset + (int)(pl.X/8) , y_offset + (int)(pl.Y/8), 4, 4), lal);
                                        }
                                    }
                                }
                            }
                        }
                        // draw route

                        List<PathFinderNode> ls = clientList[0].getBotLogics().getCurrentRoute();
                        if (ls != null)
                        {
                            for (int i = 0; i < ls.Count; i++)
                            {
                                spriteBatch.Draw(dummyTexture, new XNA.Rectangle(x_offset + ls[i].X * 4, y_offset + ls[i].Y * 4, 4, 4), XNA.Graphics.Color.Orange);
                            }
                        }


                    }
                }
            }
            
            
            spriteBatch.End();
        }

        void mWinForm_DeviceReset(GraphicsDevice pDevice)
        {

            spriteBatch = new SpriteBatch(pDevice);
            dummyTexture = new Texture2D(pDevice, 1,1, 1, ResourceUsage.None, SurfaceFormat.Color);
            dummyTexture.SetData(new XNA.Graphics.Color[] { XNA.Graphics.Color.White });
            dummyRectangle = new XNA.Rectangle(pDevice.Viewport.X, pDevice.Viewport.Y, 20, 20);
            
        }

        void mWinForm_DeviceResetting()
        {
            
        }*/

        private void button1_Click(object sender, EventArgs e)
        {
            //foreach(Client cl in clientList)
                //cl.getPacketHandler().send_team_change_20(1,1);
            foreach (Client cl in clientList)
                cl.getPacketHandler().send_leave_253();
        }

        private void button2_click(object sender, EventArgs e)
        {
            foreach (Client cl in clientList)
            {
                try
                {
                    cl.getPacketHandler().send_vote(Int32.Parse(CommandInputField.Text));
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }
            }
        }

        private void ServerlistButton_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows.Clear();
            svl = new ServerList(this,this.dataGridView1);
            svl.GetServers();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int selected = e.RowIndex;
            if (selected == -1)
                return;
            if (dataGridView1.Rows[selected].Cells[0].Value == null)
                return;

            IpInputField.Text = (string)dataGridView1.Rows[selected].Cells[3].Value;
            PortInputField.Text = (string)dataGridView1.Rows[selected].Cells[4].Value;
        }
     

        private void button3_click(object sender, EventArgs e)
        {
            foreach (Client cl in clientList)
                cl.getPacketHandler().send_fire_7();
        }

        private void reload_Click(object sender, EventArgs e)
        {
            foreach (Client cl in clientList)
                cl.getPacketHandler().send_reload_16();
        }

        private void buttonspray_click(object sender, EventArgs e)
        {
            foreach (Client cl in clientList)
                cl.getPacketHandler().send_spray_28();
        }

        private void connectToAll_Click(object sender, EventArgs e)
        {
            if (svl != null)
            {
                foreach (ServerInformation svi in svl.getServerList())
                {
                    Client cl = new Client(svi.getIp(), svi.getPort(), "UPRATE6 WATCHES YOU", "", 0, "bladiebla.bmp", this);
                    clientList.Add(cl);
                }
            }
            else
            {
                Log("No serverlist loaded yet!");
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            foreach (Client cl in clientList)
                cl.getPacketHandler().send_random_stuff();
        }

        private void CommandInputField_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void PasswordInputField_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Client cl in clientList)
            {
                cl.getBotLogics().Spin = checkBox1.Checked;
            }                
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {          
            foreach(Client cl in clientList)
            {
                cl.getBotLogics().Speed = trackBar1.Value;
            }           
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
        
    }
}
