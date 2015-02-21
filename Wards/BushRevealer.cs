using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Wards
{
    internal class BushRevealer //By Beaving & Blm95
    {
        public static Menu.MenuItemSettings BushRevealerWard = new Menu.MenuItemSettings(typeof(BushRevealer));

        private List<PlayerInfo> _playerInfo = new List<PlayerInfo>();
        private int _lastTimeWarded;
        private int lastGameUpdateTime = 0;

        public BushRevealer()
        {
            _playerInfo = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy).Select(x => new PlayerInfo(x)).ToList();
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
            //Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~BushRevealer()
        {
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            _playerInfo = null;
        }

        public bool IsActive()
        {
            return Ward.Wards.GetActive() && BushRevealerWard.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            BushRevealerWard.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("WARDS_BUSHREVEALER_MAIN"), "SAwarenessWardsBushRevealer"));
            BushRevealerWard.MenuItems.Add(
                BushRevealerWard.Menu.AddItem(new MenuItem("SAwarenessWardsBushRevealerKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(32, KeyBindType.Press))));
            BushRevealerWard.MenuItems.Add(
                BushRevealerWard.Menu.AddItem(new MenuItem("SAwarenessWardsBushRevealerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            BushRevealerWard.MenuItems.Add(
                BushRevealerWard.Menu.AddItem(new MenuItem("By Beaving & Blm95", "By Beaving & Blm95")));
            return BushRevealerWard;
        }

        private void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            int time = Environment.TickCount;

            foreach (PlayerInfo playerInfo in _playerInfo.Where(x => x.Player.IsVisible))
                playerInfo.LastSeen = time;

            Ward.WardItem ward = Ward.GetWardItem();
            if (ward == null)
                return;

            if (BushRevealerWard.GetMenuItem("SAwarenessWardsBushRevealerKey").GetValue<KeyBind>().Active)
            {
                foreach (Obj_AI_Hero enemy in _playerInfo.Where(x =>
                    x.Player.IsValid &&
                    !x.Player.IsVisible &&
                    !x.Player.IsDead &&
                    x.Player.Distance(ObjectManager.Player.ServerPosition) < 1000 && //check real ward range later
                    time - x.LastSeen < 2500).Select(x => x.Player))
                {
                    Vector3 bestWardPos = GetWardPos(enemy.ServerPosition, 165, 2);

                    if (bestWardPos != enemy.ServerPosition && bestWardPos != Vector3.Zero &&
                        bestWardPos.Distance(ObjectManager.Player.ServerPosition) < ward.Range)
                    {
                        if (_lastTimeWarded == 0 || Environment.TickCount - _lastTimeWarded > 500)
                        {
                            InventorySlot wardSlot = Ward.GetWardSlot();

                            if (wardSlot != null && wardSlot.Id != ItemId.Unknown)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, bestWardPos);
                                _lastTimeWarded = Environment.TickCount;
                            }
                        }
                    }
                }
            }
        }

        private Vector3 GetWardPos(Vector3 lastPos, int radius = 165, int precision = 3)
        {
            //Vector3 averagePos = Vector3.Zero;

            int count = precision;
            //int calculated = 0;

            while (count > 0)
            {
                int vertices = radius;

                var wardLocations = new WardLocation[vertices];
                double angle = 2*Math.PI/vertices;

                for (int i = 0; i < vertices; i++)
                {
                    double th = angle*i;
                    var pos = new Vector3((float) (lastPos.X + radius*Math.Cos(th)),
                        (float) (lastPos.Y + radius*Math.Sin(th)), 0); //wardPos.Z
                    wardLocations[i] = new WardLocation(pos, NavMesh.IsWallOfGrass(pos, 10));
                }

                var grassLocations = new List<GrassLocation>();

                for (int i = 0; i < wardLocations.Length; i++)
                {
                    if (wardLocations[i].Grass)
                    {
                        if (i != 0 && wardLocations[i - 1].Grass)
                            grassLocations.Last().Count++;
                        else
                            grassLocations.Add(new GrassLocation(i, 1));
                    }
                }

                GrassLocation grassLocation = grassLocations.OrderByDescending(x => x.Count).FirstOrDefault();

                if (grassLocation != null) //else: no pos found. increase/decrease radius?
                {
                    var midelement = (int) Math.Ceiling(grassLocation.Count/2f);
                    //averagePos += wardLocations[grassLocation.Index + midelement - 1].Pos; //uncomment if using averagePos
                    lastPos = wardLocations[grassLocation.Index + midelement - 1].Pos; //comment if using averagePos
                    radius = (int) Math.Floor(radius/2f); //precision recommended: 2-3; comment if using averagePos

                    //calculated++; //uncomment if using averagePos
                }

                count--;
            }

            return lastPos; //averagePos /= calculated; //uncomment if using averagePos
        }

        private class GrassLocation
        {
            public readonly int Index;
            public int Count;

            public GrassLocation(int index, int count)
            {
                Index = index;
                Count = count;
            }
        }

        private class PlayerInfo
        {
            public readonly Obj_AI_Hero Player;
            public int LastSeen;

            public PlayerInfo(Obj_AI_Hero player)
            {
                Player = player;
            }
        }

        private class WardLocation
        {
            public readonly bool Grass;
            public readonly Vector3 Pos;

            public WardLocation(Vector3 pos, bool grass)
            {
                Pos = pos;
                Grass = grass;
            }
        }
    }
}
