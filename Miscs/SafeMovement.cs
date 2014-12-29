using System;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness.Miscs
{
    internal class SafeMovement
    {
        public static Menu.MenuItemSettings SafeMovementMisc = new Menu.MenuItemSettings(typeof(SafeMovement));

        private decimal _lastSend;

        public SafeMovement()
        {
            Game.OnGameSendPacket += Game_OnGameSendPacket;
        }

        ~SafeMovement()
        {
            Game.OnGameSendPacket -= Game_OnGameSendPacket;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && SafeMovementMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            SafeMovementMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_SAFEMOVEMENT_MAIN"), "SAwarenessMiscsSafeMovement"));
            SafeMovementMisc.MenuItems.Add(
                SafeMovementMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsSafeMovementBlockIntervall", Language.GetString("MISCS_SAFEMOVEMENT_BLOCKINTERVAL")).SetValue(new Slider(20, 1000, 0))));
            SafeMovementMisc.MenuItems.Add(
                SafeMovementMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsSafeMovementActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return SafeMovementMisc;
        }

        private void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;

            try
            {
                decimal milli = DateTime.Now.Ticks/(decimal) TimeSpan.TicksPerMillisecond;
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte();
                if (packetId != Packet.C2S.Move.Header)
                    return;
                Packet.C2S.Move.Struct move = Packet.C2S.Move.Decoded(args.PacketData);
                if (move.MoveType == 2)
                {
                    if (move.SourceNetworkId == ObjectManager.Player.NetworkId)
                    {
                        if (milli - _lastSend <
                            SafeMovementMisc.GetMenuItem("SAwarenessMiscsSafeMovementBlockIntervall")
                                .GetValue<Slider>()
                                .Value)
                        {
                            args.Process = false;
                        }
                        else
                        {
                            _lastSend = milli;
                        }
                    }
                }
                else if (move.MoveType == 3)
                {
                    _lastSend = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("MovementProcess: " + ex);
            }
        }
    }
}