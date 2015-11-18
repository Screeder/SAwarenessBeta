using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Detectors
{
    class DisReconnect
    {
        public static Menu.MenuItemSettings DisReconnectDetector = new Menu.MenuItemSettings(typeof(DisReconnect));

        public DisReconnect()
        {
            Game.OnProcessPacket += Game_OnGameProcessPacket;
        }

        ~DisReconnect()
        {
            Game.OnProcessPacket -= Game_OnGameProcessPacket;
        }

        public bool IsActive()
        {
#if DETECTORS
            return Detector.Detectors.GetActive() && DisReconnectDetector.GetActive();
#else
            return DisReconnectDetector.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            DisReconnectDetector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("DETECTORS_DISRECONNECT_MAIN"), "SAssembliesDetectorsDisReconnect"));
            DisReconnectDetector.MenuItems.Add(
                DisReconnectDetector.Menu.AddItem(new MenuItem("SAssembliesDetectorsDisReconnectChat", Language.GetString("GLOBAL_CHAT")).SetValue(false)));
            DisReconnectDetector.MenuItems.Add(
                DisReconnectDetector.Menu.AddItem(new MenuItem("SAssembliesDetectorsDisReconnectNotification", Language.GetString("GLOBAL_NOTIFICATION")).SetValue(false)));
            DisReconnectDetector.MenuItems.Add(
                DisReconnectDetector.Menu.AddItem(new MenuItem("SAssembliesDetectorsDisReconnectSpeech", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            DisReconnectDetector.MenuItems.Add(DisReconnectDetector.CreateActiveMenuItem("SAssembliesDetectorsDisReconnectActive", () => new DisReconnect()));
            return DisReconnectDetector;
        }

        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;
            DetectDisconnect(args);
            DetectReconnect(args);
        }

        private void DetectDisconnect(GamePacketEventArgs args)
        {
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte(); //PacketId
                if (packetId != 16 || args.PacketData.Length != 12)
                    return;
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectChat").GetValue<bool>() &&
                        Menu.GlobalSettings.GetMenuItem("SAssembliesGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say("A Champion has disconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectSpeech").GetValue<bool>())
                {
                    Speech.Speak("A Champion has disconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectNotification").GetValue<bool>())
                {
                    Common.ShowNotification("A Champion has disconnected!", Color.LawnGreen, 3);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DisconnectProcess: " + ex);
            }
        }

        private void DetectReconnect(GamePacketEventArgs args)
        {
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte(); //PacketId
                if (packetId != 145)
                    return;
                if (
                    DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectChat").GetValue<bool>() &&
                    Menu.GlobalSettings.GetMenuItem("SAssembliesGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say("A Champion has reconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectSpeech").GetValue<bool>())
                {
                    Speech.Speak("A Champion has reconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectNotification").GetValue<bool>())
                {
                    Common.ShowNotification("A Champion has reconnected!", Color.Yellow, 3);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReconnectProcess: " + ex);
            }
        }
    }
}