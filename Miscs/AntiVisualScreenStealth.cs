using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Miscs
{
    class AntiVisualScreenStealth
    {
        public static Menu.MenuItemSettings AntiVisualScreenStealthMisc = new Menu.MenuItemSettings(typeof(AntiVisualScreenStealth));

        private static int Header = 0xDB;

        public AntiVisualScreenStealth()
        {
            bool available = false;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                switch (hero.ChampionName)
                {
                    case "Akali":
                        available = true;
                        break;

                    case "Khazix":
                        available = true;
                        break;

                    case "Leblanc":
                        available = true;
                        break;

                    case "MonkeyKing":
                        available = true;
                        break;

                    case "Nocturne":
                        available = true;
                        break;

                    case "Shaco":
                        available = true;
                        break;

                    case "Talon":
                        available = true;
                        break;

                    case "Teemo":
                        available = true;
                        break;

                    case "Twitch":
                        available = true;
                        break;

                    case "Vayne":
                        available = true;
                        break;
                }
                if (available)
                    break;
            }
            if (available)
                Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        ~AntiVisualScreenStealth()
        {
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && AntiVisualScreenStealthMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AntiVisualScreenStealthMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_ANTIVISUALSCREENSTEALTH_MAIN"), "SAwarenessMiscsAntiVisualScreenStealth"));
            AntiVisualScreenStealthMisc.MenuItems.Add(
                AntiVisualScreenStealthMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsAntiVisualScreenStealthActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AntiVisualScreenStealthMisc;
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;

            var reader = new BinaryReader(new MemoryStream(args.PacketData));

            byte packetId = reader.ReadByte();
            if (packetId == Header)
            {
                reader.ReadInt32();
                byte visualStealthActive = reader.ReadByte();
                if (visualStealthActive == 1)
                    args.Process = false;
            }
        }
    }
}
