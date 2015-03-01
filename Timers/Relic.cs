using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace SAwareness.Timers
{
    class Relic
    {
        public static Menu.MenuItemSettings RelicTimer = new Menu.MenuItemSettings(typeof(Relic));

        private static readonly Utility.Map GMap = Utility.Map.GetMap();
        private static List<RelicObject> Relics = new List<RelicObject>();
        private int lastGameUpdateTime = 0;

        public Relic()
        {
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            InitRelicObjects();
        }

        ~Relic()
        {
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Relics = null;
        }

        public bool IsActive()
        {
            return Timer.Timers.GetActive() && RelicTimer.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            RelicTimer.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TIMERS_RELIC_MAIN"), "SAwarenessTimersRelic"));
            RelicTimer.MenuItems.Add(
                RelicTimer.Menu.AddItem(new MenuItem("SAwarenessTimersRelicSpeech", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            RelicTimer.MenuItems.Add(
                RelicTimer.Menu.AddItem(new MenuItem("SAwarenessTimersRelicActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return RelicTimer;
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            if (sender.IsValid)
            {
                if (RelicTimer.GetActive())
                {
                    foreach (RelicObject relic in Relics)
                    {
                        if (sender.Name.Contains(relic.ObjectName))
                        {
                            relic.Obj = sender;
                            relic.Locked = false;
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

            if (RelicTimer.GetActive())
            {
                foreach (RelicObject relic in Relics)
                {
                    if (!relic.Locked && (relic.Obj != null && (!relic.Obj.IsValid || relic.Obj.IsDead)))
                    {
                        if (Game.ClockTime < relic.SpawnTime)
                        {
                            relic.NextRespawnTime = relic.SpawnTime - (int)Game.ClockTime;
                        }
                        else
                        {
                            relic.NextRespawnTime = relic.RespawnTime + (int)Game.ClockTime;
                        }
                        relic.Locked = true;
                    }
                    if ((relic.NextRespawnTime - (int)Game.ClockTime) < 0)
                    {
                        relic.NextRespawnTime = 0;
                        relic.Called = false;
                    }
                }
            }

            if (RelicTimer.GetActive())
            {
                foreach (RelicObject relic in Relics)
                {
                    if (relic.Locked)
                    {
                        if (relic.NextRespawnTime <= 0 || relic.MapType != GMap.Type)
                            continue;
                        int time = Timer.Timers.GetMenuItem("SAwarenessTimersRemindTime").GetValue<Slider>().Value;
                        if (!relic.Called && relic.NextRespawnTime - (int)Game.ClockTime <= time &&
                            relic.NextRespawnTime - (int)Game.ClockTime >= time - 1)
                        {
                            relic.Called = true;
                            Timer.PingAndCall(relic.Name + " respawns in " + time + " seconds!", relic.MinimapPosition);
                            if (RelicTimer.GetMenuItem("SAwarenessTimersRelicSpeech").GetValue<bool>())
                            {
                                Speech.Speak(relic.Name + " respawns in " + time + " seconds!");
                            }
                        }
                    }
                }
            }
        }

        public void InitRelicObjects()
        {
            //Crystal Scar
            Relics.Add(new RelicObject(
                ObjectManager.Player.Team == GameObjectTeam.Order ? "Relic Green" : "Relic Red",
                ObjectManager.Player.Team == GameObjectTeam.Order ? "Odin_Prism_Green.troy" : "Odin_Prism_Red.troy",
                GameObjectTeam.Order, null, 180, 180, new Vector3(6799, 6475, -166), new Vector3(6799, 6475, -166)));
            Relics.Add(new RelicObject(
                ObjectManager.Player.Team == GameObjectTeam.Chaos ? "Relic Green" : "Relic Red",
                ObjectManager.Player.Team == GameObjectTeam.Chaos ? "Odin_Prism_Green.troy" : "Odin_Prism_Red.troy",
                GameObjectTeam.Chaos, null, 180, 180, new Vector3(7137, 6461, -166), new Vector3(7137, 6461, -166)));

            foreach (GameObject objAiBase in ObjectManager.Get<GameObject>())
            {
                Obj_AI_Base_OnCreate(objAiBase, new EventArgs());
            }
        }

        public class RelicObject
        {
            public bool Called;
            public bool Locked;
            public Vector3 MapPosition;
            public Utility.Map.MapType MapType;
            public Vector3 MinimapPosition;
            public String Name;
            public int NextRespawnTime;
            public GameObject Obj;
            public String ObjectName;
            public int RespawnTime;
            public int SpawnTime;
            public GameObjectTeam Team;
            public Render.Text TextMinimap;
            public Render.Text TextMap;

            public RelicObject(string name, String objectName, GameObjectTeam team, Obj_AI_Minion obj, int spawnTime,
                int respawnTime, Vector3 mapPosition, Vector3 minimapPosition)
            {
                Name = name;
                ObjectName = objectName;
                Team = team;
                Obj = obj;
                SpawnTime = spawnTime;
                RespawnTime = respawnTime;
                Locked = false;
                MapPosition = mapPosition;
                MinimapPosition = minimapPosition;
                MapType = Utility.Map.MapType.CrystalScar;
                NextRespawnTime = 0;
                Called = false;
                TextMinimap = new Render.Text(0, 0, "", Timer.Timers.GetMenuItem("SAwarenessTimersTextScale").GetValue<Slider>().Value, new ColorBGRA(Color4.White));
                Timer.Timers.GetMenuItem("SAwarenessTimersTextScale").ValueChanged += RelicObject_ValueChanged;
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
                    return Timer.Timers.GetActive() && RelicTimer.GetActive() && NextRespawnTime > 0 && MapType == GMap.Type;
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
                    return Timer.Timers.GetActive() && RelicTimer.GetActive() && NextRespawnTime > 0 && MapType == GMap.Type;
                };
                TextMap.OutLined = true;
                TextMap.Centered = true;
                TextMap.Add();
            }

            void RelicObject_ValueChanged(object sender, OnValueChangeEventArgs e)
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
    }
}
