using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace SAwareness.Timers
{
    class Jungle
    {
        public static Menu.MenuItemSettings JungleTimer = new Menu.MenuItemSettings(typeof(Jungle));

        private static List<JungleMob> JungleMobs = new List<JungleMob>();
        private static List<JungleCamp> JungleCamps = new List<JungleCamp>();
        private static List<Obj_AI_Minion> JungleMobList = new List<Obj_AI_Minion>();
        private static List<PlayerBossMobBuff> Enemies = new List<PlayerBossMobBuff>();
        private static readonly Utility.Map GMap = Utility.Map.GetMap();

        private int lastGameUpdateTime = 0;

        public Jungle()
        {
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            //Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            InitJungleMobs();
        }

        ~Jungle()
        {
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            //Game.OnGameProcessPacket -= Game_OnGameProcessPacket;

            JungleMobs = null;
            JungleCamps = null;
            JungleMobList = null;
        }

        public bool IsActive()
        {
            return Timer.Timers.GetActive() && JungleTimer.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            JungleTimer.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TIMERS_JUNGLE_MAIN"), "SAwarenessTimersJungle"));
            JungleTimer.MenuItems.Add(
                JungleTimer.Menu.AddItem(new MenuItem("SAwarenessTimersJungleSpeech", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            JungleTimer.MenuItems.Add(
                JungleTimer.Menu.AddItem(new MenuItem("SAwarenessTimersJungleActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return JungleTimer;
        }

        public bool IsBigMob(Obj_AI_Minion jungleBigMob)
        {
            foreach (JungleMob jungleMob in JungleMobs)
            {
                if (jungleBigMob.Name.Contains(jungleMob.Name))
                {
                    return jungleMob.Smite;
                }
            }
            return false;
        }

        public bool IsBossMob(Obj_AI_Minion jungleBossMob)
        {
            foreach (JungleMob jungleMob in JungleMobs)
            {
                if (jungleBossMob.SkinName.Contains(jungleMob.Name))
                {
                    return jungleMob.Boss;
                }
            }
            return false;
        }

        public bool HasBuff(Obj_AI_Minion jungleBigMob)
        {
            foreach (JungleMob jungleMob in JungleMobs)
            {
                if (jungleBigMob.SkinName.Contains(jungleMob.Name))
                {
                    return jungleMob.Buff;
                }
            }
            return false;
        }

        private JungleMob GetJungleMobByName(string name, Utility.Map.MapType mapType)
        {
            return JungleMobs.Find(jm => name.Contains(jm.Name) && jm.MapType == mapType);
        }

        private JungleCamp GetJungleCampByID(int id, Utility.Map.MapType mapType)
        {
            return JungleCamps.Find(jm => jm.CampId == id && jm.MapType == mapType);
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            if (sender.IsValid)
            {
                if (JungleTimer.GetActive())
                {
                    if (sender.Type == GameObjectType.obj_AI_Minion
                        && sender.Team == GameObjectTeam.Neutral)
                    {
                        if (JungleMobs.Any(mob => sender.Name.Contains(mob.Name)))
                        {
                            JungleMobList.Add((Obj_AI_Minion)sender);
                        }
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
            var minions = ObjectManager.Get<Obj_AI_Base>().Where(minion => !minion.IsDead && minion.IsValid && (minion.Name.ToUpper().StartsWith("SRU") || minion.Name.ToUpper().StartsWith("TT_")));
            foreach (var jungleCamp in JungleCamps)
            {
                if (!jungleCamp.Dead)
                {
                    bool alive = false;
                    foreach (var creep in jungleCamp.Creeps)
                    {
                        foreach (var minion in minions)
                        {
                            if (minion.Name.ToUpper().Equals(creep.Name.ToUpper()))
                            {
                                alive = true;
                                break;
                            }
                        }
                        if (alive == true)
                        {
                            break;
                        }
                    }
                    if (alive == true)
                    {
                        jungleCamp.Visible = true;
                    }
                }
            }

            foreach (var jungleCamp in JungleCamps)
            {
                if (!jungleCamp.Dead && jungleCamp.Visible)
                {
                    int count = 0;
                    foreach (var creep in jungleCamp.Creeps)
                    {
                        foreach (var minion in minions)
                        {
                            if (minion.Name.ToUpper().Equals(creep.Name.ToUpper()))
                            {
                                count++;
                            }
                        }
                    }
                    if (count == 0)
                    {
                        jungleCamp.NextRespawnTime = (int)Game.ClockTime + jungleCamp.RespawnTime;
                        jungleCamp.Dead = true;
                        jungleCamp.Visible = false;
                    }
                }
            }

            /////////

            foreach (var enemyInfo in Enemies)
            {
                if (enemyInfo.DragonBuff < enemyInfo.GetDragonStacks())
                {
                    foreach (var jungleCamp in JungleCamps)
                    {
                        if (jungleCamp.CampId == 6)
                        {
                            jungleCamp.NextRespawnTime = (int)Game.ClockTime + jungleCamp.RespawnTime;
                            jungleCamp.Dead = true;
                            jungleCamp.Visible = false;
                        }
                    }
                }

                if (enemyInfo.NashorBuff != enemyInfo.HasNashorBuff())
                {
                    foreach (var jungleCamp in JungleCamps)
                    {
                        if (jungleCamp.CampId == 12 && !jungleCamp.Called)
                        {
                            jungleCamp.NextRespawnTime = (int)Game.ClockTime + jungleCamp.RespawnTime;
                            jungleCamp.Dead = true;
                            jungleCamp.Visible = false;
                        }
                    }
                }

                enemyInfo.DragonBuff = enemyInfo.GetDragonStacks();
                enemyInfo.NashorBuff = enemyInfo.HasNashorBuff();
            }

            /////////

            if (JungleTimer.GetActive())
            {
                foreach (JungleCamp jungleCamp in JungleCamps)
                {
                    if ((jungleCamp.NextRespawnTime - (int) Game.ClockTime) < 0)
                    {
                        jungleCamp.NextRespawnTime = 0;
                        jungleCamp.Called = false;
                        jungleCamp.Dead = false;
                    }
                }
            }

            /////

            if (JungleTimer.GetActive())
            {
                foreach (JungleCamp jungleCamp in JungleCamps)
                {
                    if (jungleCamp.NextRespawnTime <= 0 || jungleCamp.MapType != GMap.Type)
                        continue;
                    int time = Timer.Timers.GetMenuItem("SAwarenessTimersRemindTime").GetValue<Slider>().Value;
                    if (!jungleCamp.Called && jungleCamp.NextRespawnTime - (int) Game.ClockTime <= time &&
                        jungleCamp.NextRespawnTime - (int) Game.ClockTime >= time - 1)
                    {
                        jungleCamp.Called = true;
                        Timer.PingAndCall(jungleCamp.Name + " respawns in " + time + " seconds!",
                            jungleCamp.MinimapPosition);
                        if (JungleTimer.GetMenuItem("SAwarenessTimersJungleSpeech").GetValue<bool>())
                        {
                            Speech.Speak(jungleCamp.Name + " respawns in " + time + " seconds!");
                        }
                    }
                }
            }
        }

        private void UpdateCamps(int networkId, int campId/*, byte emptyType*/)
        {
            /*if (emptyType != 3)
            {*/
                JungleCamp jungleCamp = GetJungleCampByID(campId, GMap.Type);
                if (jungleCamp != null)
                {
                    jungleCamp.NextRespawnTime = (int)Game.ClockTime + jungleCamp.RespawnTime;
                }
            //}
        }

        private void EmptyCamp(BinaryReader b)
        {
            byte[] h = b.ReadBytes(4);
            int nwId = BitConverter.ToInt32(h, 0);

            h = b.ReadBytes(4);
            int cId = BitConverter.ToInt32(h, 0);

            //byte emptyType = b.ReadByte();
            UpdateCamps(nwId, cId/*, emptyType*/);
        }

        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;
            if (!JungleTimer.GetActive())
                return;
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));

                byte packetId = reader.ReadByte();
                if (packetId == Packet.S2C.EmptyJungleCamp.Header)
                {
                    //Packet.S2C.EmptyJungleCamp.Struct decoded = Packet.S2C.EmptyJungleCamp.Decoded(args.PacketData);
                    //UpdateCamps(decoded.UnitNetworkId, decoded.CampId, decoded.EmptyType);
                    //Log.LogPacket(args.PacketData);
                    var packet = new GamePacket(args.PacketData);
                    var result = new Packet.S2C.EmptyJungleCamp.Struct();
                    result.CampId = packet.ReadInteger(6);
                    result.UnitNetworkId = packet.ReadInteger();
                    UpdateCamps(result.UnitNetworkId, result.CampId);
                }
                if (packetId == Packet.S2C.EmptyJungleCamp.Header)
                {
                    var stream = new MemoryStream(args.PacketData);
                    using (var b = new BinaryReader(stream))
                    {
                        int pos = 0;
                        var length = (int) b.BaseStream.Length;
                        while (pos < length)
                        {
                            int v = b.ReadInt32();
                            if (v == 195) //OLD 194
                            {
                                byte[] h = b.ReadBytes(1);
                                EmptyCamp(b);
                            }
                            pos += sizeof (int);
                        }
                    }
                }
            }
            catch (EndOfStreamException)
            {
            }
        }

        public void InitJungleMobs()
        {
            JungleMobs.Add(new JungleMob("SRU_Baron12.1.1", null, true, true, true, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Dragon6.1.1", null, true, false, true, Utility.Map.MapType.SummonersRift));

            JungleMobs.Add(new JungleMob("SRU_Blue1.1.1", null, true, true, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Murkwolf2.1.1", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Razorbeak3.1.1", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Red4.1.1", null, true, true, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Krug5.1.2", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Gromp13.1.1", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Crab15.1.1", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RedMini4.1.2", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RedMini4.1.3", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_MurkwolfMini2.1.2", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_MurkwolfMini2.1.3", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RazorbeakMini3.1.2", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RazorbeakMini3.1.3", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RazorbeakMini3.1.4", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_KrugMini5.1.1", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_BlueMini1.1.2", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_BlueMini21.1.3", null, false, false, false, Utility.Map.MapType.SummonersRift));

            JungleMobs.Add(new JungleMob("SRU_Blue7.1.1", null, true, true, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Murkwolf8.1.1", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Razorbeak9.1.1", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Red10.1.1", null, true, true, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Krug11.1.2", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Gromp14.1.1", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Crab16.1.1", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RedMini10.1.2", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RedMini10.1.3", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_MurkwolfMini8.1.2", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_MurkwolfMini8.1.3", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RazorbeakMini9.1.2", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RazorbeakMini9.1.3", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RazorbeakMini9.1.4", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_KrugMini11.1.1", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_BlueMini7.1.2", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_BlueMini27.1.3", null, false, false, false, Utility.Map.MapType.SummonersRift));

            //Twisted Treeline
            JungleMobs.Add(new JungleMob("TT_Spiderboss8.1.1", null, true, true, true, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_Relic7.1.1", null, false, false, false, Utility.Map.MapType.TwistedTreeline));

            JungleMobs.Add(new JungleMob("TT_NWraith1.1.1", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWraith21.1.2", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWraith21.1.3", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NGolem2.1.1", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NGolem22.1.2", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWolf3.1.1", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWolf23.1.2", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWolf23.1.3", null, false, false, false, Utility.Map.MapType.TwistedTreeline));

            JungleMobs.Add(new JungleMob("TT_NWraith4.1.1", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWraith24.1.2", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWraith24.1.3", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NGolem5.1.1", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NGolem25.1.2", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWolf6.1.1", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWolf26.1.2", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWolf26.1.3", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            

            JungleCamps.Add(new JungleCamp("blue", GameObjectTeam.Order, 1, 115, 300, Utility.Map.MapType.SummonersRift,
                new Vector3(3872, 7926, 51), new Vector3(3641.058f, 8144.426f, 1105.46f),
                new[]
                {
                    GetJungleMobByName("SRU_Blue1.1.1", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_BlueMini1.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_BlueMini21.1.3", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Order, 2, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(3920, 6536, 52), new Vector3(3730.419f, 6744.748f, 1100.24f),
                new[]
                {
                    GetJungleMobByName("SRU_Murkwolf2.1.1", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_MurkwolfMini2.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_MurkwolfMini2.1.3", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Order, 3, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(6982, 5408, 52), new Vector3(7069.483f, 5800.1f, 1064.815f),
                new[]
                {
                    GetJungleMobByName("SRU_Razorbeak3.1.1", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini3.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini3.1.3", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini3.1.4", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("red", GameObjectTeam.Order, 4, 115, 300, Utility.Map.MapType.SummonersRift,
                new Vector3(7752, 4010, 54), new Vector3(7710.639f, 3963.267f, 1200.182f),
                new[]
                {
                    GetJungleMobByName("SRU_Red4.1.1", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RedMini4.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RedMini4.1.3", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Order, 5, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(8414f, 2678f, 50.79845f), new Vector3(8419.813f, 3239.516f, 1280.222f),
                new[]
                {
                    GetJungleMobByName("SRU_Krug5.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_KrugMini5.1.1", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wight", GameObjectTeam.Order, 13, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(2282, 8388, 51), new Vector3(2263.463f, 8571.541f, 1136.772f),
                new[] { GetJungleMobByName("SRU_Gromp13.1.1", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("blue", GameObjectTeam.Chaos, 7, 115, 300, Utility.Map.MapType.SummonersRift,
                new Vector3(11142, 6820, 51), new Vector3(11014.81f, 7251.099f, 1073.918f),
                new[]
                {
                    GetJungleMobByName("SRU_Blue7.1.1", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_BlueMini7.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_BlueMini27.1.3", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Chaos, 8, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(10886, 8230, 62), new Vector3(11233.96f, 8789.653f, 1051.235f),
                new[]
                {
                    GetJungleMobByName("SRU_Murkwolf8.1.1", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_MurkwolfMini8.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_MurkwolfMini8.1.3", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Chaos, 9, 115, 100,
                Utility.Map.MapType.SummonersRift, new Vector3(7884, 9466, 52), new Vector3(7962.764f, 10028.573f, 1023.06f),
                new[]
                {
                    GetJungleMobByName("SRU_Razorbeak9.1.1", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini9.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini9.1.3", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini9.1.4", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("red", GameObjectTeam.Chaos, 10, 115, 300, Utility.Map.MapType.SummonersRift,
                new Vector3(7106, 10930, 56), new Vector3(7164.198f, 11113.5f, 1093.54f),
                new[]
                {
                    GetJungleMobByName("SRU_Red10.1.1", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RedMini10.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RedMini10.1.3", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Chaos, 11, 115, 100,
                Utility.Map.MapType.SummonersRift, new Vector3(6476, 12268, 56), new Vector3(6508.562f, 12127.83f, 1185.667f),
                new[]
                {
                    GetJungleMobByName("SRU_Krug11.1.2", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_KrugMini11.1.1", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wight", GameObjectTeam.Chaos, 14, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(12668, 6360, 51), new Vector3(12671.58f, 6617.756f, 1118.074f),
                new[] { GetJungleMobByName("SRU_Gromp14.1.1", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("crab", GameObjectTeam.Neutral, 15, 2 * 60 + 30, 180, Utility.Map.MapType.SummonersRift,
                new Vector3(10586, 5114, -62), new Vector3(10586, 5114, -62),
                new[] { GetJungleMobByName("SRU_Crab15.1.1", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("crab", GameObjectTeam.Neutral, 16, 2 * 60 + 30, 180, Utility.Map.MapType.SummonersRift,
                new Vector3(4274, 9696, -68), new Vector3(4274, 9696, -68),
                new[] { GetJungleMobByName("SRU_Crab16.1.1", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("dragon", GameObjectTeam.Neutral, 6, 2 * 60 + 30, 360,
                Utility.Map.MapType.SummonersRift, new Vector3(10116, 4438, -71), new Vector3(10109.18f, 4850.93f, 1032.274f),
                new[] { GetJungleMobByName("SRU_Dragon6.1.1", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("nashor", GameObjectTeam.Neutral, 12, 20 * 60, 420,
                Utility.Map.MapType.SummonersRift, new Vector3(4940, 10406, -71), new Vector3(4951.034f, 10831.035f, 1027.482f),
                new[] { GetJungleMobByName("SRU_Baron12.1.1", Utility.Map.MapType.SummonersRift) }));

            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Order, 1, 100, 75,
                Utility.Map.MapType.TwistedTreeline, new Vector3(4270, 5871, -106), new Vector3(4414, 5774, 60),
                new[]
                {
                    GetJungleMobByName("TT_NWraith1.1.1", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWraith21.1.2", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWraith21.1.3", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Order, 2, 100, 75,
                Utility.Map.MapType.TwistedTreeline, new Vector3(5034, 7929, -107), new Vector3(5088, 8065, 60),
                new[]
                {
                    GetJungleMobByName("TT_NGolem2.1.1", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NGolem22.1.2", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Order, 3, 100, 75,
                Utility.Map.MapType.TwistedTreeline, new Vector3(6014, 6183, -98), new Vector3(6148, 5993, 60),
                new[]
                {
                    GetJungleMobByName("TT_NWolf3.1.1", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWolf23.1.2", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWolf23.1.3", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Chaos, 4, 100, 75,
                Utility.Map.MapType.TwistedTreeline, new Vector3(11022, 5815, -107), new Vector3(11008, 5775, 60),
                new[]
                {
                    GetJungleMobByName("TT_NWraith4.1.1", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWraith24.1.2", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWraith24.1.3", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Chaos, 5, 100, 75,
                Utility.Map.MapType.TwistedTreeline, new Vector3(10332, 7925, -108), new Vector3(10341, 8084, 60),
                new[]
                {
                    GetJungleMobByName("TT_NGolem5.1.1", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NGolem25.1.2", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Chaos, 6, 100, 75,
                Utility.Map.MapType.TwistedTreeline, new Vector3(9394, 6085, -95), new Vector3(9239, 6022, 60),
                new[]
                {
                    GetJungleMobByName("TT_NWolf6.1.1", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWolf26.1.2", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWolf26.1.3", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("heal", GameObjectTeam.Neutral, 7, 115, 90,
                Utility.Map.MapType.TwistedTreeline, new Vector3(7712, 6713, -69), new Vector3(7711, 6722, 60),
                new[] { GetJungleMobByName("TT_Relic7.1.1", Utility.Map.MapType.TwistedTreeline) }));
            JungleCamps.Add(new JungleCamp("vilemaw", GameObjectTeam.Neutral, 8, 10 * 60, 300,
                Utility.Map.MapType.TwistedTreeline, new Vector3(7726, 9937, -79), new Vector3(7711, 10080, 60),
                new[] { GetJungleMobByName("TT_Spiderboss8.1.1", Utility.Map.MapType.TwistedTreeline) }));


            foreach (GameObject objAiBase in ObjectManager.Get<GameObject>())
            {
                Obj_AI_Base_OnCreate(objAiBase, new EventArgs());
            }

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if(!hero.IsEnemy)
                    continue;

                Enemies.Add(new PlayerBossMobBuff(hero));
            }

            //foreach (JungleCamp jungleCamp in JungleCamps) //Game.ClockTime BUGGED
            //{
            //    if (Game.ClockTime > 30) //TODO: Reduce when Game.ClockTime got fixed
            //    {
            //        jungleCamp.NextRespawnTime = 0;
            //    }
            //    int nextRespawnTime = jungleCamp.SpawnTime - (int)Game.ClockTime;
            //    if (nextRespawnTime > 0)
            //    {
            //        jungleCamp.NextRespawnTime = nextRespawnTime;
            //    }
            //}
        }

        public class JungleCamp
        {
            public bool Called;
            public int CampId;
            public JungleMob[] Creeps;
            public Vector3 MapPosition;
            public Utility.Map.MapType MapType;
            public Vector3 MinimapPosition;
            public String Name;
            public int NextRespawnTime;
            public int RespawnTime;
            public int SpawnTime;
            public bool Dead;
            public bool Visible;
            public GameObjectTeam Team;
            public Render.Text TextMinimap;
            public Render.Text TextMap;

            public JungleCamp(String name, GameObjectTeam team, int campId, int spawnTime, int respawnTime,
                Utility.Map.MapType mapType, Vector3 mapPosition, Vector3 minimapPosition, JungleMob[] creeps)
            {
                Name = name;
                Team = team;
                CampId = campId;
                SpawnTime = spawnTime;
                RespawnTime = respawnTime;
                MapType = mapType;
                MapPosition = mapPosition;
                MinimapPosition = minimapPosition;
                Creeps = creeps;
                NextRespawnTime = 0;
                Called = false;
                Dead = false;
                Visible = false;
                TextMinimap = new Render.Text(0, 0, "", Timer.Timers.GetMenuItem("SAwarenessTimersTextScale").GetValue<Slider>().Value, new ColorBGRA(Color4.White));
                Timer.Timers.GetMenuItem("SAwarenessTimersTextScale").ValueChanged += JungleCamp_ValueChanged;
                TextMinimap.TextUpdate = delegate
                {
                    return (NextRespawnTime - (int)Game.ClockTime).ToString();
                };
                TextMinimap.PositionUpdate = delegate
                {
                    Vector2 sPos = Drawing.WorldToMinimap(MinimapPosition);
                    return new Vector2(sPos.X, sPos.Y);
                };
                TextMinimap.VisibleCondition = sender =>
                {
                    return Timer.Timers.GetActive() && JungleTimer.GetActive() && NextRespawnTime > 0 && MapType == GMap.Type;
                };
                TextMinimap.OutLined = true;
                TextMinimap.Centered = true;
                TextMinimap.Add();
                TextMap = new Render.Text(0, 0, "", (int)(Timer.Timers.GetMenuItem("SAwarenessTimersTextScale").GetValue<Slider>().Value * 3.5), new ColorBGRA(Color4.White));
                TextMap.TextUpdate = delegate
                {
                    return (NextRespawnTime - (int)Game.ClockTime).ToString();
                };
                TextMap.PositionUpdate = delegate
                {
                    Vector2 sPos = Drawing.WorldToScreen(MapPosition);
                    return new Vector2(sPos.X, sPos.Y);
                };
                TextMap.VisibleCondition = sender =>
                {
                    return Timer.Timers.GetActive() && JungleTimer.GetActive() && NextRespawnTime > 0 && MapType == GMap.Type;
                };
                TextMap.OutLined = true;
                TextMap.Centered = true;
                TextMap.Add();
            }

            void JungleCamp_ValueChanged(object sender, OnValueChangeEventArgs e)
            {
                TextMinimap.Remove();
                TextMinimap.TextFontDescription = new FontDescription
                {
                    FaceName = "Calibri",
                    Height = e.GetNewValue<Slider>().Value,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default,
                };
                TextMinimap.Add();
                TextMap.Remove();
                TextMap.TextFontDescription = new FontDescription
                {
                    FaceName = "Calibri",
                    Height = e.GetNewValue<Slider>().Value,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default,
                };
                TextMap.Add();
            }
        }

        public class JungleMob
        {
            public bool Boss;
            public bool Buff;
            public Utility.Map.MapType MapType;
            public String Name;
            public Obj_AI_Minion Obj;
            public bool Smite;

            public JungleMob(string name, Obj_AI_Minion obj, bool smite, bool buff, bool boss,
                Utility.Map.MapType mapType)
            {
                Name = name;
                Obj = obj;
                Smite = smite;
                Buff = buff;
                Boss = boss;
                MapType = mapType;
            }
        }

        public class PlayerBossMobBuff
        {
            public int DragonBuff = 0;
            public bool NashorBuff = false;
            public Obj_AI_Hero Hero;

            public PlayerBossMobBuff(Obj_AI_Hero hero)
            {
                Hero = hero;
            }

            public int GetDragonStacks()
            {
                var buff = Hero.Buffs.FirstOrDefault(x => x.Name == "s5test_dragonslayerbuff");
                if (buff == null)
                    return 0;
                else
                    return buff.Count;
            }
            public bool HasNashorBuff()
            {
                return Hero.Buffs.Any(x => x.Name == "exaltedwithbaronnashor");
            }
        }
    }
}
