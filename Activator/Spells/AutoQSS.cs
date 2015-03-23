using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Activators
{
    class AutoQSSSpell
    {
        private static Qss _qss;
        private readonly List<BuffType> _buffs = new List<BuffType>();
        public static Menu.MenuItemSettings AutoQSSSpellActivator = new Menu.MenuItemSettings(typeof(AutoQSSSpell));
        public static Menu.MenuItemSettings AutoQSSSpellActivatorConfig = new Menu.MenuItemSettings();

        public AutoQSSSpell()
        {
            if (_qss == null)
            {
                Init();
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoQSSSpell()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoQSSSpellActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoQSSActivator", "SAssembliesSActivatorsAutoQSSActivator", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoQSSSpellActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOQSS_MAIN"), "SAssembliesActivatorsAutoQSSSpell"));
            AutoQSSSpellActivator.MenuItems.Add(
                AutoQSSSpellActivator.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellMinSpells", Language.GetString("ACTIVATORS_ACTIVATOR_MIN_SPELLS")).SetValue(
                        new Slider(2, 10, 1))));
            AutoQSSSpellActivatorConfig = AutoQSSSpellActivator.AddMenuItemSettings(Language.GetString("ACTIVATORS_AUTOQSS_CONFIG_MAIN"), "SAssembliesActivatorsAutoQSSSpellConfig");
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigStun", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_STUN")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigSilence", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_SILENCE")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigTaunt", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_TAUNT")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigFear", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_FEAR")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigCharm", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_CHARM")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigBlind", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_BLIND")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigDisarm", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_DISARM")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigSuppress", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_SUPPRESS")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigSlow", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_SLOW")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigCombatDehancer", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_COMBAT_DEHANCER")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigSnare", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_SNARE")).SetValue(false)));
            AutoQSSSpellActivatorConfig.MenuItems.Add(
                AutoQSSSpellActivatorConfig.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoQSSSpellConfigPoison", Language.GetString("ACTIVATORS_ACTIVATOR_CONFIG_POISON")).SetValue(false)));
            AutoQSSSpellActivator.MenuItems.Add(
                AutoQSSSpellActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoQSSSpellActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoQSSSpellActivator;
        }

        private static void Init()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Alistar":
                    _qss = new Qss(SpellSlot.R);
                    break;

                case "Gankplank":
                    _qss = new Qss(SpellSlot.W);
                    break;

                case "Olaf":
                    _qss = new Qss(SpellSlot.R);
                    break;

                default:
                    return;
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || _qss == null || ObjectManager.Player.Spellbook.GetSpell(_qss.SpellSlot).State != SpellState.Ready)
                return;

            CreateBuffList();

            List<BuffInstance> buffList = ActivatorOld.GetActiveCcBuffs(_buffs);

            if (buffList.Count() >=
                AutoQSSActivator.GetMenuItem("SAssembliesActivatorsAutoQSSMinSpells")
                    .GetValue<Slider>()
                    .Value)
            {
                Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(ObjectManager.Player.NetworkId, _qss.SpellSlot)).Send();
            }

            if (ObjectManager.Player.HasBuffOfType(BuffType.Slow))
            {
                Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(ObjectManager.Player.NetworkId, _qss.SpellSlot)).Send();              
            }
        }

        private void CreateBuffList()
        {
            _buffs.Clear();
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigStun")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Stun);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigSilence")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Silence);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigTaunt")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Taunt);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigFear")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Fear);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigCharm")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Charm);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigBlind")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Blind);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigDisarm")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Disarm);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigSuppress")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Suppression);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigSlow")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Slow);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigCombatDehancer").GetValue<bool>())
                _buffs.Add(BuffType.CombatDehancer);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigSnare")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Snare);
            if (
                AutoQSSActivatorConfig.GetMenuItem("SAssembliesActivatorsAutoQSSSpellConfigPoison")
                    .GetValue<bool>())
                _buffs.Add(BuffType.Poison);
        }

        private class Qss
        {
            public SpellSlot SpellSlot;

            public Qss(SpellSlot spellSlot)
            {
                SpellSlot = spellSlot;
            }
        }
    }
}
