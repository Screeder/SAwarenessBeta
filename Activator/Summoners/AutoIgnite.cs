using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Activators
{
    class AutoIgnite
    {

        private static SpellSlot _ignite = SummonerSpells.GetIgniteSlot();
        public static Menu.MenuItemSettings AutoIgniteActivator = new Menu.MenuItemSettings(typeof(AutoIgnite));

        public AutoIgnite()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoIgnite()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoIgniteActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoIgnite", "SAssembliesSActivatorsAutoIgnite", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoIgniteActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOIGNITE_MAIN"), "SAssembliesActivatorsAutoIgnite"));
            AutoIgniteActivator.MenuItems.Add(
                AutoIgniteActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoIgniteActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoIgniteActivator;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || _ignite == SpellSlot.Unknown || ObjectManager.Player.Spellbook.GetSpell(_ignite).State != SpellState.Ready)
                return;

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero != null && hero.IsEnemy && hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 700)
                {
                    double igniteDmg = ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                    double regenpersec = (hero.FlatHPRegenMod + (hero.HPRegenRate * hero.Level));
                    double dmgafter = (igniteDmg - ((regenpersec * 5) / 2));
                    if (dmgafter > hero.Health)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_ignite, hero);
                    }
                }
            }
        }
    }
}
