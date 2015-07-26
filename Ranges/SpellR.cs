using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Ranges
{
    class SpellR
    {
        public static Menu.MenuItemSettings SpellRRange = new Menu.MenuItemSettings(typeof(SpellR));

        public SpellR()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~SpellR()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
#if RANGES
            return Range.Ranges.GetActive() && SpellRRange.GetActive();
#else
            return SpellRRange.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SpellRRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_SPELLR_MAIN"), "SAssembliesRangesSpellR"));
            SpellRRange.MenuItems.Add(
                SpellRRange.Menu.AddItem(new MenuItem("SAssembliesRangesSpellRMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            SpellRRange.MenuItems.Add(
                SpellRRange.Menu.AddItem(new MenuItem("SAssembliesRangesSpellRColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            SpellRRange.MenuItems.Add(
                SpellRRange.Menu.AddItem(new MenuItem("SAssembliesRangesSpellRColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.IndianRed)));
            SpellRRange.MenuItems.Add(SpellRRange.CreateActiveMenuItem("SAssembliesRangesSpellRActive", () => new SpellR()));
            return SpellRRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            var mode = SpellRRange.GetMenuItem("SAssembliesRangesSpellRMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange, SpellRRange.GetMenuItem("SAssembliesRangesSpellRColorMe").GetValue<Color>());
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange, SpellRRange.GetMenuItem("SAssembliesRangesSpellRColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange, SpellRRange.GetMenuItem("SAssembliesRangesSpellRColorMe").GetValue<Color>());
                    }
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange, SpellRRange.GetMenuItem("SAssembliesRangesSpellRColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
