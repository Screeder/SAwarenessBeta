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
    class Experience
    {
        public static Menu.MenuItemSettings ExperienceRange = new Menu.MenuItemSettings(typeof(Ranges.Experience));

        public Experience()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Experience()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Range.Ranges.GetActive() && ExperienceRange.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            ExperienceRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_EXPERIENCE_MAIN"), "SAwarenessRangesExperience"));
            ExperienceRange.MenuItems.Add(
                ExperienceRange.Menu.AddItem(new MenuItem("SAwarenessRangesExperienceMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            ExperienceRange.MenuItems.Add(
                ExperienceRange.Menu.AddItem(new MenuItem("SAwarenessRangesExperienceColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.LawnGreen)));
            ExperienceRange.MenuItems.Add(
                ExperienceRange.Menu.AddItem(new MenuItem("SAwarenessRangesExperienceColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.IndianRed)));
            ExperienceRange.MenuItems.Add(
                ExperienceRange.Menu.AddItem(new MenuItem("SAwarenessRangesExperienceActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return ExperienceRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            var mode = ExperienceRange.GetMenuItem("SAwarenessRangesExperienceMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    if (ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, 1400, ExperienceRange.GetMenuItem("SAwarenessRangesExperienceColorMe").GetValue<Color>());
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position, 1400, ExperienceRange.GetMenuItem("SAwarenessRangesExperienceColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    Utility.DrawCircle(ObjectManager.Player.Position, 1400, ExperienceRange.GetMenuItem("SAwarenessRangesExperienceColorMe").GetValue<Color>());
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead && enemy.Position.IsOnScreen())
                        {
                            Utility.DrawCircle(enemy.Position, 1400, ExperienceRange.GetMenuItem("SAwarenessRangesExperienceColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
