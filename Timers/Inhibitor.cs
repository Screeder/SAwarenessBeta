using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Timers
{
    class Inhibitor
    {
        public static Menu.MenuItemSettings InhibitorTimer = new Menu.MenuItemSettings(typeof(Inhibitor));

        private static readonly Utility.Map GMap = Utility.Map.GetMap();
        private static InhibitorObject _inhibitors;

        public Inhibitor()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            InitInhibitorObjects();
        }

        ~Inhibitor()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;

            _inhibitors = null;
        }

        public bool IsActive()
        {
            return Timer.Timers.GetActive() && InhibitorTimer.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            InhibitorTimer.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TIMERS_INHIBITOR_MAIN"), "SAwarenessTimersInhibitor"));
            InhibitorTimer.MenuItems.Add(
                InhibitorTimer.Menu.AddItem(new MenuItem("SAwarenessTimersInhibitorActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return InhibitorTimer;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;

            if (InhibitorTimer.GetActive())
            {
                if (_inhibitors == null || _inhibitors.Inhibitors == null)
                    return;
                foreach (InhibitorObject inhibitor in _inhibitors.Inhibitors)
                {
                    if (inhibitor.Obj.Health > 0)
                    {
                        inhibitor.Locked = false;
                        inhibitor.NextRespawnTime = 0;
                        inhibitor.Called = false;
                    }
                    else if (inhibitor.Obj.Health < 1 && inhibitor.Locked == false)
                    {
                        inhibitor.Locked = true;
                        inhibitor.NextRespawnTime = inhibitor.RespawnTime + (int)Game.ClockTime;
                    }
                }
            }

            /////

            if (InhibitorTimer.GetActive())
            {
                if (_inhibitors.Inhibitors == null)
                    return;
                foreach (InhibitorObject inhibitor in _inhibitors.Inhibitors)
                {
                    if (inhibitor.Locked)
                    {
                        if (inhibitor.NextRespawnTime <= 0)
                            continue;
                        int time = Timer.Timers.GetMenuItem("SAwarenessTimersRemindTime").GetValue<Slider>().Value;
                        if (!inhibitor.Called && inhibitor.NextRespawnTime - (int)Game.ClockTime <= time &&
                            inhibitor.NextRespawnTime - (int)Game.ClockTime >= time - 1)
                        {
                            inhibitor.Called = true;
                            Timer.PingAndCall("Inhibitor respawns in " + time + " seconds!", inhibitor.Obj.Position);
                        }
                    }
                }
            }
        }

        public void InitInhibitorObjects()
        {
            _inhibitors = new InhibitorObject();
            foreach (Obj_BarracksDampener inhib in ObjectManager.Get<Obj_BarracksDampener>())
            {
                _inhibitors.Inhibitors.Add(new InhibitorObject(inhib));
            }
        }

        public class InhibitorObject
        {
            public bool Called;
            public List<InhibitorObject> Inhibitors;
            public bool Locked;
            public int NextRespawnTime;
            public Obj_BarracksDampener Obj;
            public int RespawnTime;
            public int SpawnTime;
            public Render.Text Text;

            public InhibitorObject()
            {
                Inhibitors = new List<InhibitorObject>();
            }

            public InhibitorObject(Obj_BarracksDampener obj)
            {
                Obj = obj;
                SpawnTime = (int)Game.ClockTime;
                RespawnTime = 300;
                NextRespawnTime = 0;
                Locked = false;
                Called = false;
                Text = new Render.Text(0, 0, "", 12, new ColorBGRA(Color4.White));
                Text.TextUpdate = delegate
                {
                    return (NextRespawnTime - (int)Game.ClockTime).ToString();
                };
                Text.PositionUpdate = delegate
                {
                    if (Obj.Position.Length().Equals(0.0f))
                        return new Vector2(0, 0);
                    Vector2 sPos = Drawing.WorldToMinimap(Obj.Position);
                    return new Vector2(sPos.X, sPos.Y);
                };
                Text.VisibleCondition = sender =>
                {
                    return Timer.Timers.GetActive() && InhibitorTimer.GetActive() && NextRespawnTime > 0;
                };
                Text.OutLined = true;
                Text.Centered = true;
                Text.Add();
            }
        }
    }
}
