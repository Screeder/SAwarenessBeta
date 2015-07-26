using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAssemblies.Miscs
{
    class PingerName
    {
        public static Menu.MenuItemSettings PingerNameMisc = new Menu.MenuItemSettings(typeof(PingerName));

        List<PingInfo> pingInfo = new List<PingInfo>();

        public PingerName()
        {
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnPing += Game_OnPing;
        }

        ~PingerName()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
            Game.OnPing -= Game_OnPing;
            pingInfo = null;
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && PingerNameMisc.GetActive();
#else
            return PingerNameMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            PingerNameMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_PINGERNAME_MAIN"), "SAssembliesMiscsPingerName"));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if(hero.IsEnemy || hero.IsMe)
                    continue;

                PingerNameMisc.MenuItems.Add(
                    PingerNameMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsPingerNameIgnore" + hero.Name, Language.GetString("MISCS_PINGERNAME_IGNORE") + hero.Name).SetValue(false).DontSave()));
            }
            PingerNameMisc.MenuItems.Add(PingerNameMisc.CreateActiveMenuItem("SAssembliesMiscsPingerNameActive", () => new PingerName()));
            return PingerNameMisc;
        }

        void Game_OnPing(GamePingEventArgs args)
        {
            if (!IsActive())
                return;

            Obj_AI_Hero hero = args.Source as Obj_AI_Hero;
            if (hero != null && hero.IsValid)
            {
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (ally.Name.Equals(hero.Name))
                        args.Process = false;
                }
                pingInfo.Add(new PingInfo(hero.ChampionName, args.Position, Game.Time + 2));
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            foreach (var info in pingInfo.ToList())
            {
                if (info.Time < Game.Time)
                {
                    pingInfo.Remove(info);
                    continue;
                }
                Vector2 screenPos = Drawing.WorldToScreen(new Vector3(info.Pos, NavMesh.GetHeightForPosition(info.Pos.X, info.Pos.Y)));
                if (screenPos.IsOnScreen())
                {
                    Drawing.DrawText(screenPos.X - 25, screenPos.Y, System.Drawing.Color.DeepSkyBlue, info.Name);
                }
            }
        }

        private class PingInfo
        {
            public Vector2 Pos;
            public String Name;
            public float Time;

            public PingInfo(String name, Vector2 pos, float time)
            {
                Name = name;
                Pos = pos;
                Time = time;
            }
        }
    }
}
