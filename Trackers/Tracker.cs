using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace SAssemblies.Trackers
{
    class Tracker
    {
        public static Menu.MenuItemSettings Trackers = new Menu.MenuItemSettings();

        private Tracker()
        {

        }

        ~Tracker()
        {
            
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("STrackers", "SAssembliesSTrackers", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            Language.SetLanguage();
            Trackers.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_TRACKER_MAIN"), "SAssembliesTracker"));
            Trackers.MenuItems.Add(Trackers.Menu.AddItem(new MenuItem("SAssembliesTrackerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return Trackers;
        }
    }
}
