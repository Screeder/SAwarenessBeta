using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SAssemblies;
using SharpDX;
using Color = System.Drawing.Color;
using Menu = SAssemblies.Menu;

namespace SAssemblies.Wards
{
    internal class Ward
    {

        public static Menu.MenuItemSettings Wards = new Menu.MenuItemSettings();

        private Ward()
        {

        }

        ~Ward()
        {
            
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SWards", "SAssembliesSWards", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            Language.SetLanguage();
            Wards.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Wards", "SAssembliesWards"));
            Wards.MenuItems.Add(Wards.Menu.AddItem(new MenuItem("SAssembliesWardsActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return Wards;
        }
    }
}
