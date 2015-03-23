using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAssemblies.Miscs
{
    class Misc
    {
        public static Menu.MenuItemSettings Miscs = new Menu.MenuItemSettings();

        private Misc()
        {

        }

        ~Misc()
        {
            
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SMiscs", "SAssembliesSMiscs", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            Language.SetLanguage();
            Miscs.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_MISC_MAIN"), "SAssembliesMiscs"));
            Miscs.MenuItems.Add(Miscs.Menu.AddItem(new MenuItem("SAssembliesMiscsActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return Miscs;
        }
    }
}
