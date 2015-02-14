using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SAwareness.Trackers
{
    class Waypoint
    {
        public static Menu.MenuItemSettings WaypointTracker = new Menu.MenuItemSettings(typeof(Waypoint));

        public Waypoint()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Waypoint()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Tracker.Trackers.GetActive() && WaypointTracker.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            WaypointTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_WAYPOINT_MAIN"), "SAwarenessTrackersWaypoint"));
            WaypointTracker.MenuItems.Add(
                WaypointTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersWaypointActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return WaypointTracker;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                float arrivalTime = 0.0f;
                if (enemy.IsValid && enemy.IsVisible && !enemy.IsDead && enemy.IsEnemy)
                {
                    List<Vector2> waypoints = enemy.GetWaypoints();
                    for (int i = 0; i < waypoints.Count - 1; i++)
                    {
                        Vector2 oWp;
                        Vector2 nWp;
                        float time = ((Vector3.Distance(waypoints[i].To3D(), waypoints[i + 1].To3D()) /
                              (ObjectManager.Player.MoveSpeed / 1000)) / 1000);
                        time = (float)Math.Round(time, 2);
                        arrivalTime += time;
                        oWp = Drawing.WorldToScreen(waypoints[i].To3D());
                        nWp = Drawing.WorldToScreen(waypoints[i + 1].To3D());
                        if (!waypoints[i].IsOnScreen() && !waypoints[i + 1].IsOnScreen())
                        {
                            continue;
                        }
                        Drawing.DrawLine(oWp[0], oWp[1], nWp[0], nWp[1], 1, System.Drawing.Color.White);
                        if (i == enemy.Path.Length - 1)
                        {
                            DrawCross(nWp[0], nWp[1], 1.0f, 3.0f, System.Drawing.Color.Red);
                            Drawing.DrawText(nWp[0] - 15, nWp[1] + 10, System.Drawing.Color.Red, arrivalTime.ToString());
                        }
                    }
                }
            }
        }

        private void DrawCross(float x, float y, float size, float thickness, Color color)
        {
            var topLeft = new Vector2(x - 10*size, y - 10*size);
            var topRight = new Vector2(x + 10*size, y - 10*size);
            var botLeft = new Vector2(x - 10*size, y + 10*size);
            var botRight = new Vector2(x + 10*size, y + 10*size);

            Drawing.DrawLine(topLeft.X, topLeft.Y, botRight.X, botRight.Y, thickness, color);
            Drawing.DrawLine(topRight.X, topRight.Y, botLeft.X, botLeft.Y, thickness, color);
        }
    }
}
