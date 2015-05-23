using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAssemblies.Miscs
{
    class MinionLocation
    {
        public static Menu.MenuItemSettings MinionLocationMisc = new Menu.MenuItemSettings(typeof(MinionLocation));
        private static float startTime = Game.ClockTime;

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
#if MISCS
            return Misc.Miscs.GetActive() && MinionLocationMisc.GetActive();
#else
            return MinionLocationMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            MinionLocationMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_MINIONLOCATION_MAIN"), "SAssembliesMiscsMinionLocation"));
            MinionLocationMisc.MenuItems.Add(
                MinionLocationMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsMinionLocationActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return MinionLocationMisc;
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (!IsActive() || Game.MapId != GameMapId.SummonersRift)
                return;

            float timer = (RealTime() % 60 > 30 ? RealTime() - 30 : RealTime());
            float first = 325 * (timer % 60);
            float last = 325 * ((timer - 6) % 60);
            if (ObjectManager.Player.Team != GameObjectTeam.Order)
            {
                if (1720 + last < 14527)
                {
                    Drawing.DrawLine(Drawing.WorldToMinimap(new Vector3(1200, 1900 + first, 100)), 
                        Drawing.WorldToMinimap(new Vector3(1200, 1900 + last, 100)), 2, System.Drawing.Color.White);
                }
                if (11511 + (-22f / 30f) * last > (14279f / 2f) && 11776 + (-22f / 30f) * last > (14527 / 2))
                {
                    Drawing.DrawLine(Drawing.WorldToMinimap(new Vector3(1600 + (22f / 30f) * first, 1800 + (22f / 30f) * first, 100)),
                        Drawing.WorldToMinimap(new Vector3(1600 + (22f / 30f) * last, 1800 + (22f / 30f) * last, 100)), 2, System.Drawing.Color.White);
                }
                if (1546 + last < 14527)
                {
                    Drawing.DrawLine(Drawing.WorldToMinimap(new Vector3(1895 + first, 1200, 100)), 
                        Drawing.WorldToMinimap(new Vector3(1895 + last, 1200, 100)), 2, System.Drawing.Color.White);
                }
            }
            if (ObjectManager.Player.Team != GameObjectTeam.Chaos)
            {
                if (12451 + -1 * last > 0)
                {
                    Drawing.DrawLine(Drawing.WorldToMinimap(new Vector3(12451 + -1 * first, 13570, 100)),
                        Drawing.WorldToMinimap(new Vector3(12451 + -1 * last, 13570, 100)), 2, System.Drawing.Color.White);
                }
                if (11511 + (-22f / 30f) * last > (14279f / 2f) && 11776 + (-22f / 30f) * last > (14527f / 2f))
                {
                    Drawing.DrawLine(Drawing.WorldToMinimap(new Vector3(12820 + (-22f / 30f) * first, 12780 + (-22f / 30f) * first, 100)),
                        Drawing.WorldToMinimap(new Vector3(12780 + (-22f / 30f) * last, 12820 + (-22f / 30f) * last, 100)), 2, System.Drawing.Color.White);
                }
                if (12760 + -1 * last > 0)
                {
                    Drawing.DrawLine(Drawing.WorldToMinimap(new Vector3(13550, 12760 + -1 * first, 100)),
                        Drawing.WorldToMinimap(new Vector3(13550, 12760 + -1 * last, 100)), 2, System.Drawing.Color.White);
                }
            }
        }

        private float RealTime()
        {
            return Game.ClockTime -  startTime;
        }
    }
}
