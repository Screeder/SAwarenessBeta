using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Miscs
{
    class MinionLocation
    {
        public static Menu.MenuItemSettings MinionLocationMisc = new Menu.MenuItemSettings(typeof(MinionLocation));

        public MinionLocation()
        {
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        ~MinionLocation()
        {
            Drawing.OnEndScene -= Drawing_OnEndScene;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && MinionLocationMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            MinionLocationMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_MINIONLOCATION_MAIN"), "SAwarenessMiscsMinionLocation"));
            MinionLocationMisc.MenuItems.Add(
                MinionLocationMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsMinionLocationActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return MinionLocationMisc;
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (!IsActive())
                return;

            float timer = (Game.ClockTime % 60 > 30 ? Game.ClockTime - 30 : Game.ClockTime);
            float first = 325 * (timer % 60) + 4000;
            float last = 325 * ((timer - 6) % 60) + 4000;
            if (ObjectManager.Player.Team == GameObjectTeam.Order)
            {
                Utility.DrawCircle(new Vector3(917, 1720 + first, 124), 300, System.Drawing.Color.White, 2, 30, true);
                if (1720 + last < 14527 + 4000)
                {
                    Utility.DrawCircle(new Vector3(917, 1720 + last, 124), 300, System.Drawing.Color.White, 2, 30, true);
                    Drawing.DrawLine(Drawing.WorldToMinimap(new Vector3(917, 1720 + first, 100)), Drawing.WorldToMinimap(new Vector3(917, 1720 + last, 100)), 2, System.Drawing.Color.White);
                }
                Utility.DrawCircle(new Vector3(1446 + (22 / 30) * first, 1664 + (22 / 30) * first, 118), 300,
                    System.Drawing.Color.White, 5, 30, true);
                Utility.DrawCircle(new Vector3(1446 + (22 / 30) * last, 1664 + (22 / 30) * last, 118), 300,
                    System.Drawing.Color.White, 5, 30, true);
                if (1446 + (22 / 30) * last < (14279 / 2) && 1664 + (22 / 30) * last < (14527 / 2))
                {
                    Drawing.DrawLine(Drawing.WorldToMinimap(new Vector3(1446 + (22 / 30) * first, 1664 + (22 / 30) * first, 100)),
                        Drawing.WorldToMinimap(new Vector3(1446 + (22 / 30) * last, 1664 + (22 / 30) * last, 100)), 2, System.Drawing.Color.White);
                }
                Utility.DrawCircle(new Vector3(1546 + first, 1314, 124), 300, System.Drawing.Color.White, 2, 30, true);
                if (1546 + last < 14527 + 4000)
                {
                    Utility.DrawCircle(new Vector3(1546 + last, 1314, 124), 300, System.Drawing.Color.White, 2, 30, true);
                    Drawing.DrawLine(Drawing.WorldToMinimap(new Vector3(1546 + first, 1314, 100)), Drawing.WorldToMinimap(new Vector3(1546 + last, 1314, 100)), 2, System.Drawing.Color.White);
                }
            }
        }
    }
}
