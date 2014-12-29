using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Detectors
{
    class Detector
    {
        public static Menu.MenuItemSettings Detectors = new Menu.MenuItemSettings();

        private Detector()
        {

        }

        ~Detector()
        {
            
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAwareness", "SAwareness", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            Language.SetLanguage();
            Detectors.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("DETECTORS_DETECTOR_MAIN"), "SAwarenessDetectors"));
            Detectors.MenuItems.Add(Detectors.Menu.AddItem(new MenuItem("SAwarenessDetectorsActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return Detectors;
        }
    }
}
