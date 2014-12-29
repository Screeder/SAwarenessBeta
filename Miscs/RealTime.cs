using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Miscs
{
    class RealTime
    {
        public static Menu.MenuItemSettings RealTimeMisc = new Menu.MenuItemSettings(typeof(RealTime));

        public RealTime()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~RealTime()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && RealTimeMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            RealTimeMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_REALTIME_MAIN"), "SAwarenessMiscsRealTime"));
            RealTimeMisc.MenuItems.Add(
                RealTimeMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsRealTimeActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return RealTimeMisc;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            Drawing.DrawText(Drawing.Width - 75, 75, System.Drawing.Color.LimeGreen, DateTime.Now.ToString("HH:mm:ss tt"));
        }
    }
}
