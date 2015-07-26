﻿using System;
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

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu, bool useExisitingMenu = false)
        {
            Language.SetLanguage();
            if (!useExisitingMenu)
            {
                Wards.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Wards", "SAssembliesWards"));
            }
            else
            {
                Wards.Menu = menu;
            }
            if (!useExisitingMenu) 
            {
                Wards.MenuItems.Add(Wards.CreateActiveMenuItem("SAssembliesWardsActive"));
            }
            return Wards;
        }
    }
}
