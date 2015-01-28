using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Miscs
{
    internal class SmartPingImprove
    {
        public static Menu.MenuItemSettings SmartPingImproveMisc = new Menu.MenuItemSettings(typeof(SmartPingImprove));

        List<PingInfo> pingInfo = new List<PingInfo>(); 

        public SmartPingImprove()
        {
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        ~SmartPingImprove()
        {
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
            pingInfo = null;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && SmartPingImproveMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SmartPingImproveMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_SMARTPINGIMPROVE_MAIN"), "SAwarenessMiscsSmartPingImprove"));
            SmartPingImproveMisc.MenuItems.Add(
                SmartPingImproveMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsSmartPingImproveActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SmartPingImproveMisc;
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;

            var reader = new BinaryReader(new MemoryStream(args.PacketData));

            byte packetId = reader.ReadByte();
            if (packetId == Packet.S2C.Ping.Header)
            {
                Packet.S2C.Ping.Struct ping = Packet.S2C.Ping.Decoded(args.PacketData);
                Obj_AI_Hero hero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(ping.SourceNetworkId);
                if (hero != null && hero.IsValid)
                {
                    pingInfo.Add(new PingInfo(hero.NetworkId, new Vector2(ping.X, ping.Y), Game.Time + 2, ping.Type));
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
                    DeleteSprites(info);
                    pingInfo.Remove(info);
                    continue;
                }
                Obj_AI_Hero hero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(info.NetworkId);
                Vector2 screenPos = Drawing.WorldToScreen(new Vector3(info.Pos, NavMesh.GetHeightForPosition(info.Pos.X, info.Pos.Y)));
                Drawing.DrawText(screenPos.X - 25, screenPos.Y, System.Drawing.Color.DeepSkyBlue, hero.ChampionName);
                switch (info.Type)
                {
                    case Packet.PingType.AssistMe://TODO: ADD https://www.youtube.com/watch?v=HBvZZWSrmng
                        CreateSprites(info);
                        break;

                    case Packet.PingType.Danger: //TODO: ADD https://www.youtube.com/watch?v=HBvZZWSrmng
                        CreateSprites(info);
                        break;

                    case Packet.PingType.OnMyWay:
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

        private Vector2 GetScreenPosition(Vector2 wtsPos)
        {
            int apparentX = (int)Math.Max(0, Math.Min(wtsPos.X, Drawing.Width));
            int apparentY = (int)Math.Max(0, Math.Min(wtsPos.Y, Drawing.Height));
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
                case Packet.PingType.AssistMe:
                    iconName = "?????????????????";
                    iconBackgroundName = "?????????????????";
                    directionName = "?????????????????";
                    directionColor = Color.DeepSkyBlue;
                    break;

                case Packet.PingType.Danger:
                    iconName = "?????????????????";
                    iconBackgroundName = "?????????????????";
                    directionName = "?????????????????";
                    directionColor = Color.Red;
                    break;
            }

            if(iconName == null)
                return;

            SpriteHelper.LoadTexture(iconName, ref info.Icon, SpriteHelper.TextureType.Default);
            info.Icon.Sprite.PositionUpdate = delegate
            {
                return GetScreenPosition(Drawing.WorldToScreen(info.Pos.To3D2()));
            };
            info.Icon.Sprite.VisibleCondition = delegate
            {
                return Misc.Miscs.GetActive() && SmartPingImproveMisc.GetActive();
            };
            info.Icon.Sprite.Add(1);

            SpriteHelper.LoadTexture(iconBackgroundName, ref info.IconBackground, SpriteHelper.TextureType.Default);
            info.IconBackground.Sprite.PositionUpdate = delegate
            {
                return GetScreenPosition(Drawing.WorldToScreen(info.Pos.To3D2()));
            };
            info.IconBackground.Sprite.VisibleCondition = delegate
            {
                return Misc.Miscs.GetActive() && SmartPingImproveMisc.GetActive();
            };
            info.IconBackground.Sprite.Add(0);

            SpriteHelper.LoadTexture(directionName, ref info.Direction, SpriteHelper.TextureType.Default);
            info.Direction.Sprite.PositionUpdate = delegate
            {
                Vector2 normPos = Drawing.WorldToScreen(info.Pos.To3D2());
                Vector2 screenPos = GetScreenPosition(normPos);
                float angle = screenPos.AngleBetween(normPos);
                info.Direction.Sprite.Rotation = angle; //Check if it is degree
                angle = Geometry.DegreeToRadian(angle);
                screenPos = screenPos.Rotated(angle); //Check if needed
                screenPos = screenPos.Extend(normPos, 100);
                return screenPos;
            };
            info.Direction.Sprite.VisibleCondition = delegate
            {
                return Misc.Miscs.GetActive() && SmartPingImproveMisc.GetActive();
            };
            info.Direction.Sprite.Color = directionColor;
            info.Direction.Sprite.Add(2);
        }

        private class PingInfo
        {
            public Vector2 Pos;
            public int NetworkId;
            public float Time;
            public Packet.PingType Type;
            public SpriteHelper.SpriteInfo Icon;
            public SpriteHelper.SpriteInfo IconBackground;
            public SpriteHelper.SpriteInfo Direction;

            public PingInfo(int networkId, Vector2 pos, float time, Packet.PingType type)
            {
                NetworkId = networkId;
                Pos = pos;
                Time = time;
                Type = type;
            }
        }
    }
}
