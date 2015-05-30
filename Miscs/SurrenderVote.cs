using System;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Miscs
{
    internal class SurrenderVote
    {
        public static Menu.MenuItemSettings SurrenderVoteMisc = new Menu.MenuItemSettings(typeof(SurrenderVote));

        private int _lastNoVoteCount;

        public SurrenderVote()
        {
            Game.OnProcessPacket += Game_OnGameProcessPacket;
        }

        ~SurrenderVote()
        {
            Game.OnProcessPacket -= Game_OnGameProcessPacket;
        }

        public bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && SurrenderVoteMisc.GetActive();
#else
            return SurrenderVoteMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SurrenderVoteMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_SURRENDERVOTE_MAIN"), "SAssembliesMiscsSurrenderVote"));
            SurrenderVoteMisc.MenuItems.Add(
                SurrenderVoteMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsSurrenderVoteChat", Language.GetString("GLOBAL_CHAT")).SetValue(false)));
            SurrenderVoteMisc.MenuItems.Add(
                SurrenderVoteMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsSurrenderVoteActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SurrenderVoteMisc;
        }

        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;

            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte(); //PacketId
                if (packetId != Packet.C2S.Surrender.Header)
                    return;
                Packet.S2C.Surrender.Struct surrender = Packet.S2C.Surrender.Decoded(args.PacketData);

                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (hero.NetworkId == surrender.NetworkId)
                    {
                        if (surrender.NoVotes > _lastNoVoteCount)
                        {
                            if (
                                SurrenderVoteMisc.GetMenuItem("SAssembliesMiscsSurrenderVoteChat").GetValue<bool>() &&
                                Menu.GlobalSettings.GetMenuItem("SAssembliesGlobalSettingsServerChatPingActive")
                                    .GetValue<bool>())
                            {
                                Game.Say("{0} voted NO", hero.ChampionName);
                            }
                        }
                        else
                        {
                            if (
                                SurrenderVoteMisc.GetMenuItem("SAssembliesMiscsSurrenderVoteChat").GetValue<bool>() &&
                                Menu.GlobalSettings.GetMenuItem("SAssembliesGlobalSettingsServerChatPingActive")
                                    .GetValue<bool>())
                            {
                                Game.Say("{0} voted YES", hero.ChampionName);
                            }
                        }
                        break;
                    }
                }
                _lastNoVoteCount = surrender.NoVotes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SurrenderProcess: " + ex);
            }
        }
    }
}