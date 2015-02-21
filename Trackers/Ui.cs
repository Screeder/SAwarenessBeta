using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAwareness.Spectator;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;
using Packet = LeagueSharp.Common.Packet;

namespace SAwareness.Trackers
{
    class Ui
    {
        public static Menu.MenuItemSettings UiTracker = new Menu.MenuItemSettings(typeof(Ui));

        public static readonly Dictionary<Obj_AI_Hero, ChampInfos> _allies = new Dictionary<Obj_AI_Hero, ChampInfos>();
        public static readonly Dictionary<Obj_AI_Hero, ChampInfos> _enemies = new Dictionary<Obj_AI_Hero, ChampInfos>();
        private static Size _backBarSize = new Size(96, 10);
        private static Size _champSize = new Size(64, 64);
        private static Size _healthManaBarSize = new Size(96, 5);
        private static Size _recSize = new Size(64, 12);
        private static Vector2 _screen = new Vector2(Drawing.Width, Drawing.Height/2);
        private static Size _spellSize = new Size(16, 16);
        private static Size _sumSize = new Size(32, 32);

        private static Size _lvlIconSize = new Size(17, 17);

        private Size _hudSize;
        private Vector2 _lastCursorPos;
        private bool _moveActive;
        private int _oldAx = 0;
        private int _oldAy = 0;
        private int _oldEx;
        private int _oldEy;
        private float _scalePc = 1.0f;
        private bool _shiftActive;

        private int lastGameUpdateTime = 0;
        private int lastGameUpdateSpritesTime = 0;

        public Ui()
        {
            float percentScale =
                    (float)UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value / 100;

            int i = 0;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsMe)
                    continue;
                i++;
                var champ = new ChampInfos();

                champ.RecallInfo = new Packet.S2C.Teleport.Struct(hero.NetworkId, Packet.S2C.Teleport.Status.Unknown, Packet.S2C.Teleport.Type.Unknown, 0, 0);

                if (hero.IsEnemy)
                {
                    _enemies.Add(hero, champ);
                }
                if (!hero.IsEnemy)
                {
                    _allies.Add(hero, champ);
                }
            }
            //UpdateItems(true);
            //UpdateItems(false);
            //CalculateSizes(true);
            //CalculateSizes(false);
            ////new System.Threading.Thread(() =>
            ////{
            ////    SpecUtils.GetInfo();
            ////}).Start();
            Game.OnGameUpdate += Game_OnGameUpdateAsyncSprites;
            Game.OnGameUpdate += Game_OnGameUpdate;
            new Thread(LoadSpritesAsync).Start();
            LoadObjectsSync();
            
            //Game.OnGameProcessPacket += Game_OnGameProcessPacket; //TODO:Enable for Gold View currently bugged packet id never received
            Game.OnWndProc += Game_OnWndProc;
            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
        }
        
         ~Ui()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnGameUpdate -= Game_OnGameUpdateAsyncSprites;
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
            Obj_AI_Base.OnTeleport -= Obj_AI_Base_OnTeleport;
        }

         public bool IsActive()
         {
             return Tracker.Trackers.GetActive() && UiTracker.GetActive();
         }

         public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
         {
             Menu.MenuItemSettings tempSettings;
             UiTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_UI_MAIN"), "SAwarenessTrackersUi"));
             //UiTracker.MenuItems.Add(
             //    UiTracker.Menu.AddItem(new MenuItem("SAwarenessItemPanelActive", Language.GetString("TRACKERS_UI_ITEMPANEL")).SetValue(false)));
             UiTracker.MenuItems.Add(
                 UiTracker.Menu.AddItem(new MenuItem("SAwarenessUITrackerScale", Language.GetString("TRACKERS_UI_SCALE")).SetValue(new Slider(100, 100, 0))));
             tempSettings = UiTracker.AddMenuItemSettings(Language.GetString("TRACKERS_UI_ENEMY"), "SAwarenessUITrackerEnemyTracker");
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerEnemyTrackerXPos", Language.GetString("TRACKERS_UI_GLOBAL_POSITION_X")).SetValue(new Slider((int)_screen.X, Drawing.Width, 0))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerEnemyTrackerYPos", Language.GetString("TRACKERS_UI_GLOBAL_POSITION_Y")).SetValue(new Slider((int)_screen.Y, Drawing.Height, 0))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerEnemyTrackerMode", Language.GetString("GLOBAL_MODE")).SetValue(new StringList(new[]
                 {
                     Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDE"), 
                     Language.GetString("TRACKERS_UI_GLOBAL_MODE_UNIT"), 
                     Language.GetString("TRACKERS_UI_GLOBAL_MODE_BOTH")
                 }))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerEnemyTrackerSideDisplayMode", Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDE_DISPLAY")).SetValue(new StringList(new[]
                     {
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_DEFAULT"), 
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_SIMPLE"), 
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_LEAGUE")
                     }))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerEnemyTrackerHeadMode", Language.GetString("TRACKERS_UI_GLOBAL_MODE_UNIT_MODE")).SetValue(new StringList(new[]
                 {
                     Language.GetString("TRACKERS_UI_GLOBAL_MODE_HEAD_SMALL"), 
                     Language.GetString("TRACKERS_UI_GLOBAL_MODE_HEAD_BIG")
                 }))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerEnemyTrackerHeadDisplayMode", Language.GetString("TRACKERS_UI_GLOBAL_MODE_UNIT_DISPLAY")).SetValue(new StringList(new[]
                 {
                     Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_DEFAULT"), 
                     Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_SIMPLE")
                 }))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerEnemyTrackerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
             tempSettings = UiTracker.AddMenuItemSettings(Language.GetString("TRACKERS_UI_ALLY"), "SAwarenessUITrackerAllyTracker");
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerAllyTrackerXPos", Language.GetString("TRACKERS_UI_GLOBAL_POSITION_X")).SetValue(new Slider(110, Drawing.Width, 0))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerAllyTrackerYPos", Language.GetString("TRACKERS_UI_GLOBAL_POSITION_Y")).SetValue(new Slider((int)_screen.Y, Drawing.Height, 0))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerAllyTrackerMode", Language.GetString("GLOBAL_MODE")).SetValue(new StringList(new[]
                     {
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDE"), 
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_UNIT"), 
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_BOTH")
                     }))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerAllyTrackerSideDisplayMode", Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDE_DISPLAY")).SetValue(new StringList(new[]
                     {
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_DEFAULT"), 
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_SIMPLE"), 
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_LEAGUE")
                     }))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerAllyTrackerHeadMode", Language.GetString("TRACKERS_UI_GLOBAL_MODE_UNIT_MODE")).SetValue(new StringList(new[]
                     {
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_HEAD_SMALL"), 
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_HEAD_BIG")
                     }))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerAllyTrackerHeadDisplayMode", Language.GetString("TRACKERS_UI_GLOBAL_MODE_UNIT_DISPLAY")).SetValue(new StringList(new[]
                     {
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_DEFAULT"), 
                         Language.GetString("TRACKERS_UI_GLOBAL_MODE_SIDEHEAD_SIMPLE")
                     }))));
             tempSettings.MenuItems.Add(
                 tempSettings.Menu.AddItem(new MenuItem("SAwarenessUITrackerAllyTrackerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
             //Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerCameraMoveActive", "Camera move active").SetValue(false)));
             UiTracker.MenuItems.Add(
                 UiTracker.Menu.AddItem(new MenuItem("SAwarenessUITrackerPingActive", Language.GetString("TRACKERS_UI_PING")).SetValue(false)));
             UiTracker.MenuItems.Add(
                 UiTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersUiActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
             return UiTracker;
         }

         void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
         {
             Packet.S2C.Teleport.Struct decoded = Packet.S2C.Teleport.Decoded(sender, args);
             foreach (var enemy in _enemies)
             {
                 if (enemy.Value.RecallInfo.UnitNetworkId == decoded.UnitNetworkId)
                 {
                     enemy.Value.RecallInfo = decoded;
                 }
             }
         }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (!IsActive())
                return;
            HandleInput((WindowsMessages) args.Msg, Utils.GetCursorPos(), args.WParam);
        }

        private void HandleInput(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            HandleUiMove(message, cursorPos, key);
            HandleChampClick(message, cursorPos, key);
        }

        private void HandleUiMove(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_LBUTTONDOWN && message != WindowsMessages.WM_MOUSEMOVE &&
                message != WindowsMessages.WM_LBUTTONUP || (!_moveActive && message == WindowsMessages.WM_MOUSEMOVE)
                )
            {
                return;
            }
            if (message == WindowsMessages.WM_LBUTTONDOWN)
            {
                _lastCursorPos = cursorPos;
            }
            if (message == WindowsMessages.WM_LBUTTONUP)
            {
                _lastCursorPos = new Vector2();
                _moveActive = false;
                return;
            }
            var firstEnemyHero = new KeyValuePair<Obj_AI_Hero, ChampInfos>();
            foreach (var enemy in _enemies.Reverse())
            {
                firstEnemyHero = enemy;
                break;
            }
            if (firstEnemyHero.Key != null &&
                Common.IsInside(cursorPos, firstEnemyHero.Value.SpellPassive.SizeSideBar,
                    _hudSize.Width, _hudSize.Height))
            {
                _moveActive = true;
                if (message == WindowsMessages.WM_MOUSEMOVE)
                {
                    var curSliderX =
                        UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                            .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                            .GetValue<Slider>();
                    var curSliderY =
                        UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                            .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                            .GetValue<Slider>();
                    UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                        .SetValue(new Slider((int) (curSliderX.Value + cursorPos.X - _lastCursorPos.X),
                            curSliderX.MinValue, curSliderX.MaxValue));
                    UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                        .SetValue(new Slider((int) (curSliderY.Value + cursorPos.Y - _lastCursorPos.Y),
                            curSliderY.MinValue, curSliderY.MaxValue));
                    _lastCursorPos = cursorPos;
                }
            }
            var firstAllyHero = new KeyValuePair<Obj_AI_Hero, ChampInfos>();
            foreach (var ally in _allies.Reverse())
            {
                firstAllyHero = ally;
                break;
            }
            if (firstAllyHero.Key != null &&
                Common.IsInside(cursorPos, firstAllyHero.Value.SpellPassive.SizeSideBar,
                    _hudSize.Width, _hudSize.Height))
            {
                _moveActive = true;
                if (message == WindowsMessages.WM_MOUSEMOVE)
                {
                    var curSliderX =
                        UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                            .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                            .GetValue<Slider>();
                    var curSliderY =
                        UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                            .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                            .GetValue<Slider>();
                    UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                        .SetValue(new Slider((int) (curSliderX.Value + cursorPos.X - _lastCursorPos.X),
                            curSliderX.MinValue, curSliderX.MaxValue));
                    UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                        .SetValue(new Slider((int) (curSliderY.Value + cursorPos.Y - _lastCursorPos.Y),
                            curSliderY.MinValue, curSliderY.MaxValue));
                    _lastCursorPos = cursorPos;
                }
            }
        }

        private void HandleChampClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if ((message != WindowsMessages.WM_KEYDOWN && key == 16) && message != WindowsMessages.WM_LBUTTONDOWN &&
                (message != WindowsMessages.WM_KEYUP && key == 16) ||
                (!_shiftActive && message == WindowsMessages.WM_LBUTTONDOWN))
            {
                return;
            }
            if (message == WindowsMessages.WM_KEYDOWN && key == 16)
            {
                _shiftActive = true;
            }
            if (message == WindowsMessages.WM_KEYUP && key == 16)
            {
                _shiftActive = false;
            }
            if (message == WindowsMessages.WM_LBUTTONDOWN)
            {
                foreach (var enemy in _enemies.Reverse())
                {
                    if (Common.IsInside(cursorPos, enemy.Value.Champ.SizeSideBar, _champSize.Width,
                        _champSize.Height))
                    {
                        //TODO: Add Camera move
                        if (UiTracker.GetMenuItem("SAwarenessUITrackerPingActive").GetValue<bool>())
                        {
                            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(enemy.Key.ServerPosition.X,
                                enemy.Key.ServerPosition.Y, 0, 0, Packet.PingType.Normal)).Process();
                        }
                    }
                }
            }
        }

        public async static Task Init()
        {
            if (
                UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                    .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                    .GetValue<Slider>()
                    .Value == 0)
            {
                UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                    .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                    .SetValue(new Slider((int) _screen.X, Drawing.Width, 0));
            }
            if (
                UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                    .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                    .GetValue<Slider>()
                    .Value == 0)
            {
                UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                    .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                    .SetValue(new Slider((int) _screen.Y, Drawing.Height, 0));
            }
            if (
                UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                    .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                    .GetValue<Slider>()
                    .Value == 0)
            {
                UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                    .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                    .SetValue(new Slider((int) 110, Drawing.Width, 0));
            }
            if (
                UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                    .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                    .GetValue<Slider>()
                    .Value == 0)
            {
                UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                    .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                    .SetValue(new Slider((int) _screen.Y, Drawing.Height, 0));
            }

            float percentScale =
                    (float)UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value / 100;

            int i = 0;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if(hero.IsMe)
                    continue;
                i++;
                var champ = new ChampInfos();

                Console.WriteLine((i) + ". Loading Sprites for " + hero.ChampionName);
                Task<ChampInfos> champInfos = CreateSideHud(hero, champ, percentScale);
                champ = await champInfos;
                Console.WriteLine("Loaded CreateSideHud Sprites");
                champInfos = CreateOverHeadHud(hero, champ, percentScale);
                champ = await champInfos;
                Console.WriteLine("Loaded CreateOverHeadHud Sprites");
                champ.RecallInfo = new Packet.S2C.Teleport.Struct(hero.NetworkId, Packet.S2C.Teleport.Status.Unknown, Packet.S2C.Teleport.Type.Unknown, 0, 0);

                if (hero.IsEnemy)
                {
                    _enemies.Add(hero, champ);
                }
                if (!hero.IsEnemy)
                {
                    _allies.Add(hero, champ);
                }
                Console.WriteLine("Created all Sprites");
            }      
        }

        void LoadObjectsSync()
        {
            foreach (var enemy in _enemies)
            {
                LoadObjectsSyncSideHud(enemy);
                LoadObjectsSyncOverHeadHud(enemy);
            }
            foreach (var ally in _allies)
            {
                LoadObjectsSyncSideHud(ally);
                LoadObjectsSyncOverHeadHud(ally);
            }
        }

        void LoadObjectsSyncOverHeadHud(KeyValuePair<Obj_AI_Hero, ChampInfos> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            ChampInfos champ = enemy.Value;
            champ.SpellQ.Rectangle[1] = new Render.Rectangle(champ.SpellQ.SizeHpBar.Width, champ.SpellQ.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellQ.Rectangle[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellQ.SizeHpBar.Width, champ.SpellQ.SizeHpBar.Height);
            };
            champ.SpellQ.Rectangle[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellQ.Rectangle[1].Add(1);

            champ.SpellW.Rectangle[1] = new Render.Rectangle(champ.SpellW.SizeHpBar.Width, champ.SpellW.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellW.Rectangle[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellW.SizeHpBar.Width, champ.SpellW.SizeHpBar.Height);
            };
            champ.SpellW.Rectangle[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellW.Rectangle[1].Add(1);

            champ.SpellE.Rectangle[1] = new Render.Rectangle(champ.SpellE.SizeHpBar.Width, champ.SpellE.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellE.Rectangle[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellE.SizeHpBar.Width, champ.SpellE.SizeHpBar.Height);
            };
            champ.SpellE.Rectangle[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellE.Rectangle[1].Add(1);

            champ.SpellR.Rectangle[1] = new Render.Rectangle(champ.SpellR.SizeHpBar.Width, champ.SpellR.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellR.Rectangle[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.SizeHpBar.Width, champ.SpellR.SizeHpBar.Height);
            };
            champ.SpellR.Rectangle[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellR.Rectangle[1].Add(1);

            ///////

            champ.SpellQ.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellQ.Text[1].TextUpdate = delegate
            {
                return champ.SpellQ.Value.ToString();
            };
            champ.SpellQ.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellQ.CoordsHpBar.Width, champ.SpellQ.CoordsHpBar.Height);
            };
            champ.SpellQ.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                    champ.SpellQ.Value > 0.0f && enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellQ.Text[1].OutLined = true;
            champ.SpellQ.Text[1].Centered = true;
            champ.SpellQ.Text[1].Add(4);

            champ.SpellW.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellW.Text[1].TextUpdate = delegate
            {
                return champ.SpellW.Value.ToString();
            };
            champ.SpellW.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellW.CoordsHpBar.Width, champ.SpellW.CoordsHpBar.Height);
            };
            champ.SpellW.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellW.Value > 0.0f && enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellW.Text[1].OutLined = true;
            champ.SpellW.Text[1].Centered = true;
            champ.SpellW.Text[1].Add(4);

            champ.SpellE.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellE.Text[1].TextUpdate = delegate
            {
                return champ.SpellE.Value.ToString();
            };
            champ.SpellE.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellE.CoordsHpBar.Width, champ.SpellE.CoordsHpBar.Height);
            };
            champ.SpellE.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellE.Value > 0.0f && enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellE.Text[1].OutLined = true;
            champ.SpellE.Text[1].Centered = true;
            champ.SpellE.Text[1].Add(4);

            champ.SpellR.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellR.Text[1].TextUpdate = delegate
            {
                return champ.SpellR.Value.ToString();
            };
            champ.SpellR.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.CoordsHpBar.Width, champ.SpellR.CoordsHpBar.Height);
            };
            champ.SpellR.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellR.Value > 0.0f && enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellR.Text[1].OutLined = true;
            champ.SpellR.Text[1].Centered = true;
            champ.SpellR.Text[1].Add(4);

            champ.SpellSum1.Text[1] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum1.Text[1].TextUpdate = delegate
            {
                return champ.SpellSum1.Value.ToString();
            };
            champ.SpellSum1.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum1.CoordsHpBar.Width, champ.SpellSum1.CoordsHpBar.Height);
            };
            champ.SpellSum1.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum1.Value > 0.0f && enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellSum1.Text[1].OutLined = true;
            champ.SpellSum1.Text[1].Centered = true;
            champ.SpellSum1.Text[1].Add(4);

            champ.SpellSum2.Text[1] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum2.Text[1].TextUpdate = delegate
            {
                return champ.SpellSum2.Value.ToString();
            };
            champ.SpellSum2.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum2.CoordsHpBar.Width, champ.SpellSum2.CoordsHpBar.Height);
            };
            champ.SpellSum2.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum2.Value > 0.0f && enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.SpellSum2.Text[1].OutLined = true;
            champ.SpellSum2.Text[1].Centered = true;
            champ.SpellSum2.Text[1].Add(4);

            /////

            champ.Exp[0].Line[1] = new Render.Line(new Vector2(0, 0), new Vector2(0, 0), 2, SharpDX.Color.Yellow);
            champ.Exp[0].Line[1].StartPositionUpdate = delegate
            {
                return new Vector2(champ.Exp[0].SizeHpBar.Width, champ.Exp[0].SizeHpBar.Height);
            };
            champ.Exp[0].Line[1].EndPositionUpdate = delegate
            {
                return new Vector2(champ.Exp[0].SizeHpBar.Width + _lvlIconSize.Width * CalcExpBar(hero, 0), champ.Exp[0].SizeHpBar.Height);
            };
            champ.Exp[0].Line[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 &&
                        enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.Exp[0].Line[1].Add(3);

            champ.Exp[1].Line[1] = new Render.Line(new Vector2(0, 0), new Vector2(0, 0), 2, SharpDX.Color.Yellow);
            champ.Exp[1].Line[1].StartPositionUpdate = delegate
            {
                return new Vector2(champ.Exp[1].SizeHpBar.Width, champ.Exp[1].SizeHpBar.Height );
            };
            champ.Exp[1].Line[1].EndPositionUpdate = delegate
            {
                return new Vector2(champ.Exp[1].SizeHpBar.Width, champ.Exp[1].SizeHpBar.Height + _lvlIconSize.Height * CalcExpBar(hero, 1));
            };
            champ.Exp[1].Line[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 &&
                        enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.Exp[1].Line[1].Add(3);

            champ.Exp[2].Line[1] = new Render.Line(new Vector2(0, 0), new Vector2(0, 0), 2, SharpDX.Color.Yellow);
            champ.Exp[2].Line[1].StartPositionUpdate = delegate
            {
                return new Vector2(champ.Exp[2].SizeHpBar.Width, champ.Exp[2].SizeHpBar.Height);
            };
            champ.Exp[2].Line[1].EndPositionUpdate = delegate
            {
                return new Vector2(champ.Exp[2].SizeHpBar.Width - _lvlIconSize.Width * CalcExpBar(hero, 2), champ.Exp[2].SizeHpBar.Height);
            };
            champ.Exp[2].Line[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 &&
                        enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.Exp[2].Line[1].Add(3);

            champ.Exp[3].Line[1] = new Render.Line(new Vector2(0, 0), new Vector2(0, 0), 2, SharpDX.Color.Yellow);
            champ.Exp[3].Line[1].StartPositionUpdate = delegate
            {
                return new Vector2(champ.Exp[3].SizeHpBar.Width, champ.Exp[3].SizeHpBar.Height);
            };
            champ.Exp[3].Line[1].EndPositionUpdate = delegate
            {
                return new Vector2(champ.Exp[3].SizeHpBar.Width, champ.Exp[3].SizeHpBar.Height - _lvlIconSize.Width * CalcExpBar(hero, 3));
            };
            champ.Exp[3].Line[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 &&
                        enemy.Key.IsVisible && !enemy.Key.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
            };
            champ.Exp[3].Line[1].Add(3);
        }

        void LoadObjectsSyncSideHud(KeyValuePair<Obj_AI_Hero, ChampInfos> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            ChampInfos champ = enemy.Value;

            champ.SpellR.Rectangle[0] = new Render.Rectangle(champ.SpellR.SizeSideBar.Width,
                champ.SpellR.SizeSideBar.Height,
                _sumSize.Width, _sumSize.Height, SharpDX.Color.Red);
            champ.SpellR.Rectangle[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.SizeSideBar.Width, champ.SpellR.SizeSideBar.Height);
            };
            champ.SpellR.Rectangle[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 1 && champ.Draw(true);
            };
            champ.SpellR.Rectangle[0].Add(1);

            ///////

            champ.HealthBar.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.HealthBar.Text[0].TextUpdate = delegate
            {
                return champ.SHealth ?? "";
            };
            champ.HealthBar.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.HealthBar.CoordsSideBar.Width, champ.HealthBar.CoordsSideBar.Height);
            };
            champ.HealthBar.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy) && champ.Draw(true);
            };
            champ.HealthBar.Text[0].OutLined = true;
            champ.HealthBar.Text[0].Centered = true;
            champ.HealthBar.Text[0].Add(4);

            champ.ManaBar.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.ManaBar.Text[0].TextUpdate = delegate
            {
                return champ.SMana ?? "";
            };
            champ.ManaBar.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            };
            champ.ManaBar.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy) && champ.Draw(true);
            };
            champ.ManaBar.Text[0].OutLined = true;
            champ.ManaBar.Text[0].Centered = true;
            champ.ManaBar.Text[0].Add(4);

            champ.SpellQ.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellQ.Text[0].TextUpdate = delegate
            {
                return champ.SpellQ.Value.ToString();
            };
            champ.SpellQ.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellQ.CoordsSideBar.Width, champ.SpellQ.CoordsSideBar.Height);
            };
            champ.SpellQ.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       champ.SpellQ.Value > 0.0f && GetUiActive(hero.IsEnemy) &&
                       GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
            };
            champ.SpellQ.Text[0].OutLined = true;
            champ.SpellQ.Text[0].Centered = true;
            champ.SpellQ.Text[0].Add(4);

            champ.SpellW.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellW.Text[0].TextUpdate = delegate
            {
                return champ.SpellW.Value.ToString();
            };
            champ.SpellW.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellW.CoordsSideBar.Width, champ.SpellW.CoordsSideBar.Height);
            };
            champ.SpellW.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       champ.SpellW.Value > 0.0f && GetUiActive(hero.IsEnemy) &&
                       GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
            };
            champ.SpellW.Text[0].OutLined = true;
            champ.SpellW.Text[0].Centered = true;
            champ.SpellW.Text[0].Add(4);

            champ.SpellE.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellE.Text[0].TextUpdate = delegate
            {
                return champ.SpellE.Value.ToString();
            };
            champ.SpellE.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellE.CoordsSideBar.Width, champ.SpellE.CoordsSideBar.Height);
            };
            champ.SpellE.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       champ.SpellE.Value > 0.0f && GetUiActive(hero.IsEnemy) &&
                       GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
            };
            champ.SpellE.Text[0].OutLined = true;
            champ.SpellE.Text[0].Centered = true;
            champ.SpellE.Text[0].Add(4);

            champ.SpellR.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellR.Text[0].TextUpdate = delegate
            {
                return champ.SpellR.Value.ToString();
            };
            champ.SpellR.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.CoordsSideBar.Width, champ.SpellR.CoordsSideBar.Height);
            };
            champ.SpellR.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       champ.SpellR.Value > 0.0f && GetUiActive(hero.IsEnemy) && champ.Draw(true);
            };
            champ.SpellR.Text[0].OutLined = true;
            champ.SpellR.Text[0].Centered = true;
            champ.SpellR.Text[0].Add(4);

            champ.Champ.Text[0] = new Render.Text(0, 0, "", 30, SharpDX.Color.Orange);
            champ.Champ.Text[0].TextUpdate = delegate
            {
                if (champ.DeathTimeDisplay > 0.0f && hero.IsDead)
                    return champ.DeathTimeDisplay.ToString();
                else if (champ.InvisibleTime > 0.0f && !hero.IsVisible)
                    return champ.InvisibleTime.ToString();
                return "";
            };
            champ.Champ.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.Champ.CoordsSideBar.Width, champ.Champ.CoordsSideBar.Height);
            };
            champ.Champ.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       ((champ.DeathTimeDisplay > 0.0f && hero.IsDead) ||
                        (champ.InvisibleTime > 0.0f && !hero.IsVisible)) &&
                       GetUiActive(hero.IsEnemy) && champ.Draw(true);
            };
            champ.Champ.Text[0].OutLined = true;
            champ.Champ.Text[0].Centered = true;
            champ.Champ.Text[0].Add(4);

            champ.SpellSum1.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum1.Text[0].TextUpdate = delegate
            {
                return champ.SpellSum1.Value.ToString();
            };
            champ.SpellSum1.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum1.CoordsSideBar.Width, champ.SpellSum1.CoordsSideBar.Height);
            };
            champ.SpellSum1.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       champ.SpellSum1.Value > 0.0f && GetUiActive(hero.IsEnemy) && champ.Draw(true);
            };
            champ.SpellSum1.Text[0].OutLined = true;
            champ.SpellSum1.Text[0].Centered = true;
            champ.SpellSum1.Text[0].Add(4);

            champ.SpellSum2.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum2.Text[0].TextUpdate = delegate
            {
                return champ.SpellSum2.Value.ToString();
            };
            champ.SpellSum2.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum2.CoordsSideBar.Width, champ.SpellSum2.CoordsSideBar.Height);
            };
            champ.SpellSum2.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       champ.SpellSum2.Value > 0.0f && GetUiActive(hero.IsEnemy) && champ.Draw(true);
            };
            champ.SpellSum2.Text[0].OutLined = true;
            champ.SpellSum2.Text[0].Centered = true;
            champ.SpellSum2.Text[0].Add(4);

            foreach (var item in champ.Item)
            {
                if (item == null)
                    continue;
                item.Text[0] = new Render.Text(0, 0, "", 12, SharpDX.Color.Orange);
                item.Text[0].TextUpdate = delegate
                {
                    return item.Value.ToString();
                };
                item.Text[0].PositionUpdate = delegate
                {
                    return new Vector2(item.CoordsSideBar.Width, item.CoordsSideBar.Height);
                };
                item.Text[0].VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                           GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                           item.Value > 0.0f && UiTracker.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>() &&
                           GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
                };
                item.Text[0].OutLined = true;
                item.Text[0].Centered = true;
                item.Text[0].Add(4);
            }

            champ.Level.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.Level.Text[0].TextUpdate = delegate
            {
                return champ.Level.Value.ToString();
            };
            champ.Level.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.Level.CoordsSideBar.Width, champ.Level.CoordsSideBar.Height);
            };
            champ.Level.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
            };
            champ.Level.Text[0].OutLined = true;
            champ.Level.Text[0].Centered = true;
            champ.Level.Text[0].Add(4);

            champ.Cs.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.Cs.Text[0].TextUpdate = delegate
            {
                return champ.Cs.Value.ToString();
            };
            champ.Cs.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.Cs.CoordsSideBar.Width, champ.Cs.CoordsSideBar.Height);
            };
            champ.Cs.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
            };
            champ.Cs.Text[0].OutLined = true;
            champ.Cs.Text[0].Centered = true;
            champ.Cs.Text[0].Add(4);

            champ.RecallBar.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.RecallBar.Text[0].TextUpdate = delegate
            {
                if (champ.RecallInfo.Start != 0)
                {
                    float time = Environment.TickCount + champ.RecallInfo.Duration - champ.RecallInfo.Start;
                    if (time > 0.0f &&
                        (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Start))
                    {
                        return "Porting";
                    }
                    else if (time < 30.0f &&
                             (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Finish))
                    {
                        return "Ported";
                    }
                    else if (time < 30.0f &&
                             (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Abort))
                    {
                        return "Canceled";
                    }
                }
                return "";
            };
            champ.RecallBar.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.RecallBar.CoordsSideBar.Width, champ.RecallBar.CoordsSideBar.Height);
            };
            champ.RecallBar.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                       GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                       GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
            };
            champ.RecallBar.Text[0].OutLined = true;
            champ.RecallBar.Text[0].Centered = true;
            champ.RecallBar.Text[0].Add(4);
        }

        void LoadSpritesAsync()
        {
            foreach (var enemy in _enemies)
            {
                LoadSpritesAsyncSideHud(enemy);
                LoadSpritesAsyncOverHeadHud(enemy);
            }
            foreach (var ally in _allies)
            {
                LoadSpritesAsyncSideHud(ally);
                LoadSpritesAsyncOverHeadHud(ally);
            }
        }

        private void LoadSpritesAsyncOverHeadHud(KeyValuePair<Obj_AI_Hero, ChampInfos> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            SpriteHelper.DownloadImageRiot(hero.ChampionName, SpriteHelper.DownloadType.Champion, @"UI\OverHeadHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.Q).Name, SpriteHelper.DownloadType.Spell, @"UI\OverHeadHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.W).Name, SpriteHelper.DownloadType.Spell, @"UI\OverHeadHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.E).Name, SpriteHelper.DownloadType.Spell, @"UI\OverHeadHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.R).Name, SpriteHelper.DownloadType.Spell, @"UI\OverHeadHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.Summoner1).Name, SpriteHelper.DownloadType.Summoner, @"UI\OverHeadHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.Summoner2).Name, SpriteHelper.DownloadType.Summoner, @"UI\OverHeadHud");
        }

        void LoadSpritesAsyncSideHud(KeyValuePair<Obj_AI_Hero, ChampInfos> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            SpriteHelper.DownloadImageRiot(hero.ChampionName, SpriteHelper.DownloadType.Champion, @"UI\SideHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.Q).Name, SpriteHelper.DownloadType.Spell, @"UI\SideHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.W).Name, SpriteHelper.DownloadType.Spell, @"UI\SideHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.E).Name, SpriteHelper.DownloadType.Spell, @"UI\SideHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.R).Name, SpriteHelper.DownloadType.Spell, @"UI\SideHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.Summoner1).Name, SpriteHelper.DownloadType.Summoner, @"UI\SideHud");
            SpriteHelper.DownloadImageRiot(hero.Spellbook.GetSpell(SpellSlot.Summoner2).Name, SpriteHelper.DownloadType.Summoner, @"UI\SideHud");
        }

        void Game_OnGameUpdateAsyncSprites(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateSpritesTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateSpritesTime = Environment.TickCount;
            foreach (var enemy in _enemies)
            {
                Game_OnGameUpdateAsyncSpritesSideHud(enemy);
                Game_OnGameUpdateAsyncSpritesOverHeadHud(enemy);
            }
            foreach (var ally in _allies)
            {
                Game_OnGameUpdateAsyncSpritesSideHud(ally);
                Game_OnGameUpdateAsyncSpritesOverHeadHud(ally);
            }
        }

        void Game_OnGameUpdateAsyncSpritesSideHud(KeyValuePair<Obj_AI_Hero, ChampInfos> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            ChampInfos champ = enemy.Value;
            if (champ.Champ.Sprite[0] == null || !champ.Champ.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.ChampionName, ref champ.Champ.Sprite[0], @"UI\SideHud");
            }
            if (champ.Champ.Sprite[0] != null && champ.Champ.Sprite[0].DownloadFinished && !champ.Champ.Sprite[0].LoadingFinished)
            {
                champ.Champ.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.Champ.SizeSideBar.Width, champ.Champ.SizeSideBar.Height);
                };
                champ.Champ.Sprite[0].Sprite.VisibleCondition = delegate
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy) && champ.Draw(true);
                };
                champ.Champ.Sprite[0].Sprite.Add(0);
                champ.Champ.Sprite[0].LoadingFinished = true;
            }

            if (champ.SpellQ.Sprite[0] == null || !champ.SpellQ.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.Q).Name, ref champ.SpellQ.Sprite[0], @"UI\SideHud");
            }
            if (champ.SpellQ.Sprite[0] != null && champ.SpellQ.Sprite[0].DownloadFinished && !champ.SpellQ.Sprite[0].LoadingFinished)
            {
                champ.SpellQ.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellQ.SizeSideBar.Width, champ.SpellQ.SizeSideBar.Height);
                };
                champ.SpellQ.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
                };
                champ.SpellQ.Sprite[0].Sprite.Add(0);
                champ.SpellQ.Sprite[0].LoadingFinished = true;
            }

            if (champ.SpellW.Sprite[0] == null || !champ.SpellW.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.W).Name, ref champ.SpellW.Sprite[0], @"UI\SideHud");
            }
            if (champ.SpellW.Sprite[0] != null && champ.SpellW.Sprite[0].DownloadFinished && !champ.SpellW.Sprite[0].LoadingFinished)
            {
                champ.SpellW.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellW.SizeSideBar.Width, champ.SpellW.SizeSideBar.Height);
                };
                champ.SpellW.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
                };
                champ.SpellW.Sprite[0].Sprite.Add(0);
                champ.SpellW.Sprite[0].LoadingFinished = true;
            }

            if (champ.SpellE.Sprite[0] == null || !champ.SpellE.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.E).Name, ref champ.SpellE.Sprite[0], @"UI\SideHud");
            }
            if (champ.SpellE.Sprite[0] != null && champ.SpellE.Sprite[0].DownloadFinished && !champ.SpellE.Sprite[0].LoadingFinished)
            {
                champ.SpellE.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellE.SizeSideBar.Width, champ.SpellE.SizeSideBar.Height);
                };
                champ.SpellE.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
                };
                champ.SpellE.Sprite[0].Sprite.Add(0);
                champ.SpellE.Sprite[0].LoadingFinished = true;
            }

            if (champ.SpellR.Sprite[0] == null || !champ.SpellR.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.R).Name, ref champ.SpellR.Sprite[0], @"UI\SideHud");
            }
            if (champ.SpellR.Sprite[0] != null && champ.SpellR.Sprite[0].DownloadFinished && !champ.SpellR.Sprite[0].LoadingFinished)
            {
                champ.SpellR.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellR.SizeSideBar.Width, champ.SpellR.SizeSideBar.Height);
                };
                champ.SpellR.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
                };
                champ.SpellR.Sprite[0].Sprite.Add(0);
                champ.SpellR.Sprite[0].LoadingFinished = true;
            }

            if (champ.SpellSum1.Sprite[0] == null || !champ.SpellSum1.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.Summoner1).Name, ref champ.SpellSum1.Sprite[0], @"UI\SideHud");
            }
            if (champ.SpellSum1.Sprite[0] != null && champ.SpellSum1.Sprite[0].DownloadFinished && !champ.SpellSum1.Sprite[0].LoadingFinished)
            {
                champ.SpellSum1.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum1.SizeSideBar.Width, champ.SpellSum1.SizeSideBar.Height);
                };
                champ.SpellSum1.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy) && champ.Draw(true);
                };
                champ.SpellSum1.Sprite[0].Sprite.Add(0);
                champ.SpellSum1.Sprite[0].LoadingFinished = true;
            }

            if (champ.SpellSum2.Sprite[0] == null || !champ.SpellSum2.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.Summoner2).Name, ref champ.SpellSum2.Sprite[0], @"UI\SideHud");
            }
            if (champ.SpellSum2.Sprite[0] != null && champ.SpellSum2.Sprite[0].DownloadFinished && !champ.SpellSum2.Sprite[0].LoadingFinished)
            {
                champ.SpellSum2.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum2.SizeSideBar.Width, champ.SpellSum2.SizeSideBar.Height);
                };
                champ.SpellSum2.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy) && champ.Draw(true);
                };
                champ.SpellSum2.Sprite[0].Sprite.Add(0);
                champ.SpellSum2.Sprite[0].LoadingFinished = true;
            }

            if (champ.BackBar.Sprite[0] == null || !champ.BackBar.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture("BarBackground", ref champ.BackBar.Sprite[0], SpriteHelper.TextureType.Default);
            }
            if (champ.BackBar.Sprite[0] != null && champ.BackBar.Sprite[0].DownloadFinished && !champ.BackBar.Sprite[0].LoadingFinished)
            {
                champ.BackBar.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.BackBar.SizeSideBar.Width, champ.BackBar.SizeSideBar.Height);
                };
                champ.BackBar.Sprite[0].Sprite.VisibleCondition = delegate
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                           GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy) && champ.Draw(true);
                };
                champ.BackBar.Sprite[0].Sprite.Add(0);
                champ.BackBar.Sprite[0].LoadingFinished = true;
            }

            if (champ.HealthBar.Sprite[0] == null || !champ.HealthBar.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture("HealthBar", ref champ.HealthBar.Sprite[0], SpriteHelper.TextureType.Default);
            }
            if (champ.HealthBar.Sprite[0] != null && champ.HealthBar.Sprite[0].DownloadFinished && !champ.HealthBar.Sprite[0].LoadingFinished)
            {
                champ.HealthBar.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.HealthBar.SizeSideBar.Width, champ.HealthBar.SizeSideBar.Height);
                };
                champ.HealthBar.Sprite[0].Sprite.VisibleCondition = delegate
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                           GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy) && champ.Draw(true);
                };
                champ.HealthBar.Sprite[0].Sprite.Add(1);
                champ.HealthBar.Sprite[0].LoadingFinished = true;
            }

            if (champ.ManaBar.Sprite[0] == null || !champ.ManaBar.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture("ManaBar", ref champ.ManaBar.Sprite[0], SpriteHelper.TextureType.Default);
            }
            if (champ.ManaBar.Sprite[0] != null && champ.ManaBar.Sprite[0].DownloadFinished && !champ.ManaBar.Sprite[0].LoadingFinished)
            {
                champ.ManaBar.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.ManaBar.SizeSideBar.Width, champ.ManaBar.SizeSideBar.Height);
                };
                champ.ManaBar.Sprite[0].Sprite.VisibleCondition = delegate
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                           GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy) && champ.Draw(true);
                };
                champ.ManaBar.Sprite[0].Sprite.Add(1);
                champ.ManaBar.Sprite[0].LoadingFinished = true;
            }

            if (champ.RecallBar.Sprite[0] == null || !champ.RecallBar.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture("RecallBar", ref champ.RecallBar.Sprite[0], SpriteHelper.TextureType.Default);
            }
            if (champ.RecallBar.Sprite[0] != null && champ.RecallBar.Sprite[0].DownloadFinished && !champ.RecallBar.Sprite[0].LoadingFinished)
            {
                champ.RecallBar.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.RecallBar.SizeSideBar.Width, champ.RecallBar.SizeSideBar.Height);
                };
                champ.RecallBar.Sprite[0].Sprite.VisibleCondition = delegate
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                           GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                           GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
                };
                champ.RecallBar.Sprite[0].Sprite.Color = new ColorBGRA(Color3.White, 0.55f);
                champ.RecallBar.Sprite[0].Sprite.Add(1);
                champ.RecallBar.Sprite[0].LoadingFinished = true;
            }

            if (champ.GoldCsLvlBar.Sprite[0] == null || !champ.GoldCsLvlBar.Sprite[0].DownloadFinished)
            {
                SpriteHelper.LoadTexture("GoldCsLvlBar", ref champ.GoldCsLvlBar.Sprite[0], SpriteHelper.TextureType.Default);
            }
            if (champ.GoldCsLvlBar.Sprite[0] != null && champ.GoldCsLvlBar.Sprite[0].DownloadFinished && !champ.GoldCsLvlBar.Sprite[0].LoadingFinished)
            {
                champ.GoldCsLvlBar.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.Champ.SizeSideBar.Width, champ.Champ.SizeSideBar.Height);
                };
                champ.GoldCsLvlBar.Sprite[0].Sprite.VisibleCondition = delegate
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                           GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                           GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0 && champ.Draw(true);
                };
                champ.GoldCsLvlBar.Sprite[0].Sprite.Color = new ColorBGRA(Color3.White, 0.55f);
                champ.GoldCsLvlBar.Sprite[0].Sprite.Add(1);
                champ.GoldCsLvlBar.Sprite[0].LoadingFinished = true;
            }           
        }

        void Game_OnGameUpdateAsyncSpritesOverHeadHud(KeyValuePair<Obj_AI_Hero, ChampInfos> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            ChampInfos champ = enemy.Value;

            if (champ.SpellSum1.Sprite[1] == null || !champ.SpellSum1.Sprite[1].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.Summoner1).Name, ref champ.SpellSum1.Sprite[1], @"UI\OverHeadHud");
            }
            if (champ.SpellSum1.Sprite[1] != null && champ.SpellSum1.Sprite[1].DownloadFinished && !champ.SpellSum1.Sprite[1].LoadingFinished)
            {
                champ.SpellSum1.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum1.SizeHpBar.Width, champ.SpellSum1.SizeHpBar.Height);
                };
                champ.SpellSum1.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 0 && hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
                };
                champ.SpellSum1.Sprite[1].Sprite.Add();
                champ.SpellSum1.Sprite[1].LoadingFinished = true;
            }

            if (champ.SpellSum2.Sprite[1] == null || !champ.SpellSum2.Sprite[1].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.Summoner2).Name, ref champ.SpellSum2.Sprite[1], @"UI\OverHeadHud");
            }
            if (champ.SpellSum2.Sprite[1] != null && champ.SpellSum2.Sprite[1].DownloadFinished && !champ.SpellSum2.Sprite[1].LoadingFinished)
            {
                champ.SpellSum2.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum2.SizeHpBar.Width, champ.SpellSum2.SizeHpBar.Height);
                };
                champ.SpellSum2.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 0 && hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
                };
                champ.SpellSum2.Sprite[1].Sprite.Add();
                champ.SpellSum2.Sprite[1].LoadingFinished = true;
            }

            if (champ.SpellQ.Sprite[1] == null || !champ.SpellQ.Sprite[1].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.Q).Name, ref champ.SpellQ.Sprite[1], @"UI\OverHeadHud");
            }
            if (champ.SpellQ.Sprite[1] != null && champ.SpellQ.Sprite[1].DownloadFinished && !champ.SpellQ.Sprite[1].LoadingFinished)
            {
                champ.SpellQ.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellQ.SizeHpBar.Width, champ.SpellQ.SizeHpBar.Height);
                };
                champ.SpellQ.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
                };
                champ.SpellQ.Sprite[1].Sprite.Add();
                champ.SpellQ.Sprite[1].LoadingFinished = true;
            }

            if (champ.SpellW.Sprite[1] == null || !champ.SpellW.Sprite[1].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.W).Name, ref champ.SpellW.Sprite[1], @"UI\OverHeadHud");
            }
            if (champ.SpellW.Sprite[1] != null && champ.SpellW.Sprite[1].DownloadFinished && !champ.SpellW.Sprite[1].LoadingFinished)
            {
                champ.SpellW.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellW.SizeHpBar.Width, champ.SpellW.SizeHpBar.Height);
                };
                champ.SpellW.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
                };
                champ.SpellW.Sprite[1].Sprite.Add();
                champ.SpellW.Sprite[1].LoadingFinished = true;
            }

            if (champ.SpellE.Sprite[1] == null || !champ.SpellE.Sprite[1].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.E).Name, ref champ.SpellE.Sprite[1], @"UI\OverHeadHud");
            }
            if (champ.SpellE.Sprite[1] != null && champ.SpellE.Sprite[1].DownloadFinished && !champ.SpellE.Sprite[1].LoadingFinished)
            {
                champ.SpellE.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellE.SizeHpBar.Width, champ.SpellE.SizeHpBar.Height);
                };
                champ.SpellE.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
                };
                champ.SpellE.Sprite[1].Sprite.Add();
                champ.SpellE.Sprite[1].LoadingFinished = true;
            }

            if (champ.SpellR.Sprite[1] == null || !champ.SpellR.Sprite[1].DownloadFinished)
            {
                SpriteHelper.LoadTexture(hero.Spellbook.GetSpell(SpellSlot.R).Name, ref champ.SpellR.Sprite[1], @"UI\OverHeadHud");
            }
            if (champ.SpellR.Sprite[1] != null && champ.SpellR.Sprite[1].DownloadFinished && !champ.SpellR.Sprite[1].LoadingFinished)
            {
                champ.SpellR.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellR.SizeHpBar.Width, champ.SpellR.SizeHpBar.Height);
                };
                champ.SpellR.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy) && champ.Draw(false);
                };
                champ.SpellR.Sprite[1].Sprite.Add();
                champ.SpellR.Sprite[1].LoadingFinished = true;
            }
        }

        private async static Task<ChampInfos> CreateSideHud(Obj_AI_Hero hero, ChampInfos champ, float percentScale)
        {
            float percentHealth = CalcHpBar(hero);
            float percentMana = CalcManaBar(hero);
            //SpriteHelper.LoadTexture("ItemSlotEmpty", ref _overlayEmptyItem, SpriteHelper.TextureType.Default);
            Console.WriteLine(hero.ChampionName);
            Console.WriteLine("Champ");
            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            taskInfo = SpriteHelper.LoadTextureAsync(hero.ChampionName, champ.Champ.Sprite[0], SpriteHelper.DownloadType.Champion);
            champ.Champ.Sprite[0] = await taskInfo;
            if (!champ.Champ.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateChampImage(hero, champ.Champ.SizeSideBar, champ.Champ.Sprite[0], UpdateMethod.Side));
            }
            else
            {
                champ.Champ.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.Champ.SizeSideBar.Width, champ.Champ.SizeSideBar.Height);
                };
                champ.Champ.Sprite[0].Sprite.VisibleCondition = delegate
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy);
                };
                champ.Champ.Sprite[0].Sprite.Add(0);
            }

            //SpriteHelper.LoadTexture(s1[0].Name + ".dds", "PASSIVE/", loc + "PASSIVE\\" + s1[0].Name + ".dds", ref champ.Passive.Texture);
            Console.WriteLine("SpellQ");
            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.Q).Name, champ.SpellQ.Sprite[0], SpriteHelper.DownloadType.Spell);
            champ.SpellQ.Sprite[0] = await taskInfo;
            if (!champ.SpellQ.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellQ.SizeSideBar, champ.SpellQ.Sprite[0], SpellSlot.Q, UpdateMethod.Side));
            }
            else
            {
                champ.SpellQ.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellQ.SizeSideBar.Width, champ.SpellQ.SizeSideBar.Height);
                };
                champ.SpellQ.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && 
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
                };
                champ.SpellQ.Sprite[0].Sprite.Add(0);
            }

            Console.WriteLine("SpellW");
            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.W).Name, champ.SpellW.Sprite[0], SpriteHelper.DownloadType.Spell);
            champ.SpellW.Sprite[0] = await taskInfo;
            if (!champ.SpellW.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellW.SizeSideBar, champ.SpellW.Sprite[0], SpellSlot.W, UpdateMethod.Side));
            }
            else
            {
                champ.SpellW.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellW.SizeSideBar.Width, champ.SpellW.SizeSideBar.Height);
                };
                champ.SpellW.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
                };
                champ.SpellW.Sprite[0].Sprite.Add(0);
            }

            Console.WriteLine("SpellE");
            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.E).Name, champ.SpellE.Sprite[0], SpriteHelper.DownloadType.Spell);
            champ.SpellE.Sprite[0] = await taskInfo;
            if (!champ.SpellE.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellE.SizeSideBar, champ.SpellE.Sprite[0], SpellSlot.E, UpdateMethod.Side));
            }
            else
            {
                champ.SpellE.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellE.SizeSideBar.Width, champ.SpellE.SizeSideBar.Height);
                };
                champ.SpellE.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
                };
                champ.SpellE.Sprite[0].Sprite.Add(0);
            }

            Console.WriteLine("SpellR");
            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.R).Name, champ.SpellR.Sprite[0], SpriteHelper.DownloadType.Spell);
            champ.SpellR.Sprite[0] = await taskInfo;
            if (!champ.SpellR.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellR.SizeSideBar, champ.SpellR.Sprite[0], SpellSlot.R, UpdateMethod.Side));
            }
            else
            {
                champ.SpellR.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellR.SizeSideBar.Width, champ.SpellR.SizeSideBar.Height);
                };
                champ.SpellR.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
                };
                champ.SpellR.Sprite[0].Sprite.Add(0);
            }

            Console.WriteLine("Spell1");
            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.Summoner1).Name, champ.SpellSum1.Sprite[0], SpriteHelper.DownloadType.Summoner);
            champ.SpellSum1.Sprite[0] = await taskInfo;
            if (!champ.SpellSum1.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, champ.SpellSum1.SizeSideBar, champ.SpellSum1.Sprite[0], SpellSlot.Summoner1, UpdateMethod.Side));
            }
            else
            {
                champ.SpellSum1.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum1.SizeSideBar.Width, champ.SpellSum1.SizeSideBar.Height);
                };
                champ.SpellSum1.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy);
                };
                champ.SpellSum1.Sprite[0].Sprite.Add(0);
            }

            Console.WriteLine("Spell2");
            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.Summoner2).Name, champ.SpellSum2.Sprite[0], SpriteHelper.DownloadType.Summoner);
            champ.SpellSum2.Sprite[0] = await taskInfo;
            if (!champ.SpellSum2.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, champ.SpellSum2.SizeSideBar, champ.SpellSum2.Sprite[0], SpellSlot.Summoner2, UpdateMethod.Side));
            }
            else
            {
                champ.SpellSum2.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum2.SizeSideBar.Width, champ.SpellSum2.SizeSideBar.Height);
                };
                champ.SpellSum2.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy);
                };
                champ.SpellSum2.Sprite[0].Sprite.Add(0);
            }

            Console.WriteLine("Backbar");
            champ.BackBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("BarBackground", ref champ.BackBar.Sprite[0], SpriteHelper.TextureType.Default);
            champ.BackBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                return new Vector2(champ.BackBar.SizeSideBar.Width, champ.BackBar.SizeSideBar.Height);
            };
            champ.BackBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy);
            };
            champ.BackBar.Sprite[0].Sprite.Add(0);

            Console.WriteLine("Healthbar");
            champ.HealthBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("HealthBar", ref champ.HealthBar.Sprite[0], SpriteHelper.TextureType.Default);
            //SetScaleX(ref champ.HealthBar.Sprite[0].Sprite, _healthManaBarSize, percentScale * percentHealth);
            champ.HealthBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                //SetScaleX(ref champ.HealthBar.Sprite[0].Sprite, _healthManaBarSize, percentScale * CalcHpBar(hero));
                return new Vector2(champ.HealthBar.SizeSideBar.Width, champ.HealthBar.SizeSideBar.Height);
            };
            champ.HealthBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy);
            };
            champ.HealthBar.Sprite[0].Sprite.Add(1);

            Console.WriteLine("Manabar");
            champ.ManaBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("ManaBar", ref champ.ManaBar.Sprite[0], SpriteHelper.TextureType.Default);
            //SetScaleX(ref champ.ManaBar.Sprite[0].Sprite, _healthManaBarSize, percentScale * percentMana);
            champ.ManaBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                //SetScaleX(ref champ.ManaBar.Sprite[0].Sprite, _healthManaBarSize, percentScale * CalcManaBar(hero));
                return new Vector2(champ.ManaBar.SizeSideBar.Width, champ.ManaBar.SizeSideBar.Height);
            };
            champ.ManaBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy);
            };
            champ.ManaBar.Sprite[0].Sprite.Add(1);

            Console.WriteLine("Recallbar");
            champ.RecallBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("RecallBar", ref champ.RecallBar.Sprite[0], SpriteHelper.TextureType.Default);
            champ.RecallBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                return new Vector2(champ.RecallBar.SizeSideBar.Width, champ.RecallBar.SizeSideBar.Height);
            };
            champ.RecallBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && 
                    GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
            };
            champ.RecallBar.Sprite[0].Sprite.Color = new ColorBGRA(Color3.White, 0.55f);
            champ.RecallBar.Sprite[0].Sprite.Add(1);

            Console.WriteLine("Goldbar");
            champ.GoldCsLvlBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("GoldCsLvlBar", ref champ.GoldCsLvlBar.Sprite[0], SpriteHelper.TextureType.Default);
            champ.GoldCsLvlBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                return new Vector2(champ.Champ.SizeSideBar.Width, champ.Champ.SizeSideBar.Height);
            };
            champ.GoldCsLvlBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
            };
            champ.GoldCsLvlBar.Sprite[0].Sprite.Color = new ColorBGRA(Color3.White, 0.55f);
            champ.GoldCsLvlBar.Sprite[0].Sprite.Add(1);

            ///////

            champ.SpellR.Rectangle[0] = new Render.Rectangle(champ.SpellR.SizeSideBar.Width, champ.SpellR.SizeSideBar.Height,
                _sumSize.Width, _sumSize.Height, SharpDX.Color.Red);
            champ.SpellR.Rectangle[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.SizeSideBar.Width, champ.SpellR.SizeSideBar.Height);
            };
            champ.SpellR.Rectangle[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 1;
            };
            champ.SpellR.Rectangle[0].Add(1);

            ///////

            champ.HealthBar.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.HealthBar.Text[0].TextUpdate = delegate
            {
                return champ.SHealth ?? "";
            };
            champ.HealthBar.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.HealthBar.CoordsSideBar.Width, champ.HealthBar.CoordsSideBar.Height);
            };
            champ.HealthBar.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy);
            };
            champ.HealthBar.Text[0].OutLined = true;
            champ.HealthBar.Text[0].Centered = true;
            champ.HealthBar.Text[0].Add(4);

            champ.ManaBar.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.ManaBar.Text[0].TextUpdate = delegate
            {
                return champ.SMana ?? "";
            };
            champ.ManaBar.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            };
            champ.ManaBar.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && GetUiActive(hero.IsEnemy);
            };
            champ.ManaBar.Text[0].OutLined = true;
            champ.ManaBar.Text[0].Centered = true;
            champ.ManaBar.Text[0].Add(4);

            champ.SpellQ.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellQ.Text[0].TextUpdate = delegate
            {
                return champ.SpellQ.Value.ToString();
            };
            champ.SpellQ.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellQ.CoordsSideBar.Width, champ.SpellQ.CoordsSideBar.Height);
            };
            champ.SpellQ.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellQ.Value > 0.0f && GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
            };
            champ.SpellQ.Text[0].OutLined = true;
            champ.SpellQ.Text[0].Centered = true;
            champ.SpellQ.Text[0].Add(4);

            champ.SpellW.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellW.Text[0].TextUpdate = delegate
            {
                return champ.SpellW.Value.ToString();
            };
            champ.SpellW.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellW.CoordsSideBar.Width, champ.SpellW.CoordsSideBar.Height);
            };
            champ.SpellW.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellW.Value > 0.0f && GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
            };
            champ.SpellW.Text[0].OutLined = true;
            champ.SpellW.Text[0].Centered = true;
            champ.SpellW.Text[0].Add(4);

            champ.SpellE.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellE.Text[0].TextUpdate = delegate
            {
                return champ.SpellE.Value.ToString();
            };
            champ.SpellE.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellE.CoordsSideBar.Width, champ.SpellE.CoordsSideBar.Height);
            };
            champ.SpellE.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellE.Value > 0.0f && GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
            };
            champ.SpellE.Text[0].OutLined = true;
            champ.SpellE.Text[0].Centered = true;
            champ.SpellE.Text[0].Add(4);

            champ.SpellR.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellR.Text[0].TextUpdate = delegate
            {
                return champ.SpellR.Value.ToString();
            };
            champ.SpellR.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.CoordsSideBar.Width, champ.SpellR.CoordsSideBar.Height);
            };
            champ.SpellR.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellR.Value > 0.0f && GetUiActive(hero.IsEnemy);
            };
            champ.SpellR.Text[0].OutLined = true;
            champ.SpellR.Text[0].Centered = true;
            champ.SpellR.Text[0].Add(4);

            champ.Champ.Text[0] = new Render.Text(0, 0, "", 30, SharpDX.Color.Orange);
            champ.Champ.Text[0].TextUpdate = delegate
            {
                if (champ.DeathTimeDisplay > 0.0f && hero.IsDead)
                    return champ.DeathTimeDisplay.ToString();
                else if (champ.InvisibleTime > 0.0f && !hero.IsVisible)
                    return champ.InvisibleTime.ToString();
                return "";
            };
            champ.Champ.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.Champ.CoordsSideBar.Width, champ.Champ.CoordsSideBar.Height);
            };
            champ.Champ.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    ((champ.DeathTimeDisplay > 0.0f && hero.IsDead) || (champ.InvisibleTime > 0.0f && !hero.IsVisible)) && 
                    GetUiActive(hero.IsEnemy);
            };
            champ.Champ.Text[0].OutLined = true;
            champ.Champ.Text[0].Centered = true;
            champ.Champ.Text[0].Add(4);

            champ.SpellSum1.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum1.Text[0].TextUpdate = delegate
            {
                return champ.SpellSum1.Value.ToString();
            };
            champ.SpellSum1.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum1.CoordsSideBar.Width, champ.SpellSum1.CoordsSideBar.Height);
            };
            champ.SpellSum1.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum1.Value > 0.0f && GetUiActive(hero.IsEnemy);
            };
            champ.SpellSum1.Text[0].OutLined = true;
            champ.SpellSum1.Text[0].Centered = true;
            champ.SpellSum1.Text[0].Add(4);

            champ.SpellSum2.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum2.Text[0].TextUpdate = delegate
            {
                return champ.SpellSum2.Value.ToString();
            };
            champ.SpellSum2.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum2.CoordsSideBar.Width, champ.SpellSum2.CoordsSideBar.Height);
            };
            champ.SpellSum2.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum2.Value > 0.0f && GetUiActive(hero.IsEnemy);
            };
            champ.SpellSum2.Text[0].OutLined = true;
            champ.SpellSum2.Text[0].Centered = true;
            champ.SpellSum2.Text[0].Add(4);

            foreach (var item in champ.Item)
            {
                if (item == null)
                    continue;
                item.Text[0] = new Render.Text(0, 0, "", 12, SharpDX.Color.Orange);
                item.Text[0].TextUpdate = delegate
                {
                    return item.Value.ToString();
                };
                item.Text[0].PositionUpdate = delegate
                {
                    return new Vector2(item.CoordsSideBar.Width, item.CoordsSideBar.Height);
                };
                item.Text[0].VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        item.Value > 0.0f && UiTracker.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>() &&
                        GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
                };
                item.Text[0].OutLined = true;
                item.Text[0].Centered = true;
                item.Text[0].Add(4);
            }

            champ.Level.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.Level.Text[0].TextUpdate = delegate
            {
                return champ.Level.Value.ToString();
            };
            champ.Level.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.Level.CoordsSideBar.Width, champ.Level.CoordsSideBar.Height);
            };
            champ.Level.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0; 
            };
            champ.Level.Text[0].OutLined = true;
            champ.Level.Text[0].Centered = true;
            champ.Level.Text[0].Add(4);

            champ.Cs.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.Cs.Text[0].TextUpdate = delegate
            {
                return champ.Cs.Value.ToString();
            };
            champ.Cs.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.Cs.CoordsSideBar.Width, champ.Cs.CoordsSideBar.Height);
            };
            champ.Cs.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
            };
            champ.Cs.Text[0].OutLined = true;
            champ.Cs.Text[0].Centered = true;
            champ.Cs.Text[0].Add(4);

            champ.RecallBar.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.RecallBar.Text[0].TextUpdate = delegate
            {
                if (champ.RecallInfo.Start != 0)
                {
                    float time = Environment.TickCount + champ.RecallInfo.Duration - champ.RecallInfo.Start;
                    if (time > 0.0f &&
                        (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Start))
                    {
                        return "Porting";
                    }
                    else if (time < 30.0f &&
                             (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Finish))
                    {
                        return "Ported";
                    }
                    else if (time < 30.0f &&
                             (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Abort))
                    {
                        return "Canceled";
                    }
                }
                return "";
            };
            champ.RecallBar.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.RecallBar.CoordsSideBar.Width, champ.RecallBar.CoordsSideBar.Height);
            };
            champ.RecallBar.Text[0].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    GetUiActive(hero.IsEnemy) && GetSideDisplayMode(hero.IsEnemy).SelectedIndex == 0;
            };
            champ.RecallBar.Text[0].OutLined = true;
            champ.RecallBar.Text[0].Centered = true;
            champ.RecallBar.Text[0].Add(4);

            //champ.Champ.Text[0].TextFontDescription = new FontDescription()
            //{
            //    FaceName = "Calibri",
            //    Height = 24,
            //    OutputPrecision = FontPrecision.Default,
            //    Quality = FontQuality.Default
            //};

            return champ;
        }

        private async static Task<ChampInfos> CreateOverHeadHud(Obj_AI_Hero hero, ChampInfos champ, float percentScale)
        {
            float scaleSpell = GetHeadMode(hero.IsEnemy).SelectedIndex == 1 ? 1.7f : 1.0f;
            float scaleSum = GetHeadMode(hero.IsEnemy).SelectedIndex == 1 ? 1.0f : 0.8f;

            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.Summoner1).Name, champ.SpellSum1.Sprite[1], SpriteHelper.DownloadType.Summoner);
            champ.SpellSum1.Sprite[1] = await taskInfo;
            if (!champ.SpellSum1.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, champ.SpellSum1.SizeHpBar, champ.SpellSum1.Sprite[1], SpellSlot.Summoner1, UpdateMethod.Hp));
            }
            else
            {
                champ.SpellSum1.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum1.SizeHpBar.Width, champ.SpellSum1.SizeHpBar.Height);
                };
                champ.SpellSum1.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 0 && hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
                };
                champ.SpellSum1.Sprite[1].Sprite.Add();
            }

            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.Summoner2).Name, champ.SpellSum2.Sprite[1], SpriteHelper.DownloadType.Summoner);
            champ.SpellSum2.Sprite[1] = await taskInfo;
            if (!champ.SpellSum2.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, champ.SpellSum2.SizeHpBar, champ.SpellSum2.Sprite[1], SpellSlot.Summoner2, UpdateMethod.Hp));
            }
            else
            {
                champ.SpellSum2.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum2.SizeHpBar.Width, champ.SpellSum2.SizeHpBar.Height);
                };
                champ.SpellSum2.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                           GetMode(hero.IsEnemy).SelectedIndex != 0 &&
                           hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
                };
                champ.SpellSum2.Sprite[1].Sprite.Add();
            }      

            //SpriteHelper.LoadTexture(s1[1].Name + ".dds", "PASSIVE/", loc + "PASSIVE\\" + s1[1].Name + ".dds", ref champ.Passive.Texture);
            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.Q).Name, champ.SpellQ.Sprite[1], SpriteHelper.DownloadType.Spell);
            champ.SpellQ.Sprite[1] = await taskInfo;
            if (!champ.SpellQ.Sprite[1].LoadingFinished)
            {
                //Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellQ.SizeHpBar, champ.SpellQ.Sprite[1], SpellSlot.Q, UpdateMethod.Hp));
            }
            else
            {
                //SetScale(ref champ.SpellQ.Sprite[1].Sprite, _spellSize, scaleSpell * percentScale);
                champ.SpellQ.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellQ.SizeHpBar.Width, champ.SpellQ.SizeHpBar.Height);
                };
                champ.SpellQ.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
                };
                champ.SpellQ.Sprite[1].Sprite.Add();
            }

            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.W).Name, champ.SpellW.Sprite[1], SpriteHelper.DownloadType.Spell);
            champ.SpellW.Sprite[1] = await taskInfo;
            if (!champ.SpellW.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellW.SizeHpBar, champ.SpellW.Sprite[1], SpellSlot.W, UpdateMethod.Hp));
            }
            else
            {
                champ.SpellW.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellW.SizeHpBar.Width, champ.SpellW.SizeHpBar.Height);
                };
                champ.SpellW.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
                };
                champ.SpellW.Sprite[1].Sprite.Add();
            }

            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.E).Name, champ.SpellE.Sprite[1], SpriteHelper.DownloadType.Spell);
            champ.SpellE.Sprite[1] = await taskInfo;
            if (!champ.SpellE.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellE.SizeHpBar, champ.SpellE.Sprite[1], SpellSlot.E, UpdateMethod.Hp));
            }
            else
            {
                champ.SpellE.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellE.SizeHpBar.Width, champ.SpellE.SizeHpBar.Height);
                };
                champ.SpellE.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
                };
                champ.SpellE.Sprite[1].Sprite.Add();
            }

            taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.R).Name, champ.SpellR.Sprite[1], SpriteHelper.DownloadType.Spell);
            champ.SpellR.Sprite[1] = await taskInfo;
            if (!champ.SpellR.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellR.SizeHpBar, champ.SpellR.Sprite[1], SpellSlot.R, UpdateMethod.Hp));
            }
            else
            {
                champ.SpellR.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellR.SizeHpBar.Width, champ.SpellR.SizeHpBar.Height);
                };
                champ.SpellR.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
                };
                champ.SpellR.Sprite[1].Sprite.Add();
            }                

            //////

            champ.SpellQ.Rectangle[1] = new Render.Rectangle(champ.SpellQ.SizeHpBar.Width, champ.SpellQ.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellQ.Rectangle[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellQ.SizeHpBar.Width, champ.SpellQ.SizeHpBar.Height);
            };
            champ.SpellQ.Rectangle[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
            };
            champ.SpellQ.Rectangle[1].Add();

            champ.SpellW.Rectangle[1] = new Render.Rectangle(champ.SpellW.SizeHpBar.Width, champ.SpellW.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellW.Rectangle[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellW.SizeHpBar.Width, champ.SpellW.SizeHpBar.Height);
            };
            champ.SpellW.Rectangle[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
            };
            champ.SpellW.Rectangle[1].Add();

            champ.SpellE.Rectangle[1] = new Render.Rectangle(champ.SpellE.SizeHpBar.Width, champ.SpellE.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellE.Rectangle[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellE.SizeHpBar.Width, champ.SpellE.SizeHpBar.Height);
            };
            champ.SpellE.Rectangle[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
            };
            champ.SpellE.Rectangle[1].Add();

            champ.SpellR.Rectangle[1] = new Render.Rectangle(champ.SpellR.SizeHpBar.Width, champ.SpellR.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellR.Rectangle[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.SizeHpBar.Width, champ.SpellR.SizeHpBar.Height);
            };
            champ.SpellR.Rectangle[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
            };
            champ.SpellR.Rectangle[1].Add();

            ///////
             
            champ.SpellQ.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellQ.Text[1].TextUpdate = delegate
            {
                return champ.SpellQ.Value.ToString();
            };
            champ.SpellQ.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellQ.CoordsHpBar.Width, champ.SpellQ.CoordsHpBar.Height);
            };
            champ.SpellQ.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() &&
                    champ.SpellQ.Value > 0.0f && hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
            };
            champ.SpellQ.Text[1].OutLined = true;
            champ.SpellQ.Text[1].Centered = true;
            champ.SpellQ.Text[1].Add();

            champ.SpellW.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellW.Text[1].TextUpdate = delegate
            {
                return champ.SpellW.Value.ToString();
            };
            champ.SpellW.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellW.CoordsHpBar.Width, champ.SpellW.CoordsHpBar.Height);
            };
            champ.SpellW.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellW.Value > 0.0f && hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
            };
            champ.SpellW.Text[1].OutLined = true;
            champ.SpellW.Text[1].Centered = true;
            champ.SpellW.Text[1].Add();

            champ.SpellE.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellE.Text[1].TextUpdate = delegate
            {
                return champ.SpellE.Value.ToString();
            };
            champ.SpellE.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellE.CoordsHpBar.Width, champ.SpellE.CoordsHpBar.Height);
            };
            champ.SpellE.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellE.Value > 0.0f && hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
            };
            champ.SpellE.Text[1].OutLined = true;
            champ.SpellE.Text[1].Centered = true;
            champ.SpellE.Text[1].Add();

            champ.SpellR.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellR.Text[1].TextUpdate = delegate
            {
                return champ.SpellR.Value.ToString();
            };
            champ.SpellR.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.CoordsHpBar.Width, champ.SpellR.CoordsHpBar.Height);
            };
            champ.SpellR.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellR.Value > 0.0f && hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
            };
            champ.SpellR.Text[1].OutLined = true;
            champ.SpellR.Text[1].Centered = true;
            champ.SpellR.Text[1].Add();

            champ.SpellSum1.Text[1] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum1.Text[1].TextUpdate = delegate
            {
                return champ.SpellSum1.Value.ToString();
            };
            champ.SpellSum1.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum1.CoordsHpBar.Width, champ.SpellSum1.CoordsHpBar.Height);
            };
            champ.SpellSum1.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum1.Value > 0.0f && hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy);
            };
            champ.SpellSum1.Text[1].OutLined = true;
            champ.SpellSum1.Text[1].Centered = true;
            champ.SpellSum1.Text[1].Add();

            champ.SpellSum2.Text[1] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum2.Text[1].TextUpdate = delegate
            {
                return champ.SpellSum2.Value.ToString();
            };
            champ.SpellSum2.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum2.CoordsHpBar.Width, champ.SpellSum2.CoordsHpBar.Height);
            };
            champ.SpellSum2.Text[1].VisibleCondition = sender =>
            {
                return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum2.Value > 0.0f && hero.IsVisible && !hero.IsDead && GetUiActive(hero.IsEnemy); 
            };
            champ.SpellSum2.Text[1].OutLined = true;
            champ.SpellSum2.Text[1].Centered = true;
            champ.SpellSum2.Text[1].Add();

            return champ;
        }

        private static bool GetUiActive(bool enemy)
        {
            if (enemy)
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker").GetActive();
            }
            else
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker").GetActive();
            }
        }

        private static StringList GetMode(bool enemy)
        {
            if (enemy)
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerMode")
                        .GetValue<StringList>();
            }
            else
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerMode")
                        .GetValue<StringList>();
            }
        }

        private static StringList GetSideDisplayMode(bool enemy)
        {
            if (enemy)
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerSideDisplayMode")
                        .GetValue<StringList>();
            }
            else
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerSideDisplayMode")
                        .GetValue<StringList>();
            }
        }

        private static StringList GetHeadMode(bool enemy)
        {
            if (enemy)
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerHeadMode")
                        .GetValue<StringList>();
            }
            else
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerHeadMode")
                        .GetValue<StringList>();
            }
        }

        private static StringList GetHeadDisplayMode(bool enemy)
        {
            if (enemy)
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerHeadDisplayMode")
                        .GetValue<StringList>();
            }
            else
            {
                return UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerHeadDisplayMode")
                        .GetValue<StringList>();
            }
        }

        private void CalculateSizes(bool calcEenemy)
        {
            Dictionary<Obj_AI_Hero, ChampInfos> heroes;
            float percentScale;
            StringList mode;
            StringList modeHead;
            StringList modeDisplay;
            int count;
            int xOffset;
            int yOffset;
            int yOffsetAdd;
            if (calcEenemy)
            {
                heroes = _enemies;
                percentScale = (float) UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value/
                               100;
                mode =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerMode")
                        .GetValue<StringList>();
                modeHead =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerHeadMode")
                        .GetValue<StringList>();
                modeDisplay =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerSideDisplayMode")
                        .GetValue<StringList>();
                count = 0;
                xOffset =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                        .GetValue<Slider>()
                        .Value;
                _oldEx = xOffset;
                yOffset =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                        .GetValue<Slider>()
                        .Value;
                _oldEy = yOffset;
                yOffsetAdd = (int) (20*percentScale);
            }
            else
            {
                heroes = _allies;
                percentScale = (float) UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value/
                               100;
                mode =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerMode")
                        .GetValue<StringList>();
                modeHead =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerHeadMode")
                        .GetValue<StringList>();
                modeDisplay =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerSideDisplayMode")
                        .GetValue<StringList>();
                count = 0;
                xOffset =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                        .GetValue<Slider>()
                        .Value;
                _oldAx = xOffset;
                yOffset =
                    UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                        .GetValue<Slider>()
                        .Value;
                _oldAy = yOffset;
                yOffsetAdd = (int) (20*percentScale);
            }

            _hudSize = new Size();
            foreach (var hero in heroes)
            {
                float scaleSpell = GetHeadMode(hero.Key.IsEnemy).SelectedIndex == 1 ? 1.7f : 1.0f;
                float scaleSum = GetHeadMode(hero.Key.IsEnemy).SelectedIndex == 1 ? 1.0f : 0.8f;
                if (mode.SelectedIndex == 0 || mode.SelectedIndex == 2)
                {
                    if (modeDisplay.SelectedIndex == 0)
                    {
                        if(hero.Value.SpellPassive != null)
                        {
                            hero.Value.SpellPassive.SizeSideBar =
                            new Size(
                            xOffset - (int)(_champSize.Width * percentScale) - (int)(_sumSize.Width * percentScale) -
                            (int)(_spellSize.Width * percentScale),
                            yOffset - (int)(_spellSize.Height * percentScale) * (count * 4 - 0) -
                            count * (int)(_backBarSize.Height * percentScale) -
                            count * (int)(_spellSize.Height * percentScale) - yOffsetAdd);
                            hero.Value.SpellPassive.CoordsSideBar =
                            new Size(
                                hero.Value.SpellPassive.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2,
                                hero.Value.SpellPassive.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        }
                        if (hero.Value.SpellQ != null && hero.Value.SpellQ.Sprite[0] != null && hero.Value.SpellQ.Sprite[0].Sprite != null)
                        {
                            hero.Value.SpellQ.SizeSideBar = new Size(hero.Value.SpellPassive.SizeSideBar.Width, hero.Value.SpellPassive.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) * 1);
                            hero.Value.SpellQ.Sprite[0].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellQ.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.SpellQ.CoordsSideBar = new Size(hero.Value.SpellQ.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2, hero.Value.SpellQ.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        }

                        if (hero.Value.SpellW != null && hero.Value.SpellW.Sprite[0] != null && hero.Value.SpellW.Sprite[0].Sprite != null)
                        {
                            hero.Value.SpellW.SizeSideBar = new Size(hero.Value.SpellPassive.SizeSideBar.Width, hero.Value.SpellPassive.SizeSideBar.Height + (int) (_spellSize.Height*percentScale)*2);
                            hero.Value.SpellW.Sprite[0].Sprite.Scale = new Vector2(((float) _spellSize.Width/hero.Value.SpellW.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.SpellW.CoordsSideBar = new Size(hero.Value.SpellW.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2, hero.Value.SpellW.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        }

                        if (hero.Value.SpellE != null && hero.Value.SpellE.Sprite[0] != null && hero.Value.SpellE.Sprite[0].Sprite != null)
                        {
                            hero.Value.SpellE.SizeSideBar = new Size(hero.Value.SpellPassive.SizeSideBar.Width, hero.Value.SpellPassive.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) * 3);
                            hero.Value.SpellE.Sprite[0].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellE.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.SpellE.CoordsSideBar = new Size(hero.Value.SpellE.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2, hero.Value.SpellE.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        }

                        if (hero.Value.SpellR != null && hero.Value.SpellR.Sprite[0] != null && hero.Value.SpellR.Sprite[0].Sprite != null)
                        {
                            hero.Value.SpellR.SizeSideBar = new Size(hero.Value.SpellPassive.SizeSideBar.Width, hero.Value.SpellPassive.SizeSideBar.Height + (int) (_spellSize.Height*percentScale) * 4);
                            hero.Value.SpellR.Sprite[0].Sprite.Scale = new Vector2(((float) _spellSize.Width/hero.Value.SpellR.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.SpellR.CoordsSideBar = new Size(hero.Value.SpellR.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2, hero.Value.SpellR.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        }

                        if (hero.Value.Champ != null && hero.Value.Champ.Sprite[0] != null && hero.Value.Champ.Sprite[0].Sprite != null)
                        {
                            hero.Value.Champ.SizeSideBar = new Size(hero.Value.SpellPassive.SizeSideBar.Width + (int)(_spellSize.Width * percentScale), hero.Value.SpellPassive.SizeSideBar.Height);
                            hero.Value.Champ.Sprite[0].Sprite.Scale = new Vector2(((float)_champSize.Width / hero.Value.Champ.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.Champ.CoordsSideBar = new Size(hero.Value.Champ.SizeSideBar.Width + (int)(_champSize.Width * percentScale) / 2, hero.Value.Champ.SizeSideBar.Height + (int)(_champSize.Height * percentScale) / 2);
                        }

                        if (hero.Value.SpellSum1 != null && hero.Value.SpellSum1.Sprite[0] != null && hero.Value.SpellSum1.Sprite[0].Sprite != null)
                        {
                            hero.Value.SpellSum1.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width + (int)(_champSize.Width * percentScale), hero.Value.SpellPassive.SizeSideBar.Height);
                            hero.Value.SpellSum1.Sprite[0].Sprite.Scale = new Vector2(((float)_sumSize.Width / hero.Value.SpellSum1.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.SpellSum1.CoordsSideBar = new Size(hero.Value.SpellSum1.SizeSideBar.Width + (int)(_sumSize.Width * percentScale) / 2, hero.Value.SpellSum1.SizeSideBar.Height + (int)(_sumSize.Height * percentScale) / 2);
                        }

                        if (hero.Value.SpellSum2 != null && hero.Value.SpellSum2.Sprite[0] != null && hero.Value.SpellSum2.Sprite[0].Sprite != null)
                        {
                            hero.Value.SpellSum2.SizeSideBar = new Size(hero.Value.SpellSum1.SizeSideBar.Width, hero.Value.SpellPassive.SizeSideBar.Height + (int)(_sumSize.Height * percentScale));
                            hero.Value.SpellSum2.Sprite[0].Sprite.Scale = new Vector2(((float)_sumSize.Width / hero.Value.SpellSum2.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.SpellSum2.CoordsSideBar = new Size(hero.Value.SpellSum2.SizeSideBar.Width + (int)(_sumSize.Width * percentScale) / 2, hero.Value.SpellSum2.SizeSideBar.Height + (int)(_sumSize.Height * percentScale) / 2);
                        }
                        
                        if (hero.Value.Item[0] == null)
                            hero.Value.Item[0] = new ChampInfos.SpriteInfos();
                        hero.Value.Item[0].SizeSideBar = new Size(hero.Value.SpellR.SizeSideBar.Width,
                            hero.Value.SpellR.SizeSideBar.Height + (int)(_spellSize.Height * percentScale));
                        for (int i = 1; i < hero.Value.Item.Length; i++)
                        {
                            if (hero.Value.Item[i] == null)
                                hero.Value.Item[i] = new ChampInfos.SpriteInfos();
                            hero.Value.Item[i].SizeSideBar =
                                new Size(
                                    hero.Value.Item[0].SizeSideBar.Width + (int)(_spellSize.Width * percentScale) * i,
                                    hero.Value.Item[0].SizeSideBar.Height);
                        }
                        if (hero.Value.Item[0] == null)
                            hero.Value.Item[0] = new ChampInfos.SpriteInfos();
                        hero.Value.Item[0].CoordsSideBar = new Size(hero.Value.SpellR.CoordsSideBar.Width,
                            hero.Value.SpellR.CoordsSideBar.Height + (int)(_spellSize.Height * percentScale));
                        for (int i = 1; i < hero.Value.Item.Length; i++)
                        {
                            if (hero.Value.Item[i] == null)
                                hero.Value.Item[i] = new ChampInfos.SpriteInfos();
                            hero.Value.Item[i].CoordsSideBar =
                                new Size(
                                    hero.Value.Item[0].CoordsSideBar.Width + (int)(_spellSize.Width * percentScale) * i,
                                    hero.Value.Item[0].CoordsSideBar.Height);
                        }

                        if (hero.Value.BackBar != null && hero.Value.BackBar.Sprite[0] != null && hero.Value.BackBar.Sprite[0].Sprite != null)
                        {
                            hero.Value.BackBar.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width, hero.Value.SpellSum2.SizeSideBar.Height + (int)(_sumSize.Height * percentScale));
                            hero.Value.BackBar.Sprite[0].Sprite.Scale = new Vector2(((float)_backBarSize.Width / hero.Value.BackBar.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                        }

                        if (hero.Value.HealthBar != null && hero.Value.HealthBar.Sprite[0] != null && hero.Value.HealthBar.Sprite[0].Sprite != null)
                        {
                            hero.Value.HealthBar.SizeSideBar = new Size(hero.Value.BackBar.SizeSideBar.Width, hero.Value.BackBar.SizeSideBar.Height + 1);
                            float healthPercent = CalcHpBar(hero.Key);
                            hero.Value.HealthBar.Sprite[0].Sprite.Scale = new Vector2(((float)_healthManaBarSize.Width / hero.Value.HealthBar.Sprite[0].Sprite.Bitmap.Width) * (healthPercent * percentScale), percentScale);
                            hero.Value.HealthBar.CoordsSideBar = new Size(hero.Value.HealthBar.SizeSideBar.Width + (int)(_healthManaBarSize.Width * percentScale) / 2, hero.Value.HealthBar.SizeSideBar.Height - (int)(_healthManaBarSize.Height * percentScale) / 4);
                        }

                        if (hero.Value.ManaBar != null && hero.Value.ManaBar.Sprite[0] != null && hero.Value.ManaBar.Sprite[0].Sprite != null)
                        {
                            hero.Value.ManaBar.SizeSideBar = new Size(hero.Value.BackBar.SizeSideBar.Width, hero.Value.BackBar.SizeSideBar.Height + (int)(_healthManaBarSize.Height * percentScale) + 3);
                            float manaPercent = CalcHpBar(hero.Key);
                            hero.Value.ManaBar.Sprite[0].Sprite.Scale = new Vector2(((float)_healthManaBarSize.Width / hero.Value.ManaBar.Sprite[0].Sprite.Bitmap.Width) * (manaPercent * percentScale), percentScale);
                            hero.Value.ManaBar.CoordsSideBar = new Size(hero.Value.ManaBar.SizeSideBar.Width + (int)(_healthManaBarSize.Width * percentScale) / 2, hero.Value.ManaBar.SizeSideBar.Height - (int)(_healthManaBarSize.Height * percentScale) / 4 + 3);
                        }
                        
                        hero.Value.SHealth = ((int)hero.Key.Health) + "/" + ((int)hero.Key.MaxHealth);
                        hero.Value.SMana = ((int)hero.Key.Mana) + "/" + ((int)hero.Key.MaxMana);

                        if (hero.Value.RecallBar != null && hero.Value.RecallBar.Sprite[0] != null && hero.Value.RecallBar.Sprite[0].Sprite != null)
                        {
                            hero.Value.RecallBar.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width, hero.Value.BackBar.SizeSideBar.Height - (int)(_champSize.Height * percentScale) / 4);
                            if (hero.Value.RecallInfo.Start != 0)
                            {
                                float time = Environment.TickCount + hero.Value.RecallInfo.Duration - hero.Value.RecallInfo.Start;
                                if (time > 0.0f && (hero.Value.RecallInfo.Status == Packet.S2C.Teleport.Status.Start))
                                {
                                    float value = ((float)_champSize.Width / hero.Value.RecallBar.Sprite[0].Sprite.Bitmap.Width) * (percentScale);
                                    hero.Value.RecallBar.Sprite[0].Sprite.Scale = new Vector2(CalcRecallBar(hero.Value.RecallInfo) * value, 1 * value);
                                }
                            }
                            else if (hero.Value.RecallInfo.Status != Packet.S2C.Teleport.Status.Start)
                            {
                                hero.Value.RecallBar.Sprite[0].Sprite.Scale = new Vector2(((float)_champSize.Width / hero.Value.RecallBar.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            }
                            hero.Value.RecallBar.CoordsSideBar = new Size(hero.Value.RecallBar.SizeSideBar.Width + (int)(_recSize.Width * percentScale) / 2, hero.Value.RecallBar.SizeSideBar.Height + (int)(_recSize.Height * percentScale) / 4);
                        }


                        if (hero.Value.Level != null)
                        {
                            hero.Value.Level.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width, hero.Value.Champ.SizeSideBar.Height);
                            hero.Value.Level.CoordsSideBar = new Size(hero.Value.Champ.SizeSideBar.Width + (int) (_champSize.Width*percentScale) / 8, hero.Value.Champ.SizeSideBar.Height + (int) (_recSize.Height*percentScale)/2);
                        }

                        if (hero.Value.Level != null)
                        {
                            hero.Value.Cs.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width, hero.Value.Champ.SizeSideBar.Height);
                            hero.Value.Cs.CoordsSideBar = new Size(hero.Value.Champ.SizeSideBar.Width + (int)((_champSize.Width * percentScale) / 1.2), hero.Value.Champ.SizeSideBar.Height + (int)(_recSize.Height * percentScale) / 2);
                        }

                        if (hero.Value.Level != null)
                        {
                            hero.Value.Gold.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width, hero.Value.Champ.SizeSideBar.Height);
                            hero.Value.Gold.CoordsSideBar = new Size(hero.Value.Champ.SizeSideBar.Width + (int)(_champSize.Width * percentScale) / 2, hero.Value.Champ.SizeSideBar.Height + (int)(_recSize.Height * percentScale) / 2);
                        }

                        yOffsetAdd += (int)(5 * percentScale);
                        Size nSize = (hero.Value.Item[hero.Value.Item.Length - 1].SizeSideBar) -
                                     (hero.Value.SpellPassive.SizeSideBar);
                        nSize.Height += (int)(8 * percentScale);
                        _hudSize += nSize;
                        _hudSize.Width = nSize.Width;
                        _hudSize.Width += _spellSize.Width;
                        _hudSize.Height += (int)(20 * percentScale);
                        count++;
                    }
                    else
                    {
                        //yOffsetAdd = (int) (20*percentScale);
                        if (hero.Value.Champ != null && hero.Value.Champ.Sprite[0] != null && hero.Value.Champ.Sprite[0].Sprite != null)
                        {
                            hero.Value.Champ.SizeSideBar =
                            new Size(
                                xOffset - (int)(_champSize.Width * percentScale),
                                yOffset - count * (int)(_champSize.Height * percentScale) -
                            count * (int)(_backBarSize.Height * percentScale) - yOffsetAdd);
                            hero.Value.Champ.Sprite[0].Sprite.Scale = new Vector2(((float)_champSize.Width / hero.Value.Champ.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.Champ.CoordsSideBar = new Size(hero.Value.Champ.SizeSideBar.Width + (int)(_champSize.Width * percentScale) / 2, hero.Value.Champ.SizeSideBar.Height + (int)(_champSize.Height * percentScale) / 2);
                        }
                        

                        if (hero.Value.SpellSum1 != null && hero.Value.SpellSum1.Sprite[0] != null && hero.Value.SpellSum1.Sprite[0].Sprite != null)
                        {
                            hero.Value.SpellSum1.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width - (int)(_sumSize.Width * percentScale), hero.Value.Champ.SizeSideBar.Height);
                            hero.Value.SpellSum1.Sprite[0].Sprite.Scale = new Vector2(((float)_sumSize.Width / hero.Value.SpellSum1.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.SpellSum1.CoordsSideBar = new Size(hero.Value.SpellSum1.SizeSideBar.Width + (int)(_sumSize.Width * percentScale) / 2, hero.Value.SpellSum1.SizeSideBar.Height + (int)(_sumSize.Height * percentScale) / 2);
                        }
                        

                        if (hero.Value.SpellSum2 != null && hero.Value.SpellSum2.Sprite[0] != null && hero.Value.SpellSum2.Sprite[0].Sprite != null)
                        {
                            hero.Value.SpellSum2.SizeSideBar = new Size(hero.Value.SpellSum1.SizeSideBar.Width, hero.Value.Champ.SizeSideBar.Height + (int)(_sumSize.Height * percentScale));
                            hero.Value.SpellSum2.Sprite[0].Sprite.Scale = new Vector2(((float)_sumSize.Width / hero.Value.SpellSum2.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.SpellSum2.CoordsSideBar = new Size(hero.Value.SpellSum2.SizeSideBar.Width + (int)(_sumSize.Width * percentScale) / 2, hero.Value.SpellSum2.SizeSideBar.Height + (int)(_sumSize.Height * percentScale) / 2);
                        }
                        

                        if (hero.Value.SpellR != null && hero.Value.SpellR.Sprite[0] != null && hero.Value.SpellR.Sprite[0].Sprite != null)
                        {
                            hero.Value.SpellR.SizeSideBar = new Size(xOffset - (int)(_sumSize.Width * percentScale), hero.Value.Champ.SizeSideBar.Height);
                            hero.Value.SpellR.Sprite[0].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellR.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.SpellR.CoordsSideBar = new Size(hero.Value.SpellR.SizeSideBar.Width + (int)(_spellSize.Width * percentScale), hero.Value.SpellR.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        }

                        if (hero.Value.BackBar != null && hero.Value.BackBar.Sprite[0] != null && hero.Value.BackBar.Sprite[0].Sprite != null)
                        {
                            hero.Value.BackBar.SizeSideBar = new Size(hero.Value.SpellSum1.SizeSideBar.Width, hero.Value.SpellSum2.SizeSideBar.Height + (int)(_sumSize.Height * percentScale));
                            hero.Value.BackBar.Sprite[0].Sprite.Scale = new Vector2(((float)_backBarSize.Width / hero.Value.BackBar.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                        }
                        

                        if (hero.Value.HealthBar != null && hero.Value.HealthBar.Sprite[0] != null && hero.Value.HealthBar.Sprite[0].Sprite != null)
                        {
                            hero.Value.HealthBar.SizeSideBar = new Size(hero.Value.BackBar.SizeSideBar.Width, hero.Value.BackBar.SizeSideBar.Height);
                            hero.Value.HealthBar.Sprite[0].Sprite.Scale = new Vector2(((float)_healthManaBarSize.Width / hero.Value.HealthBar.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.HealthBar.CoordsSideBar = new Size(hero.Value.HealthBar.SizeSideBar.Width + (int)(_healthManaBarSize.Width * percentScale) / 2, hero.Value.HealthBar.SizeSideBar.Height - (int)(_healthManaBarSize.Height * percentScale) / 4);
                        }
                        

                        if (hero.Value.ManaBar != null && hero.Value.ManaBar.Sprite[0] != null && hero.Value.ManaBar.Sprite[0].Sprite != null)
                        {
                            hero.Value.ManaBar.SizeSideBar = new Size(hero.Value.BackBar.SizeSideBar.Width, hero.Value.BackBar.SizeSideBar.Height + (int)(_healthManaBarSize.Height * percentScale) + 3);
                            hero.Value.ManaBar.Sprite[0].Sprite.Scale = new Vector2(((float)_healthManaBarSize.Width / hero.Value.ManaBar.Sprite[0].Sprite.Bitmap.Width) * (percentScale));
                            hero.Value.ManaBar.CoordsSideBar = new Size(hero.Value.ManaBar.SizeSideBar.Width + (int)(_healthManaBarSize.Width * percentScale) / 2, hero.Value.ManaBar.SizeSideBar.Height - (int)(_healthManaBarSize.Height * percentScale) / 4);
                        }
                        
                        hero.Value.SHealth = ((int)hero.Key.Health) + "/" + ((int)hero.Key.MaxHealth);
                        hero.Value.SMana = ((int)hero.Key.Mana) + "/" + ((int)hero.Key.MaxMana);

                        //For champ click/move
                        hero.Value.SpellPassive.SizeSideBar = hero.Value.SpellSum1.SizeSideBar;

                        yOffsetAdd += (int)(5 * percentScale);
                        Size nSize = (hero.Value.ManaBar.SizeSideBar) -
                                     (hero.Value.SpellSum1.SizeSideBar);
                        nSize.Height += (int)(8 * percentScale);
                        _hudSize += nSize;
                        _hudSize.Width = nSize.Width;
                        _hudSize.Width += _spellSize.Width;
                        _hudSize.Height += (int)(20 * percentScale);
                        count++;
                    }
                }
                if (mode.SelectedIndex == 1 || mode.SelectedIndex == 2)
                {
                    if (modeHead.SelectedIndex == 0)
                    {
                        const float hpPosScale = 0.8f;
                        Vector2 hpPos = hero.Key.HPBarPosition;
                        if (hero.Value.SpellSum1 != null && hero.Value.SpellSum1.Sprite[1] != null && hero.Value.SpellSum1.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellSum1.SizeHpBar = new Size((int)hpPos.X - 20, (int)hpPos.Y);
                            hero.Value.SpellSum1.Sprite[1].Sprite.Scale = new Vector2(((float)_sumSize.Width / hero.Value.SpellSum1.Sprite[1].Sprite.Bitmap.Width) * (scaleSum * percentScale));
                            hero.Value.SpellSum1.CoordsHpBar = new Size(hero.Value.SpellSum1.SizeHpBar.Width + _sumSize.Width / 2, hero.Value.SpellSum1.SizeHpBar.Height + _sumSize.Height / 2);
                        }

                        if (hero.Value.SpellSum2 != null && hero.Value.SpellSum2.Sprite[1] != null && hero.Value.SpellSum2.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellSum2.SizeHpBar = new Size(hero.Value.SpellSum1.SizeHpBar.Width, hero.Value.SpellSum1.SizeHpBar.Height + (int)(_sumSize.Height * hpPosScale));
                            hero.Value.SpellSum2.Sprite[1].Sprite.Scale = new Vector2(((float)_sumSize.Width / hero.Value.SpellSum2.Sprite[1].Sprite.Bitmap.Width) * (scaleSum * percentScale));
                            hero.Value.SpellSum2.CoordsHpBar = new Size(hero.Value.SpellSum2.SizeHpBar.Width + _sumSize.Width / 2, hero.Value.SpellSum2.SizeHpBar.Height + _sumSize.Height / 2);
                        }

                        if (hero.Value.SpellPassive != null && hero.Value.SpellPassive.Sprite[1] != null && hero.Value.SpellPassive.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellPassive.SizeHpBar = new Size(hero.Value.SpellSum1.SizeHpBar.Width + _sumSize.Width, hero.Value.SpellSum2.SizeHpBar.Height + (int)((_spellSize.Height * hpPosScale) / 1.5));
                            //hero.Value.SpellPassive.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellPassive.Sprite[1].Sprite.Width) * scaleSum * percentScale,
                            //    ((float)_spellSize.Height / hero.Value.SpellPassive.Sprite[1].Sprite.Height) * scaleSum * percentScale);
                            hero.Value.SpellPassive.CoordsHpBar = new Size(hero.Value.SpellPassive.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellPassive.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.SpellQ != null && hero.Value.SpellQ.Sprite[1] != null && hero.Value.SpellQ.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellQ.SizeHpBar = new Size(hero.Value.SpellPassive.SizeHpBar.Width + _spellSize.Width, hero.Value.SpellPassive.SizeHpBar.Height);
                            hero.Value.SpellQ.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellQ.Sprite[1].Sprite.Bitmap.Width) * (scaleSpell * percentScale));
                            hero.Value.SpellQ.CoordsHpBar = new Size(hero.Value.SpellQ.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellQ.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.SpellW != null && hero.Value.SpellW.Sprite[1] != null && hero.Value.SpellW.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellW.SizeHpBar = new Size(hero.Value.SpellQ.SizeHpBar.Width + _spellSize.Width, hero.Value.SpellQ.SizeHpBar.Height);
                            hero.Value.SpellW.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellW.Sprite[1].Sprite.Bitmap.Width) * (scaleSpell * percentScale));
                            hero.Value.SpellW.CoordsHpBar = new Size(hero.Value.SpellW.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellW.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.SpellE != null && hero.Value.SpellE.Sprite[1] != null && hero.Value.SpellE.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellE.SizeHpBar = new Size(hero.Value.SpellW.SizeHpBar.Width + _spellSize.Width, hero.Value.SpellW.SizeHpBar.Height);
                            hero.Value.SpellE.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellE.Sprite[1].Sprite.Bitmap.Width) * (scaleSpell * percentScale));
                            hero.Value.SpellE.CoordsHpBar = new Size(hero.Value.SpellE.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellE.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.SpellR != null && hero.Value.SpellR.Sprite[1] != null && hero.Value.SpellR.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellR.SizeHpBar = new Size(hero.Value.SpellE.SizeHpBar.Width + _spellSize.Width, hero.Value.SpellE.SizeHpBar.Height);
                            hero.Value.SpellR.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellR.Sprite[1].Sprite.Bitmap.Width) * (scaleSpell * percentScale));
                            hero.Value.SpellR.CoordsHpBar = new Size(hero.Value.SpellR.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellR.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.Exp[0] != null && hero.Value.Exp[0].Line[1] != null && hero.Value.Exp[0].Line[1] != null)
                        {
                            hero.Value.Exp[0].SizeHpBar = new Size((int)hpPos.X, (int)hpPos.Y);
                            hero.Value.Exp[0].CoordsHpBar = new Size(hero.Value.SpellR.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellR.SizeHpBar.Height + _spellSize.Height / 2);
                        }
                    }
                    else
                    {
                        const float hpPosScale = 1.7f;
                        Vector2 hpPos = hero.Key.HPBarPosition;
                        if (hero.Value.SpellSum1 != null && hero.Value.SpellSum1.Sprite[1] != null && hero.Value.SpellSum1.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellSum1.SizeHpBar = new Size((int)hpPos.X - 25, (int)hpPos.Y + 2);
                            hero.Value.SpellSum1.Sprite[1].Sprite.Scale = new Vector2(((float)_sumSize.Width / hero.Value.SpellSum1.Sprite[1].Sprite.Bitmap.Width) * (scaleSum * percentScale));
                            hero.Value.SpellSum1.CoordsHpBar = new Size(hero.Value.SpellSum1.SizeHpBar.Width + _sumSize.Width / 2, hero.Value.SpellSum1.SizeHpBar.Height + _sumSize.Height / 8);
                        }

                        if (hero.Value.SpellSum2 != null && hero.Value.SpellSum2.Sprite[1] != null && hero.Value.SpellSum2.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellSum2.SizeHpBar = new Size(hero.Value.SpellSum1.SizeHpBar.Width, hero.Value.SpellSum1.SizeHpBar.Height + (int)(_sumSize.Height * 1.0f));
                            hero.Value.SpellSum2.Sprite[1].Sprite.Scale = new Vector2(((float)_sumSize.Width / hero.Value.SpellSum2.Sprite[1].Sprite.Bitmap.Width) * (scaleSum * percentScale));
                            hero.Value.SpellSum2.CoordsHpBar = new Size(hero.Value.SpellSum2.SizeHpBar.Width + _sumSize.Width / 2, hero.Value.SpellSum2.SizeHpBar.Height + _sumSize.Height / 8);
                        }

                        if (hero.Value.SpellSum1 != null && hero.Value.SpellSum1.Sprite[1] != null && hero.Value.SpellSum1.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellPassive.SizeHpBar = new Size(hero.Value.SpellSum1.SizeHpBar.Width + (int)(_spellSize.Width * hpPosScale), hero.Value.SpellSum2.SizeHpBar.Height);
                            //SetScale(ref hero.Value.SpellPassive.Sprite[1].Sprite, _spellSize, scaleSum * percentScale);
                            //hero.Value.SpellPassive.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellPassive.Sprite[1].Sprite.Width) * scaleSum * percentScale,
                            //    ((float)_spellSize.Height / hero.Value.SpellPassive.Sprite[1].Sprite.Height) * scaleSum * percentScale);
                            hero.Value.SpellPassive.CoordsHpBar = new Size(hero.Value.SpellPassive.SizeHpBar.Width + (int)(_spellSize.Width / 1.7), hero.Value.SpellPassive.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.SpellQ != null && hero.Value.SpellQ.Sprite[1] != null && hero.Value.SpellQ.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellQ.SizeHpBar = new Size(hero.Value.SpellPassive.SizeHpBar.Width + (int)(_spellSize.Width * hpPosScale), hero.Value.SpellPassive.SizeHpBar.Height);
                            hero.Value.SpellQ.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellQ.Sprite[1].Sprite.Bitmap.Width) * (scaleSpell * percentScale));
                            hero.Value.SpellQ.CoordsHpBar = new Size(hero.Value.SpellQ.SizeHpBar.Width + (int)(_spellSize.Width / 1.3), hero.Value.SpellQ.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.SpellW != null && hero.Value.SpellW.Sprite[1] != null && hero.Value.SpellW.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellW.SizeHpBar = new Size(hero.Value.SpellQ.SizeHpBar.Width + (int)(_spellSize.Width * hpPosScale), hero.Value.SpellQ.SizeHpBar.Height);
                            hero.Value.SpellW.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellW.Sprite[1].Sprite.Bitmap.Width) * (scaleSpell * percentScale));
                            hero.Value.SpellW.CoordsHpBar = new Size(hero.Value.SpellW.SizeHpBar.Width + (int)(_spellSize.Width / 1.3), hero.Value.SpellW.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.SpellE != null && hero.Value.SpellE.Sprite[1] != null && hero.Value.SpellE.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellE.SizeHpBar = new Size(hero.Value.SpellW.SizeHpBar.Width + (int)(_spellSize.Width * hpPosScale), hero.Value.SpellW.SizeHpBar.Height);
                            hero.Value.SpellE.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellE.Sprite[1].Sprite.Bitmap.Width) * (scaleSpell * percentScale));
                            hero.Value.SpellE.CoordsHpBar = new Size(hero.Value.SpellE.SizeHpBar.Width + (int)(_spellSize.Width / 1.3), hero.Value.SpellE.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.SpellR != null && hero.Value.SpellR.Sprite[1] != null && hero.Value.SpellR.Sprite[1].Sprite != null)
                        {
                            hero.Value.SpellR.SizeHpBar = new Size(hero.Value.SpellE.SizeHpBar.Width + (int)(_spellSize.Width * hpPosScale), hero.Value.SpellE.SizeHpBar.Height);
                            hero.Value.SpellR.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellR.Sprite[1].Sprite.Bitmap.Width) * (scaleSpell * percentScale));
                            hero.Value.SpellR.CoordsHpBar = new Size(hero.Value.SpellR.SizeHpBar.Width + (int)(_spellSize.Width / 1.3), hero.Value.SpellR.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.Exp[0] != null && hero.Value.Exp[0].Line[1] != null && hero.Value.Exp[0].Line[1] != null)
                        {
                            hero.Value.Exp[0].SizeHpBar = new Size((int)hpPos.X + 119, (int)hpPos.Y + 17);
                            hero.Value.Exp[0].CoordsHpBar = new Size(hero.Value.SpellR.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellR.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.Exp[1] != null && hero.Value.Exp[1].Line[1] != null && hero.Value.Exp[1].Line[1] != null)
                        {
                            hero.Value.Exp[1].SizeHpBar = new Size((int)hpPos.X + 119 + _lvlIconSize.Width, (int)hpPos.Y + 17);
                            hero.Value.Exp[1].CoordsHpBar = new Size(hero.Value.SpellR.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellR.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.Exp[2] != null && hero.Value.Exp[2].Line[1] != null && hero.Value.Exp[2].Line[1] != null)
                        {
                            hero.Value.Exp[2].SizeHpBar = new Size((int)hpPos.X + 119 + _lvlIconSize.Width, (int)hpPos.Y + 17 + _lvlIconSize.Height);
                            hero.Value.Exp[2].CoordsHpBar = new Size(hero.Value.SpellR.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellR.SizeHpBar.Height + _spellSize.Height / 2);
                        }

                        if (hero.Value.Exp[3] != null && hero.Value.Exp[3].Line[1] != null && hero.Value.Exp[3].Line[1] != null)
                        {
                            hero.Value.Exp[3].SizeHpBar = new Size((int)hpPos.X + 119, (int)hpPos.Y + 17 + _lvlIconSize.Width);
                            hero.Value.Exp[3].CoordsHpBar = new Size(hero.Value.SpellR.SizeHpBar.Width + _spellSize.Width / 2, hero.Value.SpellR.SizeHpBar.Height + _spellSize.Height / 2);
                        }
                    }
                }
            }
        }

        public enum UpdateMethod
        {
            Side,
            Hp,
            MiniMap
        }

        public async static void UpdateChampImage(Obj_AI_Hero hero, Size size, SpriteHelper.SpriteInfo sprite, UpdateMethod method)
        {
            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            taskInfo = SpriteHelper.LoadTextureAsync(hero.ChampionName, sprite, SpriteHelper.DownloadType.Champion);
            sprite = await taskInfo;
            if (sprite.LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateChampImage(hero, size, sprite, method));
            }
            else
            {
                float percentScale =
                    (float)UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value / 100;
                if (method == UpdateMethod.Side)
                {
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        return new Vector2(size.Width, size.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                    };
                }
                else if (method == UpdateMethod.MiniMap)
                {
                    if (sprite.Sprite.Bitmap != null)
                        sprite.Sprite.UpdateTextureBitmap(Uim.CropImage(sprite.Sprite.Bitmap, sprite.Sprite.Width));
                    sprite.Sprite.GrayScale();
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        Vector2 serverPos = Drawing.WorldToMinimap(hero.ServerPosition);
                        var mPos = new Size((int)(serverPos[0] - 32 * 0.3f), (int)(serverPos[1] - 32 * 0.3f));
                        return new Vector2(mPos.Width, mPos.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Tracker.Trackers.GetActive() && Uim.UimTracker.GetActive() && !hero.IsVisible;
                    };
                }
                sprite.Sprite.Add();
            }
        }

        private async static void UpdateSpellImage(Obj_AI_Hero hero, Size size, SpriteHelper.SpriteInfo sprite, SpellSlot slot, UpdateMethod method)
        {
            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            switch (slot)
            {
                case SpellSlot.Q:
                    taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.Q).Name, sprite, SpriteHelper.DownloadType.Spell);
                break;

                case SpellSlot.W:
                    taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.W).Name, sprite, SpriteHelper.DownloadType.Spell);
                break;

                case SpellSlot.E:
                    taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.E).Name, sprite, SpriteHelper.DownloadType.Spell);
                break;

                case SpellSlot.R:
                    taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.R).Name, sprite, SpriteHelper.DownloadType.Spell);
                break;
            }
            if (taskInfo == null)
                return;
            sprite = await taskInfo;
            if (sprite.LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, size, sprite, slot, method));
            }
            else
            {
                float percentScale =
                    (float)UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value / 100;
                if (method == UpdateMethod.Side)
                {
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        return new Vector2(size.Width, size.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                    };
                }
                else if (method == UpdateMethod.Hp)
                {
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        return new Vector2(size.Width, size.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                    };
                }
                else if (method == UpdateMethod.MiniMap)
                {
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        return new Vector2(size.Width, size.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                    };
                }
                sprite.Sprite.Add();
            }
        }

        private async static void UpdateSummonerSpellImage(Obj_AI_Hero hero, Size size, SpriteHelper.SpriteInfo sprite, SpellSlot slot, UpdateMethod method)
        {
            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            switch (slot)
            {
                case SpellSlot.Summoner1:
                    taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.Summoner1).Name, sprite, SpriteHelper.DownloadType.Summoner);
                    break;

                case SpellSlot.Summoner2:
                    taskInfo = SpriteHelper.LoadTextureAsync(hero.Spellbook.GetSpell(SpellSlot.Summoner2).Name, sprite, SpriteHelper.DownloadType.Summoner);
                    break;
            }
            if (taskInfo == null)
                return;
            sprite = await taskInfo;
            if (sprite.LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, size, sprite, slot, method));
            }
            else
            {
                float percentScale =
                    (float)UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value / 100;
                sprite.Sprite.PositionUpdate = delegate
                {
                    return new Vector2(size.Width, size.Height);
                };
                sprite.Sprite.VisibleCondition = sender =>
                {
                    return Tracker.Trackers.GetActive() && UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                };
                sprite.Sprite.Add();
            }
        }

        private void UpdateItems(bool enemy)
        {
            //if (!Menu.UiTracker.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>())
            //    return;
            ////var loc = Assembly.GetExecutingAssembly().Location;
            ////loc = loc.Remove(loc.LastIndexOf("\\", StringComparison.Ordinal));
            ////loc = loc + "\\Sprites\\SAwareness\\";

            //Dictionary<Obj_AI_Hero, ChampInfos> heroes;

            //if (enemy)
            //{
            //    heroes = _enemies;
            //}
            //else
            //{
            //    heroes = _allies;
            //}

            //foreach (var hero in heroes)
            //{
            //    InventorySlot[] i1 = hero.Key.InventoryItems;
            //    ChampInfos champ = hero.Value;
            //    var slot = new List<int>();
            //    var unusedId = new List<int> {0, 1, 2, 3, 4, 5, 6};
            //    foreach (InventorySlot inventorySlot in i1)
            //    {
            //        slot.Add(inventorySlot.Slot);
            //        if (inventorySlot.Slot >= 0 && inventorySlot.Slot <= 6)
            //        {
            //            unusedId.Remove(inventorySlot.Slot);
            //            if (champ.Item[inventorySlot.Slot] == null)
            //                champ.Item[inventorySlot.Slot] = new ChampInfos.SpriteInfos();
            //            if (champ.Item[inventorySlot.Slot].Sprite == null ||
            //                champ.ItemId[inventorySlot.Slot] != inventorySlot.Id)
            //            {
            //                //SpriteHelper.LoadTexture(inventorySlot.Id + ".dds", "ITEMS/",
            //                //    loc + "ITEMS\\" + inventorySlot.Id + ".dds",
            //                //    ref champ.Item[inventorySlot.Slot].Texture, true);
            //                SpriteHelper.LoadTexture(inventorySlot.Id.ToString(),
            //                    ref champ.Item[inventorySlot.Slot].Sprite[0].Sprite, SpriteHelper.DownloadType.Item);
            //                if (champ.Item[inventorySlot.Slot].Sprite != null)
            //                    champ.ItemId[inventorySlot.Slot] = inventorySlot.Id;
            //            }
            //        }
            //    }

            //    for (int i = 0; i < unusedId.Count; i++)
            //    {
            //        int id = unusedId[i];
            //        champ.ItemId[id] = 0;
            //        if (champ.Item[id] == null)
            //            champ.Item[id] = new ChampInfos.SpriteInfos();
            //        champ.Item[id].Sprite = null;
            //        if ( /*id == i*/champ.Item[id].Sprite == null &&
            //                        champ.Item[id].Sprite[0].Sprite != _overlayEmptyItem)
            //        {
            //            champ.Item[id].Sprite[0].Sprite = _overlayEmptyItem;
            //        }
            //    }
            //}
        }

        private void UpdateCds(Dictionary<Obj_AI_Hero, ChampInfos> heroes)
        {
            try
            {
                UpdateItems(true);
                UpdateItems(false);

                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    foreach (var enemy in heroes)
                    {
                        if (enemy.Key == null)
                            continue;
                        enemy.Value.SHealth = ((int) enemy.Key.Health) + "/" + ((int) enemy.Key.MaxHealth);
                        enemy.Value.SMana = ((int) enemy.Key.Mana) + "/" + ((int) enemy.Key.MaxMana);
                        if (enemy.Key.NetworkId == hero.NetworkId)
                        {
                            if (enemy.Value.Item[0] != null && hero.Spellbook.GetSpell(SpellSlot.Item1) != null)
                            {
                                if (hero.Spellbook.GetSpell(SpellSlot.Item1).CooldownExpires - Game.Time > 0.0f)
                                {
                                    enemy.Value.Item[0].Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Item1).CooldownExpires - Game.Time);
                                }
                                else if (hero.Spellbook.GetSpell(SpellSlot.Item1).CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[0].Value != 0)
                                {
                                    enemy.Value.Item[0].Value = 0;
                                }
                            }
                            else if (enemy.Value.Item[1] != null && hero.Spellbook.GetSpell(SpellSlot.Item2) != null)
                            {
                                if (hero.Spellbook.GetSpell(SpellSlot.Item2).CooldownExpires - Game.Time > 0.0f)
                                {
                                    enemy.Value.Item[1].Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Item2).CooldownExpires - Game.Time);
                                }
                                else if (hero.Spellbook.GetSpell(SpellSlot.Item2).CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[1].Value != 0)
                                {
                                    enemy.Value.Item[1].Value = 0;
                                }
                            }
                            else if (enemy.Value.Item[2] != null && hero.Spellbook.GetSpell(SpellSlot.Item3) != null)
                            {
                                if (hero.Spellbook.GetSpell(SpellSlot.Item3).CooldownExpires - Game.Time > 0.0f)
                                {
                                    enemy.Value.Item[2].Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Item3).CooldownExpires - Game.Time);
                                }
                                else if (hero.Spellbook.GetSpell(SpellSlot.Item3).CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[2].Value != 0)
                                {
                                    enemy.Value.Item[2].Value = 0;
                                }
                            }
                            else if (enemy.Value.Item[3] != null && hero.Spellbook.GetSpell(SpellSlot.Item4) != null)
                            {
                                if (hero.Spellbook.GetSpell(SpellSlot.Item4).CooldownExpires - Game.Time > 0.0f)
                                {
                                    enemy.Value.Item[3].Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Item4).CooldownExpires - Game.Time);
                                }
                                else if (hero.Spellbook.GetSpell(SpellSlot.Item4).CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[3].Value != 0)
                                {
                                    enemy.Value.Item[3].Value = 0;
                                }
                            }
                            else if (enemy.Value.Item[4] != null && hero.Spellbook.GetSpell(SpellSlot.Item5) != null)
                            {
                                if (hero.Spellbook.GetSpell(SpellSlot.Item5).CooldownExpires - Game.Time > 0.0f)
                                {
                                    enemy.Value.Item[4].Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Item5).CooldownExpires - Game.Time);
                                }
                                else if (hero.Spellbook.GetSpell(SpellSlot.Item5).CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[4].Value != 0)
                                {
                                    enemy.Value.Item[4].Value = 0;
                                }
                            }
                            else if (enemy.Value.Item[5] != null && hero.Spellbook.GetSpell(SpellSlot.Item6) != null)
                            {
                                if (hero.Spellbook.GetSpell(SpellSlot.Item6).CooldownExpires - Game.Time > 0.0f)
                                {
                                    enemy.Value.Item[5].Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Item6).CooldownExpires - Game.Time);
                                }
                                else if (hero.Spellbook.GetSpell(SpellSlot.Item6).CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[5].Value != 0)
                                {
                                    enemy.Value.Item[5].Value = 0;
                                }
                            }
                            else if (enemy.Value.Item[6] != null && hero.Spellbook.GetSpell(SpellSlot.Trinket) != null)
                            {
                                if (hero.Spellbook.GetSpell(SpellSlot.Trinket).CooldownExpires - Game.Time > 0.0f)
                                {
                                    enemy.Value.Item[6].Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Trinket).CooldownExpires - Game.Time);
                                }
                                else if (hero.Spellbook.GetSpell(SpellSlot.Trinket).CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[6].Value != 0)
                                {
                                    enemy.Value.Item[6].Value = 0;
                                }
                            }

                            if (hero.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellQ.Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time);
                            }
                            else if (hero.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellQ.Value != 0)
                            {
                                enemy.Value.SpellQ.Value = 0;
                            }
                            if (hero.Spellbook.GetSpell(SpellSlot.W).CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellW.Value = (int)(hero.Spellbook.GetSpell(SpellSlot.W).CooldownExpires - Game.Time);
                            }
                            else if (hero.Spellbook.GetSpell(SpellSlot.W).CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellW.Value != 0)
                            {
                                enemy.Value.SpellW.Value = 0;
                            }
                            if (hero.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellE.Value = (int)(hero.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time);
                            }
                            else if (hero.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellE.Value != 0)
                            {
                                enemy.Value.SpellE.Value = 0;
                            }
                            if (hero.Spellbook.GetSpell(SpellSlot.R).CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellR.Value = (int)(hero.Spellbook.GetSpell(SpellSlot.R).CooldownExpires - Game.Time);
                            }
                            else if (hero.Spellbook.GetSpell(SpellSlot.R).CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellR.Value != 0)
                            {
                                enemy.Value.SpellR.Value = 0;
                            }
                            if (hero.Spellbook.GetSpell(SpellSlot.Summoner1).CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellSum1.Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Summoner1).CooldownExpires - Game.Time);
                            }
                            else if (hero.Spellbook.GetSpell(SpellSlot.Summoner1).CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellSum1.Value != 0)
                            {
                                enemy.Value.SpellSum1.Value = 0;
                            }
                            if (hero.Spellbook.GetSpell(SpellSlot.Summoner2).CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellSum2.Value = (int)(hero.Spellbook.GetSpell(SpellSlot.Summoner2).CooldownExpires - Game.Time);
                            }
                            else if (hero.Spellbook.GetSpell(SpellSlot.Summoner2).CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellSum2.Value != 0)
                            {
                                enemy.Value.SpellSum2.Value = 0;
                            }
                            if (hero.IsVisible)
                            {
                                enemy.Value.InvisibleTime = 0;
                                enemy.Value.VisibleTime = (int)Game.Time;
                            }
                            else
                            {
                                if (enemy.Value.VisibleTime != 0)
                                {
                                    enemy.Value.InvisibleTime = (int)(Game.Time - enemy.Value.VisibleTime);
                                }
                                else
                                {
                                    enemy.Value.InvisibleTime = 0;
                                }
                            }

                            //Death
                            if (hero.IsDead && !enemy.Value.Dead)
                            {
                                enemy.Value.Dead = true;
                                float temp = enemy.Key.Level * 2.5f + 5 + 2;
                                if (Math.Floor(Game.Time / 60) >= 25)
                                {
                                    enemy.Value.DeathTime = (int)(temp + ((temp / 50) * (Math.Floor(Game.Time / 60) - 25))) + (int)Game.Time;
                                }
                                else
                                {
                                    enemy.Value.DeathTime = (int)temp + (int)Game.Time;
                                }
                                if (enemy.Key.ChampionName.Contains("KogMaw"))
                                {
                                    enemy.Value.DeathTime -= 4;
                                }
                            }
                            else if (!hero.IsDead && enemy.Value.Dead)
                            {
                                enemy.Value.Dead = false;
                                enemy.Value.DeathTime = 0;
                            }
                            if (enemy.Value.DeathTime - Game.Time > 0.0f)
                            {
                                enemy.Value.DeathTimeDisplay = (int)(enemy.Value.DeathTime - Game.Time);
                            }
                            else if (enemy.Value.DeathTime - Game.Time <= 0.0f &&
                                     enemy.Value.DeathTimeDisplay != 0)
                            {
                                enemy.Value.DeathTimeDisplay = 0;
                            }
                            enemy.Value.Gold.Value = (int)enemy.Key.GoldEarned;//TODO: enable to get gold
                            enemy.Value.Cs.Value = enemy.Key.MinionsKilled;
                            enemy.Value.Level.Value = enemy.Key.Level;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UITrackerUpdate: " + ex);
                throw;
            }
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == 0xC1 || args.PacketData[0] == 0xC2)
            {
                new System.Threading.Thread(() =>
                {
                    GetGold();
                }).Start();
            }
        }

        private void GetGold()
        {
            List<Spectator.Packet> packets = new List<Spectator.Packet>();
            if(SpecUtils.GameId == null)
                return;
            List<Byte[]> fullGameBytes = SpectatorDownloader.DownloadGameFiles(SpecUtils.GameId, SpecUtils.PlatformId, SpecUtils.Key, "KeyFrame");
            foreach (Byte[] chunkBytes in fullGameBytes)
            {
                packets.AddRange(SpectatorDecoder.DecodeBytes(chunkBytes));
            }
            foreach (Spectator.Packet p in packets)
            {
                if (p.header == (Byte)Spectator.HeaderId.PlayerStats)
                {
                    Spectator.PlayerStats playerStats = new Spectator.PlayerStats(p);
                    if (playerStats.GoldEarned <= 0.0f)
                        continue;
                    foreach (var ally in _allies)
                    {
                        if (ally.Key.NetworkId == playerStats.NetId)
                        {
                            //ally.Value.Gold = playerStats.GoldEarned;
                        }
                    }
                    foreach (var enemy in _enemies)
                    {
                        if (enemy.Key.NetworkId == playerStats.NetId)
                        {
                            //enemy.Value.Gold = playerStats.GoldEarned;
                        }
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + 10 > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            UpdateCds(_enemies);
            UpdateCds(_allies);
            CalculateSizes(true);
            CalculateSizes(false);
            UpdateOHRec(_allies);
            UpdateOHRec(_enemies);
        }

        private void UpdateOHRec(Dictionary<Obj_AI_Hero, ChampInfos> heroes)
        {
            foreach (var champ in heroes)
            {
                if (champ.Value.SpellQ.Rectangle[1] != null && 
                    champ.Value.SpellQ.Sprite[0] != null && champ.Value.SpellQ.Sprite[0].Sprite != null &&
                    champ.Value.SpellQ.Sprite[1] != null && champ.Value.SpellQ.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellQ.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.Q).Level < 1)
                    {
                        champ.Value.SpellQ.Sprite[0].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellQ.Sprite[1].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellQ.Rectangle[1].Color = SharpDX.Color.Red;
                    }
                    else
                    {
                        champ.Value.SpellQ.Sprite[0].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellQ.Sprite[1].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellQ.Rectangle[1].Color = SharpDX.Color.Green;
                    }
                }
                if (champ.Value.SpellW.Rectangle[1] != null &&
                    champ.Value.SpellW.Sprite[0] != null && champ.Value.SpellW.Sprite[0].Sprite != null &&
                    champ.Value.SpellW.Sprite[1] != null && champ.Value.SpellW.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellW.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.W).Level < 1)
                    {
                        champ.Value.SpellW.Sprite[0].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellW.Sprite[1].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellW.Rectangle[1].Color = SharpDX.Color.Red;
                    }
                    else
                    {
                        champ.Value.SpellW.Sprite[0].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellW.Sprite[1].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellW.Rectangle[1].Color = SharpDX.Color.Green;
                    }
                }
                if (champ.Value.SpellE.Rectangle[1] != null &&
                    champ.Value.SpellE.Sprite[0] != null && champ.Value.SpellE.Sprite[0].Sprite != null &&
                    champ.Value.SpellE.Sprite[1] != null && champ.Value.SpellE.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellE.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.E).Level < 1)
                    {
                        champ.Value.SpellE.Sprite[0].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellE.Sprite[1].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellE.Rectangle[1].Color = SharpDX.Color.Red;
                    }
                    else
                    {
                        champ.Value.SpellE.Sprite[0].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellE.Sprite[1].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellE.Rectangle[1].Color = SharpDX.Color.Green;
                    }
                }
                if (champ.Value.SpellR.Rectangle[0] != null && champ.Value.SpellR.Rectangle[1] != null &&
                    champ.Value.SpellR.Sprite[0] != null && champ.Value.SpellR.Sprite[0].Sprite != null &&
                    champ.Value.SpellR.Sprite[1] != null && champ.Value.SpellR.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellR.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.R).Level < 1)
                    {
                        champ.Value.SpellR.Sprite[0].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellR.Sprite[1].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellR.Rectangle[0].Color = SharpDX.Color.Red;
                        champ.Value.SpellR.Rectangle[1].Color = SharpDX.Color.Red;
                    }
                    else
                    {
                        champ.Value.SpellR.Sprite[0].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellR.Sprite[1].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellR.Rectangle[0].Color = SharpDX.Color.Green;
                        champ.Value.SpellR.Rectangle[1].Color = SharpDX.Color.Green;
                    }
                }
                if (champ.Value.SpellSum1.Sprite[0] != null && champ.Value.SpellSum1.Sprite[0].Sprite != null &&
                    champ.Value.SpellSum1.Sprite[1] != null && champ.Value.SpellSum1.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellSum1.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.Summoner1).Level < 1)
                    {
                        champ.Value.SpellSum1.Sprite[0].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellSum1.Sprite[1].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                    }
                    else
                    {
                        champ.Value.SpellSum1.Sprite[0].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellSum1.Sprite[1].Sprite.Color = SharpDX.Color.White;
                    }
                }
                if (champ.Value.SpellSum2.Sprite[0] != null && champ.Value.SpellSum2.Sprite[0].Sprite != null &&
                    champ.Value.SpellSum2.Sprite[1] != null && champ.Value.SpellSum2.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellSum2.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.Summoner2).Level < 1)
                    {
                        champ.Value.SpellSum2.Sprite[0].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                        champ.Value.SpellSum2.Sprite[1].Sprite.Color = SharpDX.Color.AdjustSaturation(SharpDX.Color.Gray, 0.0f);
                    }
                    else
                    {
                        champ.Value.SpellSum2.Sprite[0].Sprite.Color = SharpDX.Color.White;
                        champ.Value.SpellSum2.Sprite[1].Sprite.Color = SharpDX.Color.White;
                    }
                }
            }
        }

        private static float CalcHpBar(Obj_AI_Hero hero)
        {
            float percent = (100/hero.MaxHealth*hero.Health);
            return (percent <= 0 || Single.IsNaN(percent) ? 0.01f : percent / 100);
        }

        private static float CalcManaBar(Obj_AI_Hero hero)
        {
            float percent = (100/hero.MaxMana*hero.Mana);
            return (percent <= 0 || Single.IsNaN(percent) ? 0.01f : percent/100);
        }

        private static float CalcRecallBar(Packet.S2C.Teleport.Struct recall)
        {
            float percent = (100f / recall.Duration * (Environment.TickCount - recall.Start));
            return (percent <= 100 ? percent/100 : 1f);
        }

        private static float CalcExpBar(Obj_AI_Hero hero, int id)
        {
            int[] Exp = { 0, 280, 660, 1140, 1720, 2400, 3180, 4060, 5040, 6120, 7300, 8580, 9960, 11440, 13020, 14700, 16480, 18360, Int32.MaxValue };
            float neededExp = Exp[hero.Level];
            float percent = (100 / (neededExp - Exp[hero.Level - 1]) * (hero.Experience - Exp[hero.Level - 1]));
            float idPercent = (percent <= 0 || Single.IsNaN(percent) ? 0.01f : percent / 100);
            switch (id)
            {
                case 0:
                    idPercent = idPercent > 0.25f ? 1 : idPercent / 0.25f;
                    break;

                case 1:
                    idPercent = idPercent > 0.50f ? 1 : (idPercent - 0.25f) / 0.25f;
                    break;

                case 2:
                    idPercent = idPercent > 0.75f ? 1 : (idPercent - 0.50f) / 0.25f;
                    break;

                case 3:
                    idPercent = idPercent > 1.0f ? 1 : (idPercent - 0.75f) / 0.25f;
                    break;

                default:
                    idPercent = 0;
                    break;
            }
            return (idPercent <= 0 || Single.IsNaN(percent) ? 0.0f : idPercent); ;
        }

        private System.Drawing.Font CalcFont(int size, float scale)
        {
            double calcSize = (int) (size*scale);
            var newSize = (int) Math.Ceiling(calcSize);
            if (newSize%2 == 0 && newSize != 0)
                return new System.Drawing.Font("Times New Roman", (int) (size*scale));
            return null;
        }

        private void CheckValidSprite(ref Sprite sprite)
        {
            if (sprite.Device != Drawing.Direct3DDevice)
            {
                sprite = new Sprite(Drawing.Direct3DDevice);
            }
        }

        private void CheckValidFont(ref Font font)
        {
            if (font.Device != Drawing.Direct3DDevice)
            {
                AssingFonts(_scalePc, true);
            }
        }

        private void AssingFonts(float percentScale, bool force = false)
        {
            //System.Drawing.Font font = CalcFont(12, percentScale);
            //if (font != null || force)
            //    _recF = new Font(Drawing.Direct3DDevice, font);
            //font = CalcFont(8, percentScale);
            //if (font != null || force)
            //    _spellF = new Font(Drawing.Direct3DDevice, font);
            //font = CalcFont(30, percentScale);
            //if (font != null || force)
            //    _champF = new Font(Drawing.Direct3DDevice, font);
            //font = CalcFont(16, percentScale);
            //if (font != null || force)
            //    _sumF = new Font(Drawing.Direct3DDevice, font);
        }

        public class ChampInfos
        {
            public SpriteInfos BackBar = new SpriteInfos();
            public SpriteInfos Champ = new SpriteInfos();
            public SpriteInfos HealthBar = new SpriteInfos();
            public SpriteInfos[] Item = new SpriteInfos[7];
            public ItemId[] ItemId = new ItemId[7];
            public SpriteInfos ManaBar = new SpriteInfos();
            public SpriteInfos RecallBar = new SpriteInfos();
            public SpriteInfos SpellPassive = new SpriteInfos();
            public SpriteInfos SpellQ = new SpriteInfos();
            public SpriteInfos SpellW = new SpriteInfos();
            public SpriteInfos SpellE = new SpriteInfos();
            public SpriteInfos SpellR = new SpriteInfos();
            public SpriteInfos SpellSum1 = new SpriteInfos();
            public SpriteInfos SpellSum2 = new SpriteInfos();
            public SpriteInfos GoldCsLvlBar = new SpriteInfos();
            public SpriteInfos Gold = new SpriteInfos();
            public SpriteInfos Level = new SpriteInfos();
            public SpriteInfos Cs = new SpriteInfos();
            public SpriteInfos[] Exp = new SpriteInfos[] { new SpriteInfos(), new SpriteInfos(), new SpriteInfos(), new SpriteInfos() };
            public int DeathTime;
            public int DeathTimeDisplay;
            public bool Dead;
            public int InvisibleTime;
            public Vector2 Pos = new Vector2();
            public String SHealth;
            public String SMana;
            public int VisibleTime;
            public Packet.S2C.Teleport.Struct RecallInfo;

            public bool Draw(bool champ)
            {
                int sprite = champ ? 0 : 1;
                if (SpellQ.Sprite[sprite] != null && SpellQ.Sprite[sprite].DownloadFinished && SpellQ.Sprite[sprite].LoadingFinished &&
                    SpellW.Sprite[sprite] != null && SpellW.Sprite[sprite].DownloadFinished && SpellW.Sprite[sprite].LoadingFinished &&
                    SpellE.Sprite[sprite] != null && SpellE.Sprite[sprite].DownloadFinished && SpellE.Sprite[sprite].LoadingFinished &&
                    SpellR.Sprite[sprite] != null && SpellR.Sprite[sprite].DownloadFinished && SpellR.Sprite[sprite].LoadingFinished &&
                    SpellSum1.Sprite[sprite] != null && SpellSum1.Sprite[sprite].DownloadFinished && SpellSum1.Sprite[sprite].LoadingFinished &&
                    SpellSum2.Sprite[sprite] != null && SpellSum2.Sprite[sprite].DownloadFinished && SpellSum2.Sprite[sprite].LoadingFinished
                    )
                {
                    if (champ && Champ.Sprite[sprite] != null && Champ.Sprite[sprite].DownloadFinished && Champ.Sprite[sprite].LoadingFinished)
                    {
                        return true;
                    }
                    else if (!champ)
                    {
                        return true;
                    }
                }
                return false;
            }

            public class SpriteInfos
            {
                public SpriteHelper.SpriteInfo[] Sprite = new SpriteHelper.SpriteInfo[10];
                public Render.Rectangle[] Rectangle = new Render.Rectangle[10];
                public Render.Text[] Text = new Render.Text[10];
                public Render.Line[] Line = new Render.Line[10];
                public int Value;
                public Size CoordsHpBar;
                public Size CoordsSideBar;
                public Size SizeHpBar;
                public Size SizeSideBar;
            }
        }
    }
}
