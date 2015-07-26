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
    class SpellW
    {
        public static Menu.MenuItemSettings SpellWRange = new Menu.MenuItemSettings(typeof(SpellW));

        public SpellW()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~SpellW()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
#if RANGES
            return Range.Ranges.GetActive() && SpellWRange.GetActive();
#else
            return SpellWRange.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SpellWRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_SPELLW_MAIN"), "SAssembliesRangesSpellW"));
            SpellWRange.MenuItems.Add(
                SpellWRange.Menu.AddItem(new MenuItem("SAssembliesRangesSpellWMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            SpellWRange.MenuItems.Add(
                SpellWRange.Menu.AddItem(new MenuItem("SAssembliesRangesSpellWColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            SpellWRange.MenuItems.Add(
                SpellWRange.Menu.AddItem(new MenuItem("SAssembliesRangesSpellWColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.IndianRed)));
            SpellWRange.MenuItems.Add(SpellWRange.CreateActiveMenuItem("SAssembliesRangesSpellWActive", () => new SpellW()));
            return SpellWRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            var mode = SpellWRange.GetMenuItem("SAssembliesRangesSpellWMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange, SpellWRange.GetMenuItem("SAssembliesRangesSpellWColorMe").GetValue<Color>());
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange, SpellWRange.GetMenuItem("SAssembliesRangesSpellWColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange, SpellWRange.GetMenuItem("SAssembliesRangesSpellWColorMe").GetValue<Color>());
                    }
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange, SpellWRange.GetMenuItem("SAssembliesRangesSpellWColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
