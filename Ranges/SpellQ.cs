using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAwareness.Properties;

namespace SAwareness.Ranges
{
    class SpellQ
    {
        public static Menu.MenuItemSettings SpellQRange = new Menu.MenuItemSettings(typeof(SpellQ));

        public static void Main()
        {
            
        }

        public SpellQ()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~SpellQ()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Range.Ranges.GetActive() && SpellQRange.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SpellQRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_SPELLQ_MAIN"), "SAwarenessRangesSpellQ"));
            SpellQRange.MenuItems.Add(
                SpellQRange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellQMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            SpellQRange.MenuItems.Add(
                SpellQRange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellQColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            SpellQRange.MenuItems.Add(
                SpellQRange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellQColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.IndianRed)));
            SpellQRange.MenuItems.Add(
                SpellQRange.Menu.AddItem(new MenuItem("SAwarenessRangesSpellQActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SpellQRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            var mode = SpellQRange.GetMenuItem("SAwarenessRangesSpellQMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange, SpellQRange.GetMenuItem("SAwarenessRangesSpellQColorMe").GetValue<Color>());
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange, SpellQRange.GetMenuItem("SAwarenessRangesSpellQColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange, SpellQRange.GetMenuItem("SAwarenessRangesSpellQColorMe").GetValue<Color>());
                    }
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange, SpellQRange.GetMenuItem("SAwarenessRangesSpellQColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
