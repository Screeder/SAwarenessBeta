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
    class Vision
    {
        public static Menu.MenuItemSettings VisionRange = new Menu.MenuItemSettings(typeof(Vision));

        List<String> _wards = new List<String>(new[]
        {
            "Feral Flare",
            "Vision Ward",
            "Stealth Ward",
            "Wriggle's Lantern",
            "Ruby Sightstone",
            "Sightstone",
            "Explorer's Ward",
            "Greater Stealth Totem",
            "Greater Stealth Totem",
            "Greater Vision Totem",
            "Bonetooth Necklace",
            "Head of Kha'Zix",
            "Quill Coat",
            "Spirit of the Ancient Golem"
        });

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
#if RANGES
            return Range.Ranges.GetActive() && VisionRange.GetActive();
#else
            return VisionRange.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            VisionRange.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("RANGES_VISION_MAIN"), "SAssembliesRangesVision"));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAssembliesRangesVisionMode", Language.GetString("RANGES_ALL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("RANGES_ALL_MODE_ME"), 
                    Language.GetString("RANGES_ALL_MODE_ENEMY"), 
                    Language.GetString("RANGES_ALL_MODE_BOTH")
                }))));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAssembliesRangesVisionDisplayMe", Language.GetString("RANGES_VISION_ME")).SetValue(false)));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAssembliesRangesVisionDisplayChampion", Language.GetString("RANGES_VISION_CHAMPION")).SetValue(false)));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAssembliesRangesVisionDisplayTurret", Language.GetString("RANGES_VISION_TURRET")).SetValue(false)));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAssembliesRangesVisionDisplayMinion", Language.GetString("RANGES_VISION_MINION")).SetValue(false)));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAssembliesRangesVisionDisplayWard", Language.GetString("RANGES_VISION_WARD")).SetValue(false)));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAssembliesRangesVisionColorMe", Language.GetString("RANGES_ALL_COLORME")).SetValue(Color.Indigo)));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAssembliesRangesVisionColorEnemy", Language.GetString("RANGES_ALL_COLORENEMY")).SetValue(Color.Indigo)));
            VisionRange.MenuItems.Add(
                VisionRange.Menu.AddItem(new MenuItem("SAssembliesRangesVisionActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return VisionRange;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            var mode = VisionRange.GetMenuItem("SAssembliesRangesVisionMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayMe").GetValue<bool>() && ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayChampion").GetValue<bool>())
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            if (!hero.IsEnemy && hero.IsVisible && hero.IsValid && !hero.IsDead && hero.Position.IsOnScreen() &&
                                ObjectManager.Player.ServerPosition.Distance(hero.ServerPosition) < 1800)
                            {
                                Utility.DrawCircle(hero.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                            }
                        }
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayTurret").GetValue<bool>())
                    {
                        foreach (var turret in ObjectManager.Get<Obj_AI_Turret>())
                        {
                            if (!turret.IsEnemy && turret.IsVisible && turret.IsValid && !turret.IsDead && turret.Position.IsOnScreen() &&
                                ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < 1800)
                            {
                                Utility.DrawCircle(turret.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                            }
                        }
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayMinion").GetValue<bool>())
                    {
                        foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (!minion.IsEnemy && minion.IsVisible && minion.IsValid && !minion.IsDead && minion.Position.IsOnScreen() &&
                                ObjectManager.Player.ServerPosition.Distance(minion.ServerPosition) < 1800 && minion.Team != GameObjectTeam.Neutral)
                            {
                                Utility.DrawCircle(minion.Position, 1100, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                            }
                        }
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayWard").GetValue<bool>())
                    {
                        foreach (var ward in ObjectManager.Get<GameObject>())
                        {
                            foreach (var wards in _wards)
                            {
                                if (ward.Name.Contains(wards) && !ward.IsEnemy && ward.IsVisible && ward.IsValid && !ward.IsDead && ward.Position.IsOnScreen() &&
                                    ObjectManager.Player.ServerPosition.Distance(ward.Position) < 1800)
                                {
                                    Utility.DrawCircle(ObjectManager.Player.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                                }
                            }
                        }
                    }
                    break;
                case 1:
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayChampion").GetValue<bool>())
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            if (hero.IsEnemy && hero.IsVisible && hero.IsValid && !hero.IsDead && hero.Position.IsOnScreen() &&
                                ObjectManager.Player.ServerPosition.Distance(hero.ServerPosition) < 1800)
                            {
                                Utility.DrawCircle(hero.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorEnemy").GetValue<Color>());
                            }
                        }
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayTurret").GetValue<bool>())
                    {
                        foreach (var turret in ObjectManager.Get<Obj_AI_Turret>())
                        {
                            if (turret.IsEnemy && turret.IsVisible && turret.IsValid && !turret.IsDead && turret.Position.IsOnScreen() &&
                                ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < 1800)
                            {
                                Utility.DrawCircle(turret.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorEnemy").GetValue<Color>());
                            }
                        }
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayMinion").GetValue<bool>())
                    {
                        foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (minion.IsEnemy && minion.IsVisible && minion.IsValid && !minion.IsDead && minion.Position.IsOnScreen() &&
                                ObjectManager.Player.ServerPosition.Distance(minion.ServerPosition) < 1800 && minion.Team != GameObjectTeam.Neutral)
                            {
                                Utility.DrawCircle(minion.Position, 1100, VisionRange.GetMenuItem("SAssembliesRangesVisionColorEnemy").GetValue<Color>());
                            }
                        }
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayWard").GetValue<bool>())
                    {
                        foreach (var ward in ObjectManager.Get<GameObject>())
                        {
                            foreach (var wards in _wards)
                            {
                                if (ward.Name.Contains(wards) && ward.IsEnemy && ward.IsVisible && ward.IsValid && !ward.IsDead && ward.Position.IsOnScreen() &&
                                    ObjectManager.Player.ServerPosition.Distance(ward.Position) < 1800)
                                {
                                    Utility.DrawCircle(ward.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorEnemy").GetValue<Color>());
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayMe").GetValue<bool>() && ObjectManager.Player.Position.IsOnScreen())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayChampion").GetValue<bool>())
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            if (hero.IsVisible && hero.IsValid && !hero.IsDead && hero.Position.IsOnScreen() &&
                                ObjectManager.Player.ServerPosition.Distance(hero.ServerPosition) < 1800)
                            {
                                if (!hero.IsEnemy)
                                {
                                    Utility.DrawCircle(hero.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                                }
                                else
                                {
                                    Utility.DrawCircle(hero.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorEnemy").GetValue<Color>());
                                }
                            }
                        }
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayTurret").GetValue<bool>())
                    {
                        foreach (var turret in ObjectManager.Get<Obj_AI_Turret>())
                        {
                            if (turret.IsVisible && turret.IsValid && !turret.IsDead && turret.Position.IsOnScreen() &&
                                ObjectManager.Player.ServerPosition.Distance(turret.ServerPosition) < 1800)
                            {
                                if (!turret.IsEnemy)
                                {
                                    Utility.DrawCircle(turret.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                                }
                                else
                                {
                                    Utility.DrawCircle(turret.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorEnemy").GetValue<Color>());
                                }
                            }
                        }
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayMinion").GetValue<bool>())
                    {
                        foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (minion.IsVisible && minion.IsValid && !minion.IsDead && minion.Position.IsOnScreen() &&
                                ObjectManager.Player.ServerPosition.Distance(minion.ServerPosition) < 1800 && minion.Team != GameObjectTeam.Neutral)
                            {
                                if (!minion.IsEnemy)
                                {
                                    Utility.DrawCircle(minion.Position, 1100, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                                }
                                else
                                {
                                    Utility.DrawCircle(minion.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorEnemy").GetValue<Color>());
                                }
                            }
                        }
                    }
                    if (VisionRange.GetMenuItem("SAssembliesRangesVisionDisplayWard").GetValue<bool>())
                    {
                        foreach (var ward in ObjectManager.Get<GameObject>())
                        {
                            foreach (var wards in _wards)
                            {
                                if (ward.Name.Contains(wards) && ward.IsVisible && ward.IsValid && !ward.IsDead && ward.Position.IsOnScreen() &&
                                    ObjectManager.Player.ServerPosition.Distance(ward.Position) < 1800)
                                {
                                    if (!ward.IsEnemy)
                                    {
                                        Utility.DrawCircle(ward.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorMe").GetValue<Color>());
                                    }
                                    else
                                    {
                                        Utility.DrawCircle(ward.Position, 1200, VisionRange.GetMenuItem("SAssembliesRangesVisionColorEnemy").GetValue<Color>());
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}
