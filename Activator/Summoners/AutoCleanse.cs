using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Activators
{
    class AutoCleanse
    {

        private static SpellSlot _cleanse = SummonerSpells.GetCleanseSlot();
        private List<BuffType> _buffs = new List<BuffType>(); 
        public static Menu.MenuItemSettings AutoCleanseActivator = new Menu.MenuItemSettings(typeof(AutoCleanse));
        public static Menu.MenuItemSettings AutoCleanseActivatorConfig = new Menu.MenuItemSettings();

        public AutoCleanse()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoCleanse()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoCleanseActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoCleanse", "SAssembliesSActivatorsAutoCleanse", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoCleanseActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOCLEANSE_MAIN"), "SAssembliesActivatorsAutoCleanse"));
            AutoCleanseActivatorConfig = AutoCleanseActivator.AddMenuItemSettings(Language.GetString("ACTIVATORS_AUTOCLEANSE_CONFIG_MAIN"), "SAssembliesActivatorsAutoCleanseConfig");
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigStun", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_STUN")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigSilence", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_SILENCE")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigTaunt", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_TAUNT")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigFear", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_FEAR")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigCharm", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_CHARM")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigBlind", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_BLIND")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigDisarm", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_DISARM")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigSlow", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_SLOW")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigCombatDehancer", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_COMBAT_DEHANCER")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigSnare", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_SNARE")).SetValue(false)));
            AutoCleanseActivatorConfig.MenuItems.Add(
                AutoCleanseActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigPoison", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_POISON")).SetValue(false)));
            AutoCleanseActivator.MenuItems.Add(
                AutoCleanseActivator.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoCleanseConfigMinSpells", Language.GetString("ACTIVATORS_ACTIVATOR_MIN_SPELLS")).SetValue(
                        new Slider(2, 10, 1))));
            AutoCleanseActivator.MenuItems.Add(
                AutoCleanseActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoCleanseActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoCleanseActivator;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || _cleanse == SpellSlot.Unknown || ObjectManager.Player.Spellbook.GetSpell(_cleanse).State != SpellState.Ready)
                return;

            _buffs.Clear();
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigStun")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Stun);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigSilence")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Silence);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigTaunt")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Taunt);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigFear")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Fear);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigCharm")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Charm);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigBlind")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Blind);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigDisarm")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Disarm);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigSlow")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Slow);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigCombatDehancer")
                    .GetValue<bool>())
                _buffs.Add(BuffType.CombatDehancer);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigSnare")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Snare);
            if (
                AutoCleanseActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoCleanseConfigPoison")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Poison);

            List<BuffInstance> buffList = Activator.GetActiveCcBuffs(_buffs);

            if (buffList.Count() >=
                AutoCleanseActivator.GetMenuItem(
                    "SAssembliesActivatorsAutoCleanseMinSpells").GetValue<Slider>().Value &&
                Activator._lastItemCleanseUse + 1 < Game.Time)
            {
                ObjectManager.Player.Spellbook.CastSpell(_cleanse);
                Activator._lastItemCleanseUse = Game.Time;
            }
        }
    }
}
