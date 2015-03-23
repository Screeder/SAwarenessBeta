using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SActivators.Activators.Items;

namespace SAssemblies.Activators
{
    class AutoDervishBlade
    {

        private static Items.Item _db = new Items.Item(3137);
        public static Menu.MenuItemSettings AutoDervishBladeActivator = new Menu.MenuItemSettings(typeof(AutoDervishBlade));

        public AutoDervishBlade()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoDervishBlade()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoDervishBladeActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoDervishBlade", "SAssembliesSActivatorsAutoDervishBlade", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoDervishBladeActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOITEMS_DEFENSIVE_CLEANSE_DERVISHBLADE_MAIN"), "SAssembliesActivatorsAutoItemsDefensiveCleanseDervishBlade"));
            AutoDervishBladeActivator.MenuItems.Add(
                AutoDervishBladeActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoItemsDefensiveCleanseDervishBladeActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoDervishBladeActivator;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !_db.IsReady())
                return;

            AutoItems.UpdateCCList();
            List<BuffInstance> buffList = Activator.GetActiveCcBuffs(AutoItems._buffs);

            if (buffList.Count() >=
                AutoDervishBladeActivator.GetMenuItem("SAwarenessActivatorDefensiveCleanseSelfConfigMinSpells")
                    .GetValue<Slider>()
                    .Value &&
                AutoDervishBladeActivator.GetMenuItem("SAwarenessActivatorDefensiveCleanseSelfDervishBlade")
                    .GetValue<bool>() &&
                Activator._lastItemCleanseUse + 1 < Game.Time)
            {
                var db = new Items.Item(3137, 0);
                if (db.IsReady())
                {
                    db.Cast();
                    Activator._lastItemCleanseUse = Game.Time;
                }
            }
        }
    }
}
