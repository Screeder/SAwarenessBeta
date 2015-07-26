using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAssemblies;
using SAssemblies.Trackers;
using Menu = SAssemblies.Menu;

namespace SAwareness.Trackers
{
    class Avoid
    {
        public static Menu.MenuItemSettings AvoidTracker = new Menu.MenuItemSettings(typeof(Avoid));
        private List<AvoidObject> _avoidObjects = new List<AvoidObject>(); 

        public Avoid()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    switch (hero.ChampionName)
                    {
                        case "Caitlyn": //W
                            _avoidObjects.Add(new AvoidObject("caitlyntrap", "CaitlynYordleTrap", 70.0f));
                            break;

                        case "Jinx": //E
                            _avoidObjects.Add(new AvoidObject("jinxmine", "JinxEMine", 80.0f));
                            break;

                        case "Karma": //RQ
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Kennen": //R
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "KogMaw": //Passive
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Malzahar": //W
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Morgana": //W
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Nasus": //W
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Nidalee": //W
                            _avoidObjects.Add(new AvoidObject("Nidalee_Spear", "", 70.0f));
                            break;

                        case "Rammus": //R
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Rumble": //R
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Shaco": //W
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Singed": //W
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Teemo": //R
                            _avoidObjects.Add(new AvoidObject("teemomushroom", "Noxious Trap", 80.0f));
                            break;

                        case "Viktor": //R
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;

                        case "Ziggs": //E
                            _avoidObjects.Add(new AvoidObject("ZiggsE_red.troy", "", 0.0f));
                            break;

                        case "Zilean": //Q
                            _avoidObjects.Add(new AvoidObject("", "", 0.0f));
                            break;
                    }
                }
            }

            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnIssueOrder += Obj_AI_Hero_OnIssueOrder;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
        }

        ~Avoid()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
#if TRACKERS
            return Tracker.Trackers.GetActive() && AvoidTracker.GetActive();
#else
            return AvoidTracker.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AvoidTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_AVOID_MAIN"), "SAssembliesTrackersAvoid"));
            AvoidTracker.MenuItems.Add(AvoidTracker.CreateActiveMenuItem("SAssembliesTrackersAvoidActive", () => new Avoid()));
            return AvoidTracker;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            
        }

        void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!IsActive())
                return;
        }

        void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;

            var obj = sender as Obj_AI_Base;
            if (obj != null)
            {
                
            }
        }

        class AvoidObject
        {
            public String ObjectName;
            public String BuffName;
            public float Radius;

            public AvoidObject(string buffName, string objectName, float radius)
            {
                BuffName = buffName;
                ObjectName = objectName;
                Radius = radius;
            }
        }
    }
}
