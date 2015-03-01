using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Ranges
{
    class SpellE
    {
        public static Menu.MenuItemSettings SpellERange = new Menu.MenuItemSettings(typeof(Ranges.SpellE));

        public SpellE()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~SpellE()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Range.Ranges.GetActive() && SpellERange.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SpellERange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_SPELLE_MAIN"), "SAwarenessRangesSpellE"));
            SpellERange.MenuItems.Add(
                SpellERange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellEMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            SpellERange.MenuItems.Add(
                SpellERange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellEColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            SpellERange.MenuItems.Add(
                SpellERange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellEColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.IndianRed)));
            SpellERange.MenuItems.Add(
                SpellERange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellEActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SpellERange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            var mode = SpellERange.GetMenuItem("SAwarenessRangesSpellEMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange, SpellERange.GetMenuItem("SAwarenessRangesSpellEColorMe").GetValue<Color>());
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange, SpellERange.GetMenuItem("SAwarenessRangesSpellEColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange, SpellERange.GetMenuItem("SAwarenessRangesSpellEColorMe").GetValue<Color>());
                    }
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange, SpellERange.GetMenuItem("SAwarenessRangesSpellEColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
