using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Activators
{
    class AutoHealSummoner
    {

        private static SpellSlot _heal;
        public static Menu.MenuItemSettings AutoHealSummonerActivator = new Menu.MenuItemSettings(typeof(AutoHealSummoner));

        public AutoHealSummoner()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoHealSummoner()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoHealSummonerActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoHeal", "SAssembliesSActivatorsAutoHealSummoner", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoHealSummonerActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOHEAL_MAIN"), "SAssembliesActivatorsAutoHealSummoner"));
            AutoHealSummonerActivator.MenuItems.Add(
                AutoHealSummonerActivator.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoHealSummonerPercent", Language.GetString("GLOBAL_MODE_PERCENT")).SetValue(
                        new Slider(20, 100, 1))));
            AutoHealSummonerActivator.MenuItems.Add(
                AutoHealSummonerActivator.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoHealSummonerAllyActive", Language.GetString("ACTIVATORS_AUTOHEALSUMMONER_ALLY")).SetValue(false)));
            AutoHealSummonerActivator.MenuItems.Add(
                AutoHealSummonerActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoHealSummonerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoHealSummonerActivator;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || _heal == SpellSlot.Unknown || ObjectManager.Player.Spellbook.GetSpell(_heal).State != SpellState.Ready ||
                ObjectManager.Player.IsDead || ObjectManager.Player.InFountain() ||
                ObjectManager.Player.HasBuff("Recall") || ObjectManager.Player.HasBuff("SummonerTeleport") ||
                ObjectManager.Player.HasBuff("RecallImproved") ||
                ObjectManager.Player.ServerPosition.CountEnemiesInRange(1500) > 0)
                return;

            if (
            AutoHealSummonerActivator.GetMenuItem("SAssembliesActivatorsAutoHealSummonerAllyActive")
                .GetValue<bool>())
            {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (!hero.IsEnemy && !hero.IsDead &&
                        hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 700)
                    {
                        if (((hero.Health / hero.MaxHealth) * 100) <
                            AutoHealSummonerActivator.GetMenuItem(
                                "SAssembliesActivatorsAutoHealSummonerPercent").GetValue<Slider>().Value)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(_heal);
                        }
                        foreach (var damage in Activator.Damages)
                        {
                            if (damage.Key.NetworkId != ObjectManager.Player.NetworkId)
                                continue;

                            if (Activator.CalcMaxDamage(damage.Key) >= damage.Key.Health)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(_heal);
                            }
                        }
                    }
                }
            }
            if (((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100) <
                AutoHealSummonerActivator.GetMenuItem("SAssembliesActivatorsAutoHealSummonerPercent")
                    .GetValue<Slider>()
                    .Value)
            {
                ObjectManager.Player.Spellbook.CastSpell(_heal);
            }
            foreach (var damage in Activator.Damages)
            {
                if (damage.Key.NetworkId != ObjectManager.Player.NetworkId)
                    continue;

                if (Activator.CalcMaxDamage(damage.Key) >= damage.Key.Health)
                {
                    ObjectManager.Player.Spellbook.CastSpell(_heal);
                }
            }
        }
    }
}
