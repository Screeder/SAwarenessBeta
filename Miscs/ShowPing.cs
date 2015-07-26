using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Miscs
{
    class ShowPing
    {
        public static Menu.MenuItemSettings ShowPingMisc = new Menu.MenuItemSettings(typeof(ShowPing));

        public ShowPing()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~ShowPing()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && ShowPingMisc.GetActive();
#else
            return ShowPingMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            ShowPingMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_SHOWPING_MAIN"), "SAssembliesMiscsShowPing"));
            ShowPingMisc.MenuItems.Add(ShowPingMisc.CreateActiveMenuItem("SAssembliesMiscsShowPingActive", () => new ShowPing()));
            return ShowPingMisc;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            //foreach (var objectType in ObjectManager.Get<GameObject>())
            //{
            //    var pos = Drawing.WorldToScreen(objectType.Position);
            //    Drawing.DrawText(pos.X, pos.Y, Color.Blue, objectType.Name);
            //    //Drawing.DrawText(pos.X, pos.Y, Color.Blue, objectType.Type.ToString());
            //}
            //Console.WriteLine("{0}, {1}, {2}", ObjectManager.Player.ServerPosition.X, ObjectManager.Player.ServerPosition.Y, ObjectManager.Player.ServerPosition.Z);
            Drawing.DrawText(Drawing.Width - 75, 90, System.Drawing.Color.LimeGreen, Game.Ping.ToString() + "ms");
        }
    }
}
