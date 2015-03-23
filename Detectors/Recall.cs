using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Detectors
{
    internal class Recall
    {
        public static Menu.MenuItemSettings RecallDetector = new Menu.MenuItemSettings(typeof(Recall));

        public List<Packet.S2C.Teleport.Struct> Recalls = new List<Packet.S2C.Teleport.Struct>();

        public Recall()
        {
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy)
                {
                    Recalls.Add(new Packet.S2C.Teleport.Struct(enemy.NetworkId, Packet.S2C.Teleport.Status.Unknown, Packet.S2C.Teleport.Type.Unknown, 0, 0));
                }
            }
            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
        }

        ~Recall()
        {
            Recalls = null;
        }

        public bool IsActive()
        {
            return Detector.Detectors.GetActive() && RecallDetector.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            RecallDetector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("DETECTORS_RECALL_MAIN"), "SAssembliesDetectorsRecall"));
            RecallDetector.MenuItems.Add(
                RecallDetector.Menu.AddItem(new MenuItem("SAssembliesDetectorsRecallPingTimes", Language.GetString("GLOBAL_PING_TIMES")).SetValue(new Slider(0, 5, 0))));
            RecallDetector.MenuItems.Add(
                RecallDetector.Menu.AddItem(new MenuItem("SAssembliesDetectorsRecallLocalPing", Language.GetString("GLOBAL_PING_LOCAL")).SetValue(true)));
            RecallDetector.MenuItems.Add(
                RecallDetector.Menu.AddItem(new MenuItem("SAssembliesDetectorsRecallChatChoice", Language.GetString("GLOBAL_CHAT_CHOICE")).SetValue(
                        new StringList(new[]
                        {
                            Language.GetString("GLOBAL_CHAT_CHOICE_NONE"), 
                            Language.GetString("GLOBAL_CHAT_CHOICE_LOCAL"), 
                            Language.GetString("GLOBAL_CHAT_CHOICE_SERVER")
                        }))));
            RecallDetector.MenuItems.Add(
                RecallDetector.Menu.AddItem(new MenuItem("SAssembliesDetectorsRecallSpeech", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            RecallDetector.MenuItems.Add(
                RecallDetector.Menu.AddItem(new MenuItem("SAssembliesDetectorsRecallActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return RecallDetector;
        }

        private void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                Packet.S2C.Teleport.Struct decoded = Packet.S2C.Teleport.Decoded(sender, args);
                HandleRecall(decoded);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RecallProcess: " + ex);
            }
        }

        private void HandleRecall(Packet.S2C.Teleport.Struct recallEx)
        {
            int time = Environment.TickCount - Game.Ping;

            for (int i = 0; i < Recalls.Count; i++)
            {
                Packet.S2C.Teleport.Struct recall = Recalls[i];
                if (true/*recallEx.Type == Recall.ObjectType.Player*/)
                {
                    var obj = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recall.UnitNetworkId);
                    var objEx = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recallEx.UnitNetworkId);
                    if (obj == null)
                        continue;
                    if (obj.NetworkId == objEx.NetworkId) //already existing
                    {
                        recall = recallEx;
                        //recall.Recall2 = new Recall.Struct();

                        var percentHealth = (int)((obj.Health / obj.MaxHealth) * 100);
                        String sColor;
                        String hColor = (percentHealth > 50
                            ? "<font color='#00FF00'>"
                            : (percentHealth > 30 ? "<font color='#FFFF00'>" : "<font color='#FF0000'>"));
                        if (recallEx.Status == Packet.S2C.Teleport.Status.Start)
                        {
                            String text = (recallEx.Type == Packet.S2C.Teleport.Type.Recall
                                ? Language.GetString("DETECTORS_RECALL_TEXT_RECALLING")
                                : Language.GetString("DETECTORS_RECALL_TEXT_PORTING"));
                            sColor = "<font color='#FFFF00'>";
                            recall.Start = (int)Game.Time;
                            if (
                                RecallDetector.GetMenuItem("SAssembliesDetectorsRecallChatChoice")
                                    .GetValue<StringList>()
                                    .SelectedIndex == 1)
                            {
                                Game.PrintChat("{0}" + obj.ChampionName + " {1} " + Language.GetString("DETECTORS_RECALL_TEXT_WITH") + " {2} " +
                                    Language.GetString("DETECTORS_RECALL_TEXT_HP") + " {3}({4})", sColor, text,
                                    (int)obj.Health, hColor, percentHealth);
                            }
                            else if (
                                RecallDetector.GetMenuItem("SAssembliesDetectorsRecallChatChoice")
                                    .GetValue<StringList>()
                                    .SelectedIndex == 2 &&
                                Menu.GlobalSettings.GetMenuItem("SAssembliesGlobalSettingsServerChatPingActive")
                                    .GetValue<bool>())
                            {
                                Game.Say("{0}" + obj.ChampionName + " {1} " + Language.GetString("DETECTORS_RECALL_TEXT_WITH") + " {2} " +
                                    Language.GetString("DETECTORS_RECALL_TEXT_HP") + " {3}({4})", sColor, text, (int)obj.Health,
                                    hColor, percentHealth);
                            }
                            if (RecallDetector.GetMenuItem("SAssembliesDetectorsRecallSpeech").GetValue<bool>())
                            {
                                Speech.Speak(obj.ChampionName + " " + text);
                            }
                        }
                        else if (recallEx.Status == Packet.S2C.Teleport.Status.Finish)
                        {
                            String text = (recallEx.Type == Packet.S2C.Teleport.Type.Recall
                                ? Language.GetString("DETECTORS_RECALL_TEXT_RECALLED")
                                : Language.GetString("DETECTORS_RECALL_TEXT_PORTED"));
                            sColor = "<font color='#FF0000'>";
                            if (
                                RecallDetector.GetMenuItem("SAssembliesDetectorsRecallChatChoice")
                                    .GetValue<StringList>()
                                    .SelectedIndex == 1)
                            {
                                Game.PrintChat("{0}" + obj.ChampionName + " {1} " + Language.GetString("DETECTORS_RECALL_TEXT_WITH") + " {2} " +
                                    Language.GetString("DETECTORS_RECALL_TEXT_HP") + " {3}({4})", sColor, text,
                                    (int)obj.Health, hColor, percentHealth);
                            }
                            else if (
                                RecallDetector.GetMenuItem("SAssembliesDetectorsRecallChatChoice")
                                    .GetValue<StringList>()
                                    .SelectedIndex == 2 &&
                                Menu.GlobalSettings.GetMenuItem(
                                    "SAssembliesGlobalSettingsServerChatPingActive").GetValue<bool>())
                            {
                                Game.Say("{0}" + obj.ChampionName + " {1} " + Language.GetString("DETECTORS_RECALL_TEXT_WITH") + " {2} " +
                                    Language.GetString("DETECTORS_RECALL_TEXT_HP") + " {3}({4})", sColor, text,
                                    (int)obj.Health, hColor, percentHealth);
                            }
                            if (RecallDetector.GetMenuItem("SAssembliesDetectorsRecallSpeech").GetValue<bool>())
                            {
                                Speech.Speak(obj.ChampionName + " " + text);
                            }
                        }
                        else
                        {
                            sColor = "<font color='#00FF00'>";
                            if (
                                RecallDetector.GetMenuItem("SAssembliesDetectorsRecallChatChoice")
                                    .GetValue<StringList>()
                                    .SelectedIndex == 1)
                            {
                                Game.PrintChat("{0}" + obj.ChampionName + " " + Language.GetString("DETECTORS_RECALL_TEXT_CANCELED") + " " + 
                                    Language.GetString("DETECTORS_RECALL_TEXT_WITH") + " {1} " +
                                    Language.GetString("DETECTORS_RECALL_TEXT_HP") + "", sColor, (int)obj.Health);
                            }
                            else if (
                                RecallDetector.GetMenuItem("SAssembliesDetectorsRecallChatChoice")
                                    .GetValue<StringList>()
                                    .SelectedIndex == 2 &&
                                Menu.GlobalSettings.GetMenuItem(
                                    "SAssembliesGlobalSettingsServerChatPingActive").GetValue<bool>())
                            {
                                Game.Say("{0}" + obj.ChampionName + " " + Language.GetString("DETECTORS_RECALL_TEXT_CANCELED") + " " 
                                    + Language.GetString("DETECTORS_RECALL_TEXT_WITH") + " {1} " +
                                    Language.GetString("DETECTORS_RECALL_TEXT_HP") + "", sColor, (int)obj.Health);
                            }
                            if (RecallDetector.GetMenuItem("SAssembliesDetectorsRecallSpeech").GetValue<bool>())
                            {
                                Speech.Speak(obj.ChampionName + " " + Language.GetString("DETECTORS_RECALL_TEXT_CANCELED"));
                            }
                        }
                        return;
                    }
                }
            }
        }
    }
}
