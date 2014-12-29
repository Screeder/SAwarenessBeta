using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace SAwareness.Healths
{
    internal class Health
    {
        public static Menu.MenuItemSettings Healths = new Menu.MenuItemSettings();

        public Health()
        {

        }

        ~Health()
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
            Healths.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("HEALTHS_HEALTH_MAIN"), "SAwarenessHealths"));
            Healths.MenuItems.Add(
                Healths.Menu.AddItem(new MenuItem("SAwarenessHealthsMode", Language.GetString("GLOBAL_MODE")).SetValue(new StringList(new[] { Language.GetString("HEALTHS_MODE_PERCENT"), Language.GetString("HEALTHS_MODE_VALUE") }))));
            Healths.MenuItems.Add(Healths.Menu.AddItem(new MenuItem("SAwarenessHealthsActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return Healths;
        }

        public class HealthConf
        {
            public Object Obj;
            public Render.Text Text;

            public HealthConf(object obj, Render.Text text)
            {
                Obj = obj;
                Text = text;
            }
        }
    }
}
