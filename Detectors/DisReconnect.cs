using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Detectors
{
    class DisReconnect
    {
        public static Menu.MenuItemSettings DisReconnectDetector = new Menu.MenuItemSettings(typeof(DisReconnect));

        private Dictionary<Obj_AI_Hero, bool> _disconnects = new Dictionary<Obj_AI_Hero, bool>();
        private Dictionary<Obj_AI_Hero, bool> _reconnects = new Dictionary<Obj_AI_Hero, bool>();

        public DisReconnect()
        {
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        ~DisReconnect()
        {
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
            _disconnects = null;
            _reconnects = null;
        }

        public bool IsActive()
        {
            return Detector.Detectors.GetActive() && DisReconnectDetector.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            DisReconnectDetector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("DETECTORS_DISRECONNECT_MAIN"), "SAwarenessDetectorsDisReconnect"));
            DisReconnectDetector.MenuItems.Add(
                DisReconnectDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsDisReconnectChatChoice", Language.GetString("GLOBAL_CHAT_CHOICE")).SetValue(new StringList(new[]
                        {
                            Language.GetString("GLOBAL_CHAT_CHOICE_NONE"), 
                            Language.GetString("GLOBAL_CHAT_CHOICE_LOCAL"), 
                            Language.GetString("GLOBAL_CHAT_CHOICE_SERVER")
                        }))));
            DisReconnectDetector.MenuItems.Add(
                DisReconnectDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsDisReconnectSpeech", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            DisReconnectDetector.MenuItems.Add(
                DisReconnectDetector.Menu.AddItem(new MenuItem("SAwarenessDetectorsDisReconnectActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return DisReconnectDetector;
        }

        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte(); //PacketId
                if (packetId != Packet.S2C.PlayerDisconnect.Header)
                    return;
                Packet.S2C.PlayerDisconnect.Struct disconnect = Packet.S2C.PlayerDisconnect.Decoded(args.PacketData);
                if (disconnect.Player == null)
                    return;
                if (_disconnects.ContainsKey(disconnect.Player))
                {
                    _disconnects[disconnect.Player] = true;
                }
                else
                {
                    _disconnects.Add(disconnect.Player, true);
                }
                if (
                    DisReconnectDetector.GetMenuItem("SAwarenessDetectorsDisReconnectChatChoice")
                        .GetValue<StringList>()
                        .SelectedIndex == 1)
                {
                    Game.PrintChat("Champion " + disconnect.Player.ChampionName + " has disconnected!");
                }
                else if (
                    DisReconnectDetector.GetMenuItem("SAwarenessDetectorsDisReconnectChatChoice")
                        .GetValue<StringList>()
                        .SelectedIndex == 2 &&
                    Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say("Champion " + disconnect.Player.ChampionName + " has disconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAwarenessDetectorsDisReconnectSpeech").GetValue<bool>())
                {
                    Speech.Speak("Champion " + disconnect.Player.ChampionName + " has disconnected!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DisconnectProcess: " + ex);
            }
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte(); //PacketId
                if (packetId != Packet.S2C.PlayerReconnected.Header)
                    return;
                Packet.S2C.PlayerReconnected.Struct reconnect = Packet.S2C.PlayerReconnected.Decoded(args.PacketData);
                if (reconnect.Player == null)
                    return;
                if (_reconnects.ContainsKey(reconnect.Player))
                {
                    _reconnects[reconnect.Player] = true;
                }
                else
                {
                    _reconnects.Add(reconnect.Player, true);
                }
                if (
                    DisReconnectDetector.GetMenuItem("SAwarenessDetectorsDisReconnectChatChoice")
                        .GetValue<StringList>()
                        .SelectedIndex == 1)
                {
                    Game.PrintChat("Champion " + reconnect.Player.ChampionName + " has reconnected!");
                }
                else if (
                    DisReconnectDetector.GetMenuItem("SAwarenessDetectorsDisReconnectChatChoice")
                        .GetValue<StringList>()
                        .SelectedIndex == 2 &&
                    Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say("Champion " + reconnect.Player.ChampionName + " has reconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAwarenessDetectorsDisReconnectSpeech").GetValue<bool>())
                {
                    Speech.Speak("Champion " + reconnect.Player.ChampionName + " has reconnected!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReconnectProcess: " + ex);
            }
        }
    }
}
