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
    class Vision
    {
        public static Menu.MenuItemSettings VisionRange = new Menu.MenuItemSettings(typeof(Ranges.Vision));

        public Vision()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Vision()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Range.Ranges.GetActive() && VisionRange.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            VisionRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_VISION_MAIN"), "SAwarenessRangesVision"));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAwarenessRangesVisionMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAwarenessRangesVisionColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.Indigo)));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAwarenessRangesVisionColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.Indigo)));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAwarenessRangesVisionActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return VisionRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            var mode = VisionRange.GetMenuItem("SAwarenessRangesVisionMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position, 1200, VisionRange.GetMenuItem("SAwarenessRangesVisionColorMe").GetValue<Color>());
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead &&
                            ObjectManager.Player.ServerPosition.Distance(enemy.ServerPosition) < 1800)
                        {
                            Utility.DrawCircle(enemy.Position, 1200, VisionRange.GetMenuItem("SAwarenessRangesVisionColorEnemy").GetValue<Color>());
                        }
                    }
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && turret.IsEnemy && turret.IsValid &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < 1800)
                        {
                            Utility.DrawCircle(turret.Position, 1200, VisionRange.GetMenuItem("SAwarenessRangesVisionColorEnemy").GetValue<Color>());
                        }
                    }
                    foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>())
                    {
                        if (minion.IsEnemy && minion.Team != GameObjectTeam.Neutral && minion.IsVisible && minion.IsValid && !minion.IsDead &&
                            ObjectManager.Player.ServerPosition.Distance(minion.ServerPosition) < 1800)
                        {
                            Utility.DrawCircle(minion.Position, 1200, VisionRange.GetMenuItem("SAwarenessRangesVisionColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
                case 2:
                    Utility.DrawCircle(ObjectManager.Player.Position, 1200, VisionRange.GetMenuItem("SAwarenessRangesVisionColorMe").GetValue<Color>());
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead &&
                            ObjectManager.Player.ServerPosition.Distance(enemy.ServerPosition) < 1800)
                        {
                            Utility.DrawCircle(enemy.Position, 1200, VisionRange.GetMenuItem("SAwarenessRangesVisionColorEnemy").GetValue<Color>());
                        }
                    }
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && turret.IsEnemy && turret.IsValid &&
                            ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < 1800)
                        {
                            Utility.DrawCircle(turret.Position, 1200, VisionRange.GetMenuItem("SAwarenessRangesVisionColorEnemy").GetValue<Color>());
                        }
                    }
                    foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>())
                    {
                        if (minion.IsEnemy && minion.Team != GameObjectTeam.Neutral && minion.IsVisible && minion.IsValid && !minion.IsDead &&
                            ObjectManager.Player.ServerPosition.Distance(minion.ServerPosition) < 1800)
                        {
                            Utility.DrawCircle(minion.Position, 1200, VisionRange.GetMenuItem("SAwarenessRangesVisionColorEnemy").GetValue<Color>());
                        }
                    }
                    break;
            }
        }
    }
}
