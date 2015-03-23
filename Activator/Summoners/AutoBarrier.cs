using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Activators
{
    class AutoBarrier
    {

        private static SpellSlot _barrier = SummonerSpells.GetBarrierSlot();
        public static Menu.MenuItemSettings AutoBarrierActivator = new Menu.MenuItemSettings(typeof(AutoBarrier));

        public AutoBarrier()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoBarrier()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoBarrierActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoBarrier", "SAssembliesSActivatorsAutoBarrier", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoBarrierActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOBARRIER_MAIN"), "SAssembliesActivatorsAutoBarrier"));
            AutoBarrierActivator.MenuItems.Add(
                AutoBarrierActivator.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoBarrierPercent", Language.GetString("GLOBAL_MODE_PERCENT")).SetValue(
                        new Slider(20, 100, 1))));
            AutoBarrierActivator.MenuItems.Add(
                AutoBarrierActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoBarrierActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoBarrierActivator;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || _barrier == SpellSlot.Unknown || ObjectManager.Player.Spellbook.GetSpell(_barrier).State != SpellState.Ready ||
                ObjectManager.Player.IsDead || ObjectManager.Player.InFountain() ||
                ObjectManager.Player.HasBuff("Recall") || ObjectManager.Player.HasBuff("SummonerTeleport") ||
                ObjectManager.Player.HasBuff("RecallImproved") ||
                ObjectManager.Player.ServerPosition.CountEnemiesInRange(1500) > 0)
                return;

            if (((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100) <
                AutoBarrierActivator.GetMenuItem("SAssembliesActivatorsAutoBarrierPercent")
                    .GetValue<Slider>()
                    .Value)
            {
                ObjectManager.Player.Spellbook.CastSpell(_barrier);
            }
            foreach (var damage in Activator.Damages)
            {
                if (damage.Key.NetworkId != ObjectManager.Player.NetworkId)
                    continue;

                if (Activator.CalcMaxDamage(damage.Key) >= damage.Key.Health)
                {
                    ObjectManager.Player.Spellbook.CastSpell(_barrier);
                }
            }
        }
    }
}
