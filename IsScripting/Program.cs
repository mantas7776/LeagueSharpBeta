using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace IsScripting
{
    class Program
    {
        struct XY
        {
            public float x;
            public float y;
            public XY(float x, float y) 
            {
                this.x = x;
                this.y = y;
            }
        }
        struct wpherotime
        {
            public XY pos;
            public DateTime time;
            public Obj_AI_Hero hero;
        }
        static Dictionary<Obj_AI_Hero, XY> LastPos = new Dictionary<Obj_AI_Hero, XY>();
        static Dictionary<Obj_AI_Hero, int> avgcount = new Dictionary<Obj_AI_Hero, int>();
        static Dictionary<Obj_AI_Hero, double> Avgtotal = new Dictionary<Obj_AI_Hero, double>();
        static List<wpherotime> waypoints = new List<wpherotime>();
        static List<int> secondavg = new List<int>();
        static DateTime TimeToDel;
        static Menu Menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Isscripting?");
            Game.OnUpdate += Game_OnUpdate;
            foreach (var hero in HeroManager.AllHeroes)
            {
                LastPos.Add(hero, new XY(0,0));
                Avgtotal.Add(hero, 0);
                avgcount.Add(hero, 0);
            }
            TimeToDel = DateTime.Now;
            CreateMenus();
        }


        static void Game_OnUpdate(EventArgs args)
        {
            AddNewWaypoints();
            if(DateTime.Now>=TimeToDel)
            {
                DeleteOldWaypoints();
                TimeToDel = DateTime.Now.AddSeconds(1);
            }
            int count = 0;

            foreach (var hero in HeroManager.AllHeroes)
            {
                if (avgcount[hero] != 0)
                {          
                    var avg = Math.Round((decimal)Avgtotal[hero] / avgcount[hero], 2);
                    if (Menu.Item("table").GetValue<KeyBind>().Active)
                    {
                        Drawing.DrawText(1700, 200 + count * 12, System.Drawing.Color.Red, hero.Name + ": " + avg.ToString());
                        count++;
                    }
                    
                }

            }
        }


        static void DeleteOldWaypoints()
        {
            Dictionary<Obj_AI_Hero, int> nomovement = new Dictionary<Obj_AI_Hero,int>();
            foreach (var hero in HeroManager.AllHeroes) nomovement.Add(hero, 0);

            foreach(var wp in waypoints)
            {
                Avgtotal[wp.hero] += 1;
                nomovement[wp.hero] += 1;
            }

            foreach (var hero in HeroManager.AllHeroes.Where(x => x.IsVisible && !x.IsDead && nomovement[x] != 0)) avgcount[hero]++;

            waypoints.Clear();
        }
        static void AddNewWaypoints()
        {
            foreach (var hero in HeroManager.AllHeroes)
            {
                if (hero.GetWaypoints().Count >= 1 && !hero.IsDead && hero.IsVisible)
                {
                    var waypoint = hero.GetWaypoints().Last();
                    if (LastPos[hero].x != waypoint.X || LastPos[hero].y != waypoint.Y)
                    {

                        //Drawing.DrawText(400, 200 + count * 10, System.Drawing.Color.Red, hero.GetWaypoints().Last().X + " " + hero.GetWaypoints().Last().Y);
                        wpherotime item;
                        item.hero = hero;
                        item.time = DateTime.Now;
                        item.pos = new XY(waypoint.X, waypoint.Y);
                        waypoints.Add(item);
                       // Game.PrintChat("waypoint added " + hero.Name);
                       // Game.PrintChat("Lastpos x: " + LastPos[hero].x.ToString() + "Wp x: " + waypoint.X.ToString());
                        LastPos[hero] = new XY(waypoint.X, waypoint.Y);
                    }
                }
            }
        }

        static void CreateMenus()
        {
            Menu = new Menu("ScriptDetect", "ScriptDetect", true);
            Menu.AddToMainMenu();
            Menu.AddItem(new MenuItem("table", "Show table: ").SetValue(new KeyBind(0x6A, KeyBindType.Toggle)));
        }


    }
}
