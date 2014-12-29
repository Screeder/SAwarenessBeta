using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    pingInfo.Add(new PingInfo(hero.ChampionName, new Vector2(ping.X, ping.Y), Game.Time + 2));
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
                    pingInfo.Remove(info);
                    continue;
                }
                Vector2 screenPos = Drawing.WorldToScreen(new Vector3(info.Pos, NavMesh.GetHeightForPosition(info.Pos.X, info.Pos.Y)));
                Drawing.DrawText(screenPos.X - 25, screenPos.Y, System.Drawing.Color.DeepSkyBlue, info.Name);
            }
        }

        private class PingInfo
        {
            public Vector2 Pos;
            public String Name;
            public float Time;

            public PingInfo(String name, Vector2 pos, float time)
            {
                Name = name;
                Pos = pos;
                Time = time;
            }
        }
    }
}
