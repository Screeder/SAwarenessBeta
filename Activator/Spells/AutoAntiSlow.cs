using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Activators
{
    class AutoAntiSlow
    {
        private static AntiSlow _antiSlow;
        public static Menu.MenuItemSettings AutoAntiSlowActivator = new Menu.MenuItemSettings(typeof(AutoAntiSlow));

        public AutoAntiSlow()
        {
            if (_antiSlow == null)
            {
                Init();
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoAntiSlow()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoAntiSlowActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoAntiSlow", "SAssembliesSActivatorsAutoAntiSlow", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoAntiSlowActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOANTISLOW_MAIN"), "SAssembliesActivatorsAutoAntiSlow"));
            AutoAntiSlowActivator.MenuItems.Add(
                AutoAntiSlowActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoAntiSlowMode", Language.GetString("GLOBAL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("GLOBAL_MODE_MANUAL"), 
                    Language.GetString("GLOBAL_MODE_AUTOMATIC")
                }))));
            AutoAntiSlowActivator.MenuItems.Add(
                AutoAntiSlowActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoAntiSlowKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(32, KeyBindType.Press))));
            AutoAntiSlowActivator.MenuItems.Add(
                AutoAntiSlowActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoAntiSlowActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoAntiSlowActivator;
        }

        private static void Init()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Evelynn":
                    _antiSlow = new AntiSlow(SpellSlot.W);
                    break;

                case "Garen":
                    _antiSlow = new AntiSlow(SpellSlot.Q);
                    break;

                case "MasterYi":
                    _antiSlow = new AntiSlow(SpellSlot.R);
                    break;

                default:
                    return;
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || _antiSlow == null || ObjectManager.Player.Spellbook.GetSpell(_antiSlow.SpellSlot).State != SpellState.Ready)
                return;

            var mode =
                AutoAntiSlowActivator.GetMenuItem("SAssembliesActivatorsAutoAntiSlowMode")
                    .GetValue<StringList>();

            if (mode.SelectedIndex == 0 &&
                    AutoAntiSlowActivator.GetMenuItem("SAssembliesActivatorsAutoAntiSlowKey").GetValue<KeyBind>().Active ||
                    mode.SelectedIndex == 1)
            {
                if (ObjectManager.Player.HasBuffOfType(BuffType.Slow))
                {
                    ObjectManager.Player.Spellbook.CastSpell(_antiSlow.SpellSlot);
                }
            }
        }

        private class AntiSlow
        {
            public SpellSlot SpellSlot;

            public AntiSlow(SpellSlot spellSlot)
            {
                SpellSlot = spellSlot;
            }
        }
    }
}
