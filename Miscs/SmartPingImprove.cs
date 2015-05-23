using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = SharpDX.Color;

namespace SAssemblies.Miscs
{
    internal class SmartPingImprove //https://www.youtube.com/watch?v=HBvZZWSrmng
    {
        public static Menu.MenuItemSettings SmartPingImproveMisc = new Menu.MenuItemSettings(typeof(SmartPingImprove));

        List<PingInfo> pingInfo = new List<PingInfo>(); 

        public SmartPingImprove()
        {
            Game.OnPing += Game_OnPing;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~SmartPingImprove()
        {
            Game.OnPing -= Game_OnPing;
            Drawing.OnDraw -= Drawing_OnDraw;
            pingInfo = null;
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && SmartPingImproveMisc.GetActive();
#else
            return SmartPingImproveMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SmartPingImproveMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_SMARTPINGIMPROVE_MAIN"), "SAssembliesMiscsSmartPingImprove"));
            SmartPingImproveMisc.MenuItems.Add(
                SmartPingImproveMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsSmartPingImproveActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SmartPingImproveMisc;
        }

        void Game_OnPing(GamePingEventArgs args)
        {
            if (!IsActive())
                return;

            Obj_AI_Hero hero = args.Source as Obj_AI_Hero;
            if (hero != null && hero.IsValid)
            {
                PingInfo pingInfoN = new PingInfo(hero.NetworkId, args.Position, Game.Time + 2, args.PingType);
                pingInfo.Add(pingInfoN);
                switch (args.PingType)
                {
                    case PingCategory.AssistMe:
                        CreateSprites(pingInfoN);
                        break;

                    case PingCategory.Danger:
                        CreateSprites(pingInfoN);
                        break;
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            foreach (var info in pingInfo.ToList())
            {
                if (info.Time < Game.Time)
                {
                    //DeleteSprites(info);
                    pingInfo.Remove(info);
                    continue;
                }
                Obj_AI_Hero hero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(info.NetworkId);
                Vector2 screenPos = Drawing.WorldToScreen(new Vector3(info.Pos, NavMesh.GetHeightForPosition(info.Pos.X, info.Pos.Y)));
                Drawing.DrawText(screenPos.X - 25, screenPos.Y, System.Drawing.Color.DeepSkyBlue, hero.ChampionName);
                switch (info.Type)
                {
                    case PingCategory.OnMyWay:
                        if (!hero.Position.IsOnScreen())
                        {
                            DrawWaypoint(hero, info.Pos.To3D2());
                        }
                        break;
                }
            }
        }

        private void DrawWaypoint(Obj_AI_Hero hero, Vector3 endPos)
        {
            List<Vector3> waypoints = hero.GetPath(endPos).ToList();
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Vector2 oWp = Drawing.WorldToScreen(waypoints[i]);
                Vector2 nWp = Drawing.WorldToScreen(waypoints[i + 1]);
                Drawing.DrawLine(oWp[0], oWp[1], nWp[0], nWp[1], 1, System.Drawing.Color.GreenYellow);
            }
        }

        private Vector2 GetScreenPosition(Vector2 wtsPos, Size size)
        {
            int apparentX = (int)Math.Max(1, Math.Min(wtsPos.X, Drawing.Width - size.Width));
            int apparentY = (int)Math.Max(1, Math.Min(wtsPos.Y, Drawing.Height - size.Height));
            return new Vector2(apparentX, apparentY);
        }

        private void DeleteSprites(PingInfo info)
        {
            if (info.Direction != null)
            {
                info.Direction.Dispose();
            }
            if (info.Icon != null)
            {
                info.Icon.Dispose();
            }
            if (info.IconBackground != null)
            {
                info.IconBackground.Dispose();
            }
        }

        private void CreateSprites(PingInfo info)
        {
            String iconName = null;
            String iconBackgroundName = null;
            String directionName = null;
            Color directionColor = Color.White;

            switch (info.Type)
            {
                case PingCategory.AssistMe:
                    iconName = "pingcomehere";
                    iconBackgroundName = "pingmarker";
                    directionName = "directionindicator";
                    directionColor = Color.DeepSkyBlue;
                    break;

                case PingCategory.Danger:
                    iconName = "pinggetback";
                    iconBackgroundName = "pingmarker_red";
                    directionName = "directionindicator";
                    directionColor = Color.Red;
                    break;
            }

            if(iconName == null)
                return;

            SpriteHelper.LoadTexture(iconName, ref info.Icon, SpriteHelper.TextureType.Default);
            info.Icon.Sprite.Scale = new Vector2(0.4f);
            info.Icon.Sprite.PositionUpdate = delegate
            {
                return GetScreenPosition(Drawing.WorldToScreen(info.Pos.To3D2()), new Size(info.Icon.Sprite.Width, info.Icon.Sprite.Height));
            };
            info.Icon.Sprite.VisibleCondition = delegate
            {
                return Misc.Miscs.GetActive() && SmartPingImproveMisc.GetActive();
            };
            info.Icon.Sprite.Add(1);

            SpriteHelper.LoadTexture(iconBackgroundName, ref info.IconBackground, SpriteHelper.TextureType.Default);
            info.IconBackground.Sprite.Scale = new Vector2(1.5f);
            info.IconBackground.Sprite.PositionUpdate = delegate
            {
                return GetScreenPosition(Drawing.WorldToScreen(info.Pos.To3D2()), new Size(info.IconBackground.Sprite.Width, info.IconBackground.Sprite.Height));
            };
            info.IconBackground.Sprite.VisibleCondition = delegate
            {
                return Misc.Miscs.GetActive() && SmartPingImproveMisc.GetActive();
            };
            info.IconBackground.Sprite.Add(0);

            SpriteHelper.LoadTexture(directionName, ref info.Direction, SpriteHelper.TextureType.Default);
            info.Direction.Sprite.Scale = new Vector2(0.6f);
            info.Direction.Sprite.PositionUpdate = delegate
            {
                Vector2 normPos = Drawing.WorldToScreen(info.Pos.To3D2());
                Vector2 screenPos = GetScreenPosition(normPos, new Size(info.Direction.Sprite.Width, info.Direction.Sprite.Height));
                float angle = AngleBetween(screenPos, normPos);//screenPos.AngleBetween(normPos);
                angle = Geometry.DegreeToRadian(angle);
                info.Direction.Sprite.Rotation = angle; //Check if it is degree
                //screenPos = screenPos.Extend(normPos, 100);
                //screenPos = screenPos.Rotated(angle); //Check if needed
                return screenPos;
            };
            info.Direction.Sprite.VisibleCondition = delegate
            {
                return Misc.Miscs.GetActive() && SmartPingImproveMisc.GetActive();
            };
            info.Direction.Sprite.Color = directionColor;
            info.Direction.Sprite.Add(2);
        }

        /// <summary>
        /// AngleBetween - the angle between 2 vectors
        /// </summary>
        /// <returns>
        /// Returns the the angle in degrees between vector1 and vector2
        /// </returns>
        /// <param name="vector1"> The first Vector </param>
        /// <param name="vector2"> The second Vector </param>
        public static float AngleBetween(Vector2 vector1, Vector2 vector2)
        {
            double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
            double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

            return (float)(Math.Atan2(sin, cos) * (180 / Math.PI));
        }

        private class PingInfo
        {
            public Vector2 Pos;
            public int NetworkId;
            public float Time;
            public PingCategory Type;
            public SpriteHelper.SpriteInfo Icon;
            public SpriteHelper.SpriteInfo IconBackground;
            public SpriteHelper.SpriteInfo Direction;

            public PingInfo(int networkId, Vector2 pos, float time, PingCategory type)
            {
                NetworkId = networkId;
                Pos = pos;
                Time = time;
                Type = type;
            }
        }
    }
}
