using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Timers
{
    class Jungle
    {
        public static Menu.MenuItemSettings JungleTimer = new Menu.MenuItemSettings(typeof(Jungle));

        private static List<JungleMob> JungleMobs = new List<JungleMob>();
        private static List<JungleCamp> JungleCamps = new List<JungleCamp>();
        private static List<Obj_AI_Minion> JungleMobList = new List<Obj_AI_Minion>();
        private static readonly Utility.Map GMap = Utility.Map.GetMap();

        public Jungle()
        {
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            InitJungleMobs();
        }

        ~Jungle()
        {
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;

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
            return JungleMobs.Find(jm => jm.Name == name && jm.MapType == mapType);
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
            if (!IsActive())
                return;
            if (JungleTimer.GetActive())
            {
                foreach (JungleCamp jungleCamp in JungleCamps)
                {
                    if ((jungleCamp.NextRespawnTime - (int)Game.ClockTime) < 0)
                    {
                        jungleCamp.NextRespawnTime = 0;
                        jungleCamp.Called = false;
                    }
                }
            }

            /////

            if (JungleTimer.GetActive())
            {
                foreach (JungleCamp jungleCamp in JungleCamps)
                {
                    if (jungleCamp.NextRespawnTime <= 0 || jungleCamp.MapType != GMap._MapType)
                        continue;
                    int time = Timer.Timers.GetMenuItem("SAwarenessTimersRemindTime").GetValue<Slider>().Value;
                    if (!jungleCamp.Called && jungleCamp.NextRespawnTime - (int)Game.ClockTime <= time &&
                        jungleCamp.NextRespawnTime - (int)Game.ClockTime >= time - 1)
                    {
                        jungleCamp.Called = true;
                        Timer.PingAndCall(jungleCamp.Name + " respawns in " + time + " seconds!", jungleCamp.MinimapPosition);
                    }
                }
            }
        }

        private void UpdateCamps(int networkId, int campId, byte emptyType)
        {
            if (emptyType != 3)
            {
                JungleCamp jungleCamp = GetJungleCampByID(campId, GMap._MapType);
                if (jungleCamp != null)
                {
                    jungleCamp.NextRespawnTime = (int)Game.ClockTime + jungleCamp.RespawnTime;
                }
            }
        }

        private void EmptyCamp(BinaryReader b)
        {
            byte[] h = b.ReadBytes(4);
            int nwId = BitConverter.ToInt32(h, 0);

            h = b.ReadBytes(4);
            int cId = BitConverter.ToInt32(h, 0);

            byte emptyType = b.ReadByte();
            UpdateCamps(nwId, cId, emptyType);
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
                    Packet.S2C.EmptyJungleCamp.Struct decoded = Packet.S2C.EmptyJungleCamp.Decoded(args.PacketData);
                    UpdateCamps(decoded.UnitNetworkId, decoded.CampId, decoded.EmptyType);
                    Log.LogPacket(args.PacketData);
                }
                var stream = new MemoryStream(args.PacketData);
                using (var b = new BinaryReader(stream))
                {
                    int pos = 0;
                    var length = (int)b.BaseStream.Length;
                    while (pos < length)
                    {
                        int v = b.ReadInt32();
                        if (v == 195) //OLD 194
                        {
                            byte[] h = b.ReadBytes(1);
                            EmptyCamp(b);
                        }
                        pos += sizeof(int);
                    }
                }
            }
            catch (EndOfStreamException)
            {
            }
        }

        public void InitJungleMobs()
        {
            JungleMobs.Add(new JungleMob("SRU_Blue", null, true, true, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Murkwolf", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Razorbeak", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Red", null, true, true, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Krug", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_BaronSpawn", null, true, true, true, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Dragon", null, true, false, true, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Gromp", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_Crab", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RedMini", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_MurkwolfMini", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_RazorbeakMini", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_KrugMini", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_BlueMini", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SRU_BlueMini2", null, false, false, false, Utility.Map.MapType.SummonersRift));

            //Twisted Treeline
            JungleMobs.Add(new JungleMob("TT_NWraith", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NGolem", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWolf", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_Spiderboss", null, true, true, true, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_Relic", null, false, false, false, Utility.Map.MapType.TwistedTreeline));

            JungleCamps.Add(new JungleCamp("blue", GameObjectTeam.Order, 1, 115, 300, Utility.Map.MapType.SummonersRift,
                new Vector3(3570, 7670, 54), new Vector3(3641.058f, 8144.426f, 1105.46f),
                new[]
                {
                    GetJungleMobByName("SRU_Blue", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_BlueMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_BlueMini2", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Order, 2, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(3430, 6300, 56), new Vector3(3730.419f, 6744.748f, 1100.24f),
                new[]
                {
                    GetJungleMobByName("SRU_Murkwolf", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_MurkwolfMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_MurkwolfMini", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Order, 3, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(6540, 7230, 56), new Vector3(7069.483f, 5800.1f, 1064.815f),
                new[]
                {
                    GetJungleMobByName("SRU_Razorbeak", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("red", GameObjectTeam.Order, 4, 115, 300, Utility.Map.MapType.SummonersRift,
                new Vector3(7370, 3830, 58), new Vector3(7710.639f, 3963.267f, 1200.182f),
                new[]
                {
                    GetJungleMobByName("SRU_Red", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RedMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RedMini", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Order, 5, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(7990, 2550, 54), new Vector3(8419.813f, 3239.516f, 1280.222f),
                new[]
                {
                    GetJungleMobByName("SRU_Krug", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_KrugMini", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wight", GameObjectTeam.Order, 13, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(1688, 8248, 54), new Vector3(2263.463f, 8571.541f, 1136.772f),
                new[] { GetJungleMobByName("SRU_Gromp", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("blue", GameObjectTeam.Chaos, 7, 115, 300, Utility.Map.MapType.SummonersRift,
                new Vector3(10455, 6800, 55), new Vector3(11014.81f, 7251.099f, 1073.918f),
                new[]
                {
                    GetJungleMobByName("SRU_Blue", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_BlueMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_BlueMini2", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Chaos, 8, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(10570, 8150, 63), new Vector3(11233.96f, 8789.653f, 1051.235f),
                new[]
                {
                    GetJungleMobByName("SRU_Murkwolf", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_MurkwolfMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_MurkwolfMini", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Chaos, 9, 115, 100,
                Utility.Map.MapType.SummonersRift, new Vector3(7465, 9220, 56), new Vector3(7962.764f, 10028.573f, 1023.06f),
                new[]
                {
                    GetJungleMobByName("SRU_Razorbeak", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RazorbeakMini", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("red", GameObjectTeam.Chaos, 10, 115, 300, Utility.Map.MapType.SummonersRift,
                new Vector3(6620, 10637, 55), new Vector3(7164.198f, 11113.5f, 1093.54f),
                new[]
                {
                    GetJungleMobByName("SRU_Red", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RedMini", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_RedMini", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Chaos, 11, 115, 100,
                Utility.Map.MapType.SummonersRift, new Vector3(6010, 11920, 40), new Vector3(6508.562f, 12127.83f, 1185.667f),
                new[]
                {
                    GetJungleMobByName("SRU_Krug", Utility.Map.MapType.SummonersRift),
                    GetJungleMobByName("SRU_KrugMini", Utility.Map.MapType.SummonersRift)
                }));
            JungleCamps.Add(new JungleCamp("wight", GameObjectTeam.Chaos, 14, 115, 100, Utility.Map.MapType.SummonersRift,
                new Vector3(12266, 6215, 54), new Vector3(12671.58f, 6617.756f, 1118.074f),
                new[] { GetJungleMobByName("SRU_Gromp", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("crab", GameObjectTeam.Neutral, 15, 2 * 60 + 30, 180, Utility.Map.MapType.SummonersRift,
                new Vector3(12266, 6215, 54), new Vector3(10557.22f, 5481.414f, 1068.042f),
                new[] { GetJungleMobByName("SRU_Crab", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("crab", GameObjectTeam.Neutral, 16, 2 * 60 + 30, 180, Utility.Map.MapType.SummonersRift,
                new Vector3(12266, 6215, 54), new Vector3(4535.956f, 10104.067f, 1029.071f),
                new[] { GetJungleMobByName("SRU_Crab", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("dragon", GameObjectTeam.Neutral, 6, 2 * 60 + 30, 360,
                Utility.Map.MapType.SummonersRift, new Vector3(9400, 4130, -61), new Vector3(10109.18f, 4850.93f, 1032.274f),
                new[] { GetJungleMobByName("Dragon", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("nashor", GameObjectTeam.Neutral, 12, 20 * 60, 420,
                Utility.Map.MapType.SummonersRift, new Vector3(4620, 10265, -63), new Vector3(4951.034f, 10831.035f, 1027.482f),
                new[] { GetJungleMobByName("Worm", Utility.Map.MapType.SummonersRift) }));

            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Order, 1, 100, 50,
                Utility.Map.MapType.TwistedTreeline, new Vector3(4414, 5774, 60), new Vector3(4414, 5774, 60),
                new[]
                {
                    GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Order, 2, 100, 50,
                Utility.Map.MapType.TwistedTreeline, new Vector3(5088, 8065, 60), new Vector3(5088, 8065, 60),
                new[]
                {
                    GetJungleMobByName("TT_NGolem", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NGolem", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Order, 3, 100, 50,
                Utility.Map.MapType.TwistedTreeline, new Vector3(6148, 5993, 60), new Vector3(6148, 5993, 60),
                new[]
                {
                    GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Chaos, 4, 100, 50,
                Utility.Map.MapType.TwistedTreeline, new Vector3(11008, 5775, 60), new Vector3(11008, 5775, 60),
                new[]
                {
                    GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Chaos, 5, 100, 50,
                Utility.Map.MapType.TwistedTreeline, new Vector3(10341, 8084, 60), new Vector3(10341, 8084, 60),
                new[]
                {
                    GetJungleMobByName("TT_NGolem", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NGolem", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Chaos, 6, 100, 50,
                Utility.Map.MapType.TwistedTreeline, new Vector3(9239, 6022, 60), new Vector3(9239, 6022, 60),
                new[]
                {
                    GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline),
                    GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline)
                }));
            JungleCamps.Add(new JungleCamp("heal", GameObjectTeam.Neutral, 7, 115, 90,
                Utility.Map.MapType.TwistedTreeline, new Vector3(7711, 6722, 60), new Vector3(7711, 6722, 60),
                new[] { GetJungleMobByName("TT_Relic", Utility.Map.MapType.TwistedTreeline) }));
            JungleCamps.Add(new JungleCamp("vilemaw", GameObjectTeam.Neutral, 8, 10 * 60, 300,
                Utility.Map.MapType.TwistedTreeline, new Vector3(7711, 10080, 60), new Vector3(7711, 10080, 60),
                new[] { GetJungleMobByName("TT_Spiderboss", Utility.Map.MapType.TwistedTreeline) }));


            foreach (GameObject objAiBase in ObjectManager.Get<GameObject>())
            {
                Obj_AI_Base_OnCreate(objAiBase, new EventArgs());
            }

            foreach (JungleCamp jungleCamp in JungleCamps) //Game.ClockTime BUGGED
            {
                if (Game.ClockTime > 30) //TODO: Reduce when Game.ClockTime got fixed
                {
                    jungleCamp.NextRespawnTime = 0;
                }
                int nextRespawnTime = jungleCamp.SpawnTime - (int)Game.ClockTime;
                if (nextRespawnTime > 0)
                {
                    jungleCamp.NextRespawnTime = nextRespawnTime;
                }
            }
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
            public GameObjectTeam Team;
            public Render.Text Text;

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
                Text = new Render.Text(0, 0, "", 12, new ColorBGRA(Color4.White));
                Text.TextUpdate = delegate
                {
                    return (NextRespawnTime - (int)Game.ClockTime).ToString();
                };
                Text.PositionUpdate = delegate
                {
                    Vector2 sPos = Drawing.WorldToMinimap(MinimapPosition);
                    return new Vector2(sPos.X, sPos.Y);
                };
                Text.VisibleCondition = sender =>
                {
                    return Timer.Timers.GetActive() && JungleTimer.GetActive() && NextRespawnTime > 0 && MapType == GMap._MapType;
                };
                Text.OutLined = true;
                Text.Centered = true;
                Text.Add();
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
    }
}
