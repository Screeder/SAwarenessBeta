using System;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Ranges
{
    internal class Range
    {
        public static Menu.MenuItemSettings Ranges = new Menu.MenuItemSettings();

        private Range()
        {

        }

        ~Range()
        {
            
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SRanges", "SAwarenessSRanges", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu, bool useExisitingMenu = false)
        {
            Language.SetLanguage();
            if (!useExisitingMenu)
            {
                Ranges.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_RANGE_MAIN"), "SAwarenessRanges"));
            }
            else
            {
                Ranges.Menu = menu;
            }
            if (!useExisitingMenu)
            {
                Ranges.MenuItems.Add(Ranges.Menu.AddItem(new MenuItem("SAwarenessRangesActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            }
            return Ranges;
        }
    }
}