﻿using System;
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

        private Dictionary<Obj_AI_Hero, bool> _disconnects = new Dictionary<Obj_AI_Hero, bool>();
        private Dictionary<Obj_AI_Hero, bool> _reconnects = new Dictionary<Obj_AI_Hero, bool>();

        public DisReconnect()
        {
            Game.OnProcessPacket += Game_OnGameProcessPacket;
        }

        ~DisReconnect()
        {
            Game.OnProcessPacket -= Game_OnGameProcessPacket;
            _disconnects = null;
            _reconnects = null;
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
            DisReconnectDetector.MenuItems.Add(
                DisReconnectDetector.Menu.AddItem(new MenuItem("SAssembliesDetectorsDisReconnectActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
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
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectChat").GetValue<bool>() &&
                        Menu.GlobalSettings.GetMenuItem("SAssembliesGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say("Champion " + disconnect.Player.ChampionName + " has disconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectSpeech").GetValue<bool>())
                {
                    Speech.Speak("Champion " + disconnect.Player.ChampionName + " has disconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectNotification").GetValue<bool>())
                {
                    Common.ShowNotification("Champion " + disconnect.Player.ChampionName + " has disconnected!", Color.LawnGreen, 3);
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
                    DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectChat").GetValue<bool>() &&
                    Menu.GlobalSettings.GetMenuItem("SAssembliesGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say("Champion " + reconnect.Player.ChampionName + " has reconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectSpeech").GetValue<bool>())
                {
                    Speech.Speak("Champion " + reconnect.Player.ChampionName + " has reconnected!");
                }
                if (DisReconnectDetector.GetMenuItem("SAssembliesDetectorsDisReconnectNotification").GetValue<bool>())
                {
                    Common.ShowNotification("Champion " + reconnect.Player.ChampionName + " has reconnected!", Color.Yellow, 3);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReconnectProcess: " + ex);
            }
        }
    }
}
