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
    class SpellR
    {
        public static Menu.MenuItemSettings SpellRRange = new Menu.MenuItemSettings(typeof(Ranges.SpellR));

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
            return Range.Ranges.GetActive() && SpellRRange.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SpellRRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_SPELLR_MAIN"), "SAwarenessRangesSpellR"));
            SpellRRange.MenuItems.Add(
                SpellRRange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellRMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            SpellRRange.MenuItems.Add(
                SpellRRange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellRColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            SpellRRange.MenuItems.Add(
                SpellRRange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellRColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.IndianRed)));
            SpellRRange.MenuItems.Add(
                SpellRRange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellRActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SpellRRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            var mode = SpellRRange.GetMenuItem("SAwarenessRangesSpellRMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange, SpellRRange.GetMenuItem("SAwarenessRangesSpellRColorMe").GetValue<Color>());
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange, SpellRRange.GetMenuItem("SAwarenessRangesSpellRColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange, SpellRRange.GetMenuItem("SAwarenessRangesSpellRColorMe").GetValue<Color>());
                    }
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange, SpellRRange.GetMenuItem("SAwarenessRangesSpellRColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
