using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAssemblies;
using Menu = SAssemblies.Menu;

namespace SActivators.Activators.Items
{
    class AutoItems
    {
        public static List<BuffType> _buffs = new List<BuffType>(); 

        public static Menu.MenuItemSettings AutoItemsActivator = new Menu.MenuItemSettings();
        public static Menu.MenuItemSettings AutoItemsActivatorOffensive = new Menu.MenuItemSettings();
        public static Menu.MenuItemSettings AutoItemsActivatorOffensiveAd = new Menu.MenuItemSettings();
        public static Menu.MenuItemSettings AutoItemsActivatorOffensiveAp = new Menu.MenuItemSettings();
        public static Menu.MenuItemSettings AutoItemsActivatorDefensive = new Menu.MenuItemSettings();
        public static Menu.MenuItemSettings AutoItemsActivatorCleanse = new Menu.MenuItemSettings();
        public static Menu.MenuItemSettings AutoItemsActivatorCleanseConfig = new Menu.MenuItemSettings();

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoItems", "SAssembliesSActivatorsAutoItems", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            Language.SetLanguage();
            AutoItemsActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoItems", "SAssembliesActivatorsAutoItems"));
            AutoItemsActivatorOffensive = AutoItemsActivator.AddMenuItemSettings("Offensive", "SAssembliesActivatorsAutoItemsOffensive");
            AutoItemsActivatorOffensiveAd = AutoItemsActivatorOffensive.AddMenuItemSettings("Ad", "SAssembliesActivatorsAutoItemsOffensiveAd");
            AutoItemsActivatorOffensiveAp = AutoItemsActivatorOffensive.AddMenuItemSettings("Ap", "SAssembliesActivatorsAutoItemsOffensiveAp");
            AutoItemsActivatorOffensive.MenuItems.Add(
                AutoItemsActivatorOffensive.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsOffensiveKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(32,
                        KeyBindType.Press))));
            AutoItemsActivatorDefensive = AutoItemsActivator.AddMenuItemSettings("Defensive", "SAssembliesActivatorsAutoItemsDefensive");
            AutoItemsActivatorCleanse = AutoItemsActivatorDefensive.AddMenuItemSettings("Cleanse", "SAssembliesActivatorsAutoItemsCleanse");
            AutoItemsActivatorCleanseConfig.Menu =
                AutoItemsActivatorCleanse.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Cleanse Config",
                    "SAssembliesActivatorsAutoItemsCleanseConfig"));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigStun", "Stun").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigSilence", "Silence").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigTaunt", "Taunt").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigFear", "Fear").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigCharm", "Charm").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigBlind", "Blind").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigDisarm", "Disarm").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigSuppress", "Suppress").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigSlow", "Slow").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigCombatDehancer", "Combat Dehancer")
                        .SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigSnare", "Snare").SetValue(false)));
            AutoItemsActivatorCleanseConfig.MenuItems.Add(
                AutoItemsActivatorCleanseConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoItemsCleanseConfigPoison", "Posion").SetValue(false)));
            AutoItemsActivator.MenuItems.Add(AutoItemsActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoItemsActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoItemsActivator;
        }

        public static void UpdateCCList()
        {
            _buffs.Clear();
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigStun")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Stun);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigSilence")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Silence);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigTaunt")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Taunt);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigFear")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Fear);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigCharm")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Charm);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigBlind")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Blind);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigDisarm")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Disarm);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigSuppress")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Suppression);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigSlow")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Slow);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigCombatDehancer")
                    .GetValue<bool>())
                _buffs.Add(BuffType.CombatDehancer);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigSnare")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Snare);
            if (
                AutoItemsActivatorCleanseConfig.GetMenuItem("SAssembliesActivatorsAutoItemsCleanseConfigPoison")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Poison);
        }
    }
}
