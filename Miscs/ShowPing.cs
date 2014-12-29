using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Miscs
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
            return Misc.Miscs.GetActive() && ShowPingMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            ShowPingMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_SHOWPING_MAIN"), "SAwarenessMiscsShowPing"));
            ShowPingMisc.MenuItems.Add(
                ShowPingMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsShowPingActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return ShowPingMisc;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            Drawing.DrawText(Drawing.Width - 75, 90, System.Drawing.Color.LimeGreen, Game.Ping.ToString() + "ms");
        }
    }
}
