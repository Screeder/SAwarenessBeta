using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Activators
{
    class AutoClairvoyance
    {

        private static SpellSlot _clairvoyance = SummonerSpells.GetClairvoyanceSlot();
        public static Menu.MenuItemSettings AutoClairvoyanceActivator = new Menu.MenuItemSettings(typeof(AutoClairvoyance));

        public AutoClairvoyance()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoClairvoyance()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoClairvoyanceActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoClairvoyance", "SAssembliesSActivatorsAutoClairvoyance", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoClairvoyanceActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOCLAIRVOYANCE_MAIN"), "SAssembliesActivatorsAutoClairvoyance"));
            AutoClairvoyanceActivator.MenuItems.Add(
                AutoClairvoyanceActivator.Menu.AddItem(
                    new MenuItem("SAssembliesSActivatorsAutoClairvoyanceMinAllies", Language.GetString("ACTIVATORS_AUTOEXHAUST_MIN_ALLIES")).SetValue(
                        new Slider(1, 4, 0))));
            AutoClairvoyanceActivator.MenuItems.Add(
                AutoClairvoyanceActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoClairvoyanceActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoClairvoyanceActivator;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || _clairvoyance == SpellSlot.Unknown || ObjectManager.Player.Spellbook.GetSpell(_clairvoyance).State != SpellState.Ready)
                return;

            int countA = ObjectManager.Player.ServerPosition.CountAlliesInRange(750);
            if (((ObjectManager.Player.Mana / ObjectManager.Player.MaxMana) * 100) < 50 &&
                countA >=
                    AutoClairvoyanceActivator.GetMenuItem(
                        "SAssembliesSActivatorsAutoClairvoyanceMinAllies").GetValue<Slider>().Value)
            {
                ObjectManager.Player.Spellbook.CastSpell(_clairvoyance);
            }
        }
    }
}
