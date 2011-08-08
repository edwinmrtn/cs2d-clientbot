using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Timers;

namespace Cs2dClientBot
{
    public enum WalkingState { WeaponSearch = 0, BombPlant = 1, EnemySearch = 2 };

    public class BotLogics
    {
        public static int[] primary_weapons = { 1, 2, 3, 4, 5, 6 };
        private PacketHandler ph;
        bool b_isRoundStarted = false;
        PlayerObject local;
        private int walkingSpeed = 5;
        PlayerObject[] players;
        private int collision_resolution = 4;
        bool b_reloading;
        List<Client> otherClients;
        List<PathFinderNode> currentRoute;
        Timer shootTimer;
        PathFinder pf;
        bool canShoot = true;

        bool b_spin = true;

        List<Item> dropped_items;
        bool weaponSearch = false;
        int targetX, targetY;

        WalkingState walkingState;

        public bool Spin
        {
            get { return b_spin; }
            set { b_spin = value; }
        }

        public int Speed
        {
            get { return walkingSpeed; }
            set { walkingSpeed = value; }
        }
        public BotLogics(PacketHandler ph, PlayerObject[] players, List<Client> clients)
        {
            this.ph = ph;
            this.players = players;
            local = ph.getLocalPlayer();
            this.otherClients = clients;
            shootTimer = new Timer(100);
            shootTimer.Elapsed += new ElapsedEventHandler(_shootTimerElapsed);
            shootTimer.Enabled = true;

            dropped_items = new List<Item>();
            walkingState = WalkingState.EnemySearch;
        }

        private void _shootTimerElapsed(object sender, ElapsedEventArgs e)
        {
            canShoot = true;
        }

        public List<PathFinderNode> getCurrentRoute()
        {
            return currentRoute;
        }

        public void AddPathFinder()
        {
            byte[,] grid = ph.getMap.Tilemodemap;
            byte[,] new_grid = new byte[grid.GetUpperBound(0)+1, grid.GetUpperBound(1)+1];

            for (int i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    if (grid[i, j] ==  1 || grid[i,j] == 2)
                        new_grid[i, j] = 0; // unwalkable
                    else
                        new_grid[i, j] = 1; // walkable
                }
            }
            //pf = new PathFinder(new_grid);
            pf = new PathFinder(new_grid);
            pf.Formula = HeuristicFormula.Manhattan;
            pf.Diagonals = false;
           // pf.HeavyDiagonals = false;
        }

        private int selectFairTeam()
        {
            int team1 = 0;
            int team2 = 0;
            for (int i = 0; i < players.Length; i++)
            {
                PlayerObject temp = players[i];

                if (temp != null)
                {
                    if (temp.Team == 1)
                        team1++;
                    else if (temp.Team == 2)
                        team2++;
                }
            }
            return (team2 >= team1 ? 1 : 2);
        }

        private bool canHit(PlayerObject opponent,float distance)
        {
            int dx = opponent.X - local.X;
            int dy = opponent.Y - local.Y;
            float angle = (float)Math.Atan2(dy, dx);

            byte[,] tm = ph.getMap.Tilemodemap;

            if (tm == null)
                return true;

            double n_dy = Math.Sin(angle) * collision_resolution;
            double n_dx = Math.Cos(angle) * collision_resolution;

            for (int i = 0; i < (int)(Math.Floor(distance) / collision_resolution); i++)
            {
                int tx = (int)(local.X + i * n_dx) / 32;
                int ty = (int)(local.Y + i * n_dy) / 32;

                if (tx > tm.GetLength(0) || ty > tm.GetLength(1))
                    return true;

                if (tm[tx, ty] == 1) // wall
                    return false;                
            }
            return true;
        }

        private PlayerObject SelectTarget(bool b_hitable)
        {
            float min_dist = 5000.0f;
            PlayerObject closestObj = null;
            
            for (int i = 0; i < players.Length; i++)
            {
                PlayerObject tmp = players[i];
                if (tmp.Id != 0)
                {
                    if (tmp.Health != 0)
                    {
                        if (tmp.Team != local.Team)
                        {
                            float dx = local.X - tmp.X; float dy = local.Y - tmp.Y;
                            float dist = (float)Math.Sqrt((double)(dx * dx + dy * dy));

                            if (dist < min_dist)
                            {
                                if (!b_hitable)
                                {
                                    min_dist = dist;
                                    closestObj = tmp;
                                }
                                else
                                {
                                    if (dist < 700.0f)
                                    {
                                        if (canHit(tmp, dist))
                                        {
                                            min_dist = dist;
                                            closestObj = tmp;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return closestObj;
        }

        private void Move(short x, short y)
        {
            local.X = x; local.Y = y;
            ph.send_move_10(x, y);
        }

        private bool inWalkable(int x, int y)
        {
             byte[,] tm = ph.getMap.Tilemodemap;
            int tilemode = tm[x, y];
            return (tilemode != 1 && tilemode != 2);
        }

        private bool intersectWithMap(Rectangle re)
        {
            int tilex = re.Left / 32;
            int tiley = re.Right / 32;

            return !(inWalkable(re.Left / 32, re.Top / 32) && inWalkable(re.Right / 32, re.Top / 32) && inWalkable(re.Right / 32, re.Bottom / 32) && inWalkable(re.Left / 32, re.Bottom / 32));
            
        }
        private void DxMove(int dx, int dy)
        {
            int next_x = dx + local.X;
            int next_y = dy + local.Y;

            Rectangle rect = new Rectangle(next_x -12, next_y - 12, 24, 24);

            if (!intersectWithMap(rect))
            {
                local.X += (short)dx;
                local.Y += (short)dy;
                ph.send_move_10(local.X, local.Y);
            }
            //local.X += (short)dx;
            //local.Y += (short)dy;
            //ph.send_move_10(local.X, local.Y);
        }

        private Item ItemScan(string type)
        {

            int x = local.X; int y = local.Y;


            foreach (Item it in dropped_items)
            {

                int weaponx = it.tilex * 32 + 16;
                int weapony = it.tiley * 32 + 16;

                float dist = (float)Math.Sqrt(
                    Math.Pow((double)(weaponx - x), 2) +
                    Math.Pow((double)(weapony - y), 2)
                    );

                if(dist <= 300)
                {

                    if (type == "primary")
                    {
                        if (it.wpn_id <= 40 && it.wpn_id > 10)                        
                            return it;                        
                    }
                    else if (type == "secondary")
                    {
                        if(it.wpn_id <= 10)
                            return it;
                    }
                    else
                        return it;
                }
            }

            return null;
        }

        private void MakeCircles()
        {
            local.Rotation+=30;

            if (local.Rotation >= 180)
                local.Rotation = -180;
            ph.send_rotate_12(local.Rotation);            
        }

        public void Run()
        {
            int divider = 0;
            while (true)
            {
                System.Threading.Thread.Sleep(20); // 50 fps

                local = players[ph.getLocalId()];

                if (local == null)
                    continue;

                if (local.Team == 0) // spectator
                {
                    //continue;
                    int fair_team = selectFairTeam();
                    ph.send_team_change_20((byte)fair_team, 2);
                    local.Team = (byte)fair_team;
                }

                if (local.Health == 0) // dead
                    continue;
                                              
                PlayerObject cl = SelectTarget(true); // hitable target
                if (cl != null)
                {
                    float dx = cl.X - local.X; float dy = cl.Y - local.Y;
                    float angle = (float)Math.Atan2(dy, dx) * (float)(180 / Math.PI) + 90.0f;
                    ph.send_rotate_12(angle);

                    if (canShoot)
                    {
                        canShoot = false;
                        local.AmmoIn--;
                        if (local.AmmoIn == 0)
                        {
                            ph.send_reload_16();
                            b_reloading = true;
                        }
                        //if(!b_reloading)
                        ph.send_fire_7();
                    }
                }
                else
                {
                    if(b_spin)
                        MakeCircles();
                }               

                if (currentRoute != null) // yay a path has been found
                {
                    //DxMove((currentRoute[0].X * 32+16) - local.X, (currentRoute[0].Y * 32) - local.Y+16);
                    if (currentRoute.Count >= 1)
                    {
                        int targetX = currentRoute[0].X * 32 + 16;
                        int targetY = currentRoute[0].Y * 32 + 16;


                        if (local.X == targetX && local.Y == targetY)
                        {
                            if(currentRoute.Count >= 2)
                                currentRoute.RemoveAt(0);
                            targetX = (currentRoute[0].X * 32 + 16); targetY = (currentRoute[0].Y * 32 + 16);
                        }

                        int next_x = targetX;   //.X * 32 + 16;
                        int next_y = targetY;

                        int dx = next_x - local.X; int dy = next_y - local.Y;

                        Vector2D moveVector = new Vector2D(dx, dy);

                        if (moveVector.length() <= walkingSpeed)
                        {
                            DxMove((int)moveVector.X, (int)moveVector.Y);
                        }
                        else
                        {
                            Vector2D moveVectorUniformed = moveVector.Uniform();
                            DxMove((int)moveVectorUniformed.X * walkingSpeed, (int)moveVectorUniformed.Y * walkingSpeed);                            
                        }                        
                    }
                }

                /* DETERMINE WALK PATH */

                divider++;
                if (divider == 10)
                {
                    divider = 0;
                    currentRoute = null;

                    switch (walkingState)
                    {
                        case WalkingState.EnemySearch:
                            PlayerObject cl2 = SelectTarget(false); // walkable target
                            if (cl2 != null)
                            {                            
                                Goto(cl2.X, cl2.Y);                            
                            }
                            break;
                        case WalkingState.WeaponSearch:
                            Item scan = ItemScan("primary");
                            if (scan != null)
                            {
                                Goto(scan.tilex*32 + 16, scan.tiley*32 + 16);
                            }
                            break;
                        case WalkingState.BombPlant:
                            break;
                    }                                      
                }
            }
        }

        private bool isPrimaryWeapon(int wpn_id)
        {
            return (wpn_id > 10 && wpn_id <= 40);
        }

        private bool Goto(int x, int y)
        {
            if( pf != null)
                currentRoute = pf.FindPath(new Point(local.X /32, local.Y /32), new Point(x/32, y/32));

            if (currentRoute != null)
                return true;
            else
                return false;
        }

        public void OnWeaponDrop(int id, int tilex, int tiley, short ammo, short ammoin)
        {
            Item droppedItem = new Item(id, tilex, tiley, ammo, ammoin);
            dropped_items.Add(droppedItem);
        }

        public void OnRoundStart()
        {
            int money = local.Money;

            ph.send_buy_23(30);
            ph.send_buy_23(32);
            ph.send_buy_23(61);
            ph.send_buy_23(58);
            ph.send_buy_23(57);
            ph.send_buy_23(62); 

            if (isPrimaryWeapon(local.Currentweapon))// has primary weapon
            {
                walkingState = WalkingState.EnemySearch;
            }
            else if (ItemScan("primary") != null)
            {
                walkingState = WalkingState.WeaponSearch;
            }
            else
            {
                walkingState = WalkingState.EnemySearch;                              
            }            

            b_isRoundStarted = true;
            currentRoute = null;
        }

        public void OnDeath()
        {
            local.Currentweapon = 0;
        }

        public void OnKill(byte victim)
        {
            local.Money += 300;
            ph.send_chat_message_240("You got owned " + players[victim].Name);
        }

        public void OnWeaponBuy(byte wpn_id)
        {
            switch (wpn_id)
            {
                case 248:
                    return;
                case 249:
                    return;
                case 250:
                    return;
                case 251:
                    return;
                case 252:
                    //ph.LogConsole("Cannot buy ( wrong team)");
                    return;
                case 253:
                    //ph.LogConsole("Not enough money to buy weapon!");
                    return;
                case 254:
                    //ph.LogConsole("Cannot buy this weapon! Buytime expired ");
                    return;
                case 255:
                    //ph.LogConsole("Not in the right area to buy weapon!");
                    return;
                default:                    
                    break;
            }

            if (isPrimaryWeapon(wpn_id))
                walkingState = WalkingState.EnemySearch;

            local.Currentweapon = wpn_id;
            local.Ammo = (short)Weapons.wpns[wpn_id].ammo;
            local.AmmoIn = (short)Weapons.wpns[wpn_id].ammoIn;
        }

        public void OnRoundEnd()
        {
            b_isRoundStarted = false;
        }

        
        public void OnVote(string data)
        {
            Regex voteregex = new Regex("(?<actor>[^,]+),(?<victim>.+)");
            Match m = voteregex.Match(data);
            if (m.Success)
            {
                try
                {
                    int actor = Int32.Parse(m.Groups["actor"].Value);
                    int victim = Int32.Parse(m.Groups["victim"].Value);

                    foreach (Client cl in otherClients) // if any of the online clients is voted, vote him back!
                    {
                        if (cl.getPacketHandler().getLocalId() == victim)
                        {
                            ph.send_vote(actor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ph.LogConsole(ex.Message);
                }
                //
            }
        }

        public void OnWeaponPickup(int wpnid, short ammo, short ammoin)
        {
            if (walkingState == WalkingState.WeaponSearch) // only if lookign for weapon, pick it up
            {
                if (isPrimaryWeapon(wpnid))
                {
                    ph.send_weaponchange_9(wpnid);
                    local.Currentweapon = (byte)wpnid;
                    local.Ammo = ammo;
                    local.AmmoIn = ammoin;

                    // need remove item from dropped item list

                    walkingState = WalkingState.EnemySearch;
                }
            }
           
        }

        public void OnReloadFinish()
        {
            ph.LogConsole("OnReloadFinish!");
            local.AmmoIn = 30;
            b_reloading = false;
        }

        public void OnFire()
        {
            local.AmmoIn--;
            if (local.AmmoIn <= 0)
            {
                ph.send_reload_16();
                b_reloading = true;
            }
        }
    }
}
