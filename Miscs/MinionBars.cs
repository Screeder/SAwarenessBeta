using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Miscs
{
    class MinionBars
    {
        public static Menu.MenuItemSettings MinionBarsMisc = new Menu.MenuItemSettings(typeof(MinionBars));

        public MinionBars()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~MinionBars()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && MinionBarsMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            MinionBarsMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_MINIONBARS_MAIN"), "SAwarenessMiscsMinionBars"));
            MinionBarsMisc.MenuItems.Add(
                MinionBarsMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsMinionBarsGlowActive", Language.GetString("MISCS_MINIONBARS_GLOW")).SetValue(false)));
            MinionBarsMisc.MenuItems.Add(
                MinionBarsMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsMinionBarsActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return MinionBarsMisc;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (!minion.IsVisible || minion.IsDead || minion.IsAlly)
                    continue;
                Vector2 hpPos = minion.HPBarPosition;
                //hpPos.Y -= 3;
                double damageMinion = ObjectManager.Player.GetAutoAttackDamage(minion);
                double hitsToKill = Math.Ceiling(minion.MaxHealth / damageMinion);
                double barsToDraw = Math.Floor(minion.MaxHealth / 100.0);
                double barDistance = 100.0 / (minion.MaxHealth / 62.0);
                double myDamageDistance = damageMinion / (minion.MaxHealth / 62.0);
                double barsDrawn = 0;
                int heightOffset = 1;
                int barSize = 2;
                int barWidth = 1;
                //hpPos.X = hpPos.X - 32;
                hpPos.Y = hpPos.Y + heightOffset;
                if (minion.BaseSkinName == "Dragon" || minion.BaseSkinName == "Worm" ||
                    minion.BaseSkinName == "TT_Spiderboss")
                {
                    double healthDraw = 500.0;
                    if (minion.BaseSkinName == "Dragon")
                    {
                        hpPos.X -= 31;
                        hpPos.Y -= 7;
                    }
                    else if (minion.BaseSkinName == "Worm")
                    {
                        hpPos.X -= 31;
                        healthDraw = 1000.0;
                    }
                    else if (minion.BaseSkinName == "TT_Spiderboss")
                        hpPos.X -= 3;
                    barsToDraw = Math.Floor(minion.MaxHealth / healthDraw);
                    barDistance = healthDraw / (minion.MaxHealth / 124.0);
                    double drawDistance = 0;
                    while (barsDrawn != barsToDraw && barsToDraw != 0 && barsToDraw < 200)
                    {
                        drawDistance = drawDistance + barDistance;
                        if (barsDrawn % 2 == 1)
                        {
                            DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth + 1, barSize,
                                System.Drawing.Color.Black);
                        }
                        else
                            DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth, barSize,
                                System.Drawing.Color.Black);
                        barsDrawn = barsDrawn + 1;
                    }
                    DrawRectangleAL(hpPos.X + 43 + myDamageDistance, hpPos.Y + 19, barWidth, barSize, System.Drawing.Color.GreenYellow);
                    if (damageMinion > minion.Health)
                    {
                        OutLineBar(hpPos.X + 43, hpPos.Y + 20, System.Drawing.Color.GreenYellow);
                    }
                }
                else
                {
                    double drawDistance = 0;
                    while (barsDrawn != barsToDraw && barsToDraw != 0 && barsToDraw < 50)
                    {
                        drawDistance = drawDistance + barDistance;
                        if (barsToDraw > 20)
                        {
                            if (barsDrawn % 5 == 4)
                                if (barsDrawn % 10 == 9)
                                    DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth + 1, barSize,
                                        System.Drawing.Color.Black);
                                else
                                    DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth, barSize,
                                        System.Drawing.Color.Black);
                        }
                        else
                            DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth, barSize,
                                System.Drawing.Color.Black);
                        barsDrawn = barsDrawn + 1;

                    }
                    DrawRectangleAL(hpPos.X + 43 + myDamageDistance, hpPos.Y + 19, barWidth, barSize, System.Drawing.Color.GreenYellow);
                    if (damageMinion > minion.Health && MinionBarsMisc.GetMenuItem("SAwarenessMiscsMinionBarsGlowActive").GetValue<bool>())
                    {
                        OutLineBar(hpPos.X + 43, hpPos.Y + 20, System.Drawing.Color.GreenYellow);
                    }
                }
            }
        }

        private void DrawRectangleAL(double x, double y, double w, double h, System.Drawing.Color color)
        {
            Vector2[] points = new Vector2[4];
            points[0] = new Vector2((float)Math.Floor(x), (float)Math.Floor(y));
            points[1] = new Vector2((float)Math.Floor(x + w), (float)Math.Floor(y));
            points[2] = new Vector2((float)Math.Floor(x), (float)Math.Floor(y + h));
            points[3] = new Vector2((float)Math.Floor(x + w), (float)Math.Floor(y + h));
            if (Common.IsOnScreen(points[0]) && Common.IsOnScreen(points[1]))
                Drawing.DrawLine(points[0], points[1], 1, color);
            if (Common.IsOnScreen(points[0]) && Common.IsOnScreen(points[2]))
                Drawing.DrawLine(points[0], points[2], 1, color);
            if (Common.IsOnScreen(points[1]) && Common.IsOnScreen(points[3]))
                Drawing.DrawLine(points[1], points[3], 1, color);
            if (Common.IsOnScreen(points[2]) && Common.IsOnScreen(points[3]))
                Drawing.DrawLine(points[2], points[3], 1, color);
        }

        private void OutLineBar(double x, double y, System.Drawing.Color color)
        {
            DrawRectangleAL(x, y - 3, 64, 1, color);
            DrawRectangleAL(x, y + 2, 64, 1, color);

            DrawRectangleAL(x, y, 1, 5, color);
            DrawRectangleAL(x + 63, y, 1, 5, color);
        }
    }
}
