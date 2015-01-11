using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = SharpDX.Color;

namespace SAwareness.Trackers
{
    class Uim
    {
        public static Menu.MenuItemSettings UimTracker = new Menu.MenuItemSettings(typeof(Uim));

        private static Dictionary<Obj_AI_Hero, InternalUimTracker> _enemies = new Dictionary<Obj_AI_Hero, InternalUimTracker>();
        private int lastGameUpdateTime = 0;

        public Uim()
        {
            if (!IsActive())
                return;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    InternalUimTracker champ = new InternalUimTracker(hero);
                    champ.RecallInfo = new Packet.S2C.Teleport.Struct(hero.NetworkId, Packet.S2C.Teleport.Status.Unknown, Packet.S2C.Teleport.Type.Unknown, 0, 0);
                    champ = LoadTexts(champ);
                    _enemies.Add(hero, champ);
                }
            }

            new System.Threading.Thread(LoadSprites).Start();
            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~Uim()
        {
            Obj_AI_Base.OnTeleport -= Obj_AI_Base_OnTeleport;
            _enemies = null;
        }

        public bool IsActive()
        {
            return Tracker.Trackers.GetActive() && UimTracker.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            UimTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_UIM_MAIN"), "SAwarenessTrackersUim"));
            UimTracker.MenuItems.Add(
                UimTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersUimScale", Language.GetString("TRACKERS_UIM_SCALE")).SetValue(new Slider(100, 100, 0))));
            UimTracker.MenuItems.Add(
                UimTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersUimShowSS", Language.GetString("TRACKERS_UIM_TIME")).SetValue(false)));
            UimTracker.MenuItems.Add(
                UimTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersUimActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return UimTracker;
        }

        InternalUimTracker LoadTexts(InternalUimTracker champ)
        {
            champ.Text = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.Text.TextUpdate = delegate
            {
                if (champ.RecallInfo.Start != 0)
                {
                    float time = Environment.TickCount + champ.RecallInfo.Duration - champ.RecallInfo.Start;
                    if (time > 0.0f &&
                        (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Start))
                    {
                        return "Recalling";
                    }
                }
                if (champ.Timer.InvisibleTime != 0)
                {
                    return champ.Timer.InvisibleTime.ToString();
                }
                return "";
            };
            champ.Text.PositionUpdate = delegate
            {
                return Drawing.WorldToMinimap(champ.LastPosition);
            };
            champ.Text.VisibleCondition = sender =>
            {
                bool recall = false;

                if (champ.RecallInfo.Start != 0)
                {
                    float time = Environment.TickCount + champ.RecallInfo.Duration - champ.RecallInfo.Start;
                    if (time > 0.0f &&
                        (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Start))
                    {
                        recall = true;
                    }
                }
                return Tracker.Trackers.GetActive() && UimTracker.GetActive() && recall || champ.Timer.InvisibleTime != 0;
            };
            champ.Text.OutLined = true;
            champ.Text.Centered = true;
            champ.Text.Add(4);
            return champ;
        }

        void LoadSprites()
        {
            foreach (var enemy in _enemies)
            {
                SpriteHelper.DownloadImageRiot(enemy.Key.ChampionName, SpriteHelper.DownloadType.Champion, "UIM");
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            float percentScale = (float)UimTracker.GetMenuItem("SAwarenessTrackersUimScale").GetValue<Slider>().Value / 100;
            foreach (var enemy in _enemies)
            {
                if (enemy.Key.IsVisible)
                {
                    enemy.Value.LastPosition = enemy.Key.ServerPosition;
                }
                if (enemy.Value.SpriteInfo == null)
                {
                    SpriteHelper.LoadTexture(enemy.Key.ChampionName, ref enemy.Value.SpriteInfo, "UIM");
                }
                if (enemy.Value.SpriteInfo != null && enemy.Value.SpriteInfo.DownloadFinished && !enemy.Value.SpriteInfo.LoadingFinished)
                {
                    enemy.Value.SpriteInfo.Sprite.GrayScale();
                    enemy.Value.SpriteInfo.Sprite.UpdateTextureBitmap(CropImage(enemy.Value.SpriteInfo.Sprite.Bitmap, enemy.Value.SpriteInfo.Sprite.Width));
                    enemy.Value.SpriteInfo.Sprite.Scale = new Vector2(((float)24 / enemy.Value.SpriteInfo.Sprite.Width) * percentScale, ((float)24 / enemy.Value.SpriteInfo.Sprite.Height) * percentScale);
                    enemy.Value.SpriteInfo.Sprite.PositionUpdate = delegate
                    {
                        Vector2 serverPos = Drawing.WorldToMinimap(enemy.Value.LastPosition);
                        var mPos = new Vector2((int)(serverPos[0] - 32 * 0.3f), (int)(serverPos[1] - 32 * 0.3f));
                        return new Vector2(mPos.X, mPos.Y);
                    };
                    enemy.Value.SpriteInfo.Sprite.VisibleCondition = delegate
                    {
                        return Tracker.Trackers.GetActive() && UimTracker.GetActive() && !enemy.Key.IsVisible;
                    };
                    enemy.Value.SpriteInfo.Sprite.Add(1);
                    enemy.Value.SpriteInfo.LoadingFinished = true;
                }
            }
        }

        void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            Packet.S2C.Teleport.Struct decoded = Packet.S2C.Teleport.Decoded(sender, args);
            foreach (var enemy in _enemies)
            {
                if (enemy.Value.RecallInfo.UnitNetworkId == decoded.UnitNetworkId)
                {
                    enemy.Value.RecallInfo = decoded;
                    if (decoded.Status == Packet.S2C.Teleport.Status.Finish)
                    {
                        Vector3 spawnPos = ObjectManager.Get<GameObject>().First(spawnPoint => spawnPoint is Obj_SpawnPoint &&
                                spawnPoint.Team != ObjectManager.Player.Team).Position;
                        enemy.Value.LastPosition = spawnPos;
                    }
                }
            }
        }

        //public async static Task Init()
        //{
        //    foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
        //    {
        //        if (hero.IsEnemy)
        //        {
        //            InternalUimTracker champ = new InternalUimTracker(hero);

        //            Task<InternalUimTracker> champInfos = CreateImage(hero, champ);
        //            champ = await champInfos;
        //            champ.RecallInfo = new Packet.S2C.Teleport.Struct(hero.NetworkId, Packet.S2C.Teleport.Status.Unknown, Packet.S2C.Teleport.Type.Unknown, 0, 0);

        //            _enemies.Add(hero, champ);
        //        }
        //    }
        //}

        //private async static Task<InternalUimTracker> CreateImage(Obj_AI_Hero hero, InternalUimTracker champ)
        //{
        //    float percentScale =
        //            (float)UimTracker.GetMenuItem("SAwarenessTrackersUimScale").GetValue<Slider>().Value / 100;
        //    Task<SpriteHelper.SpriteInfo> taskInfo = SpriteHelper.LoadTextureAsync(
        //        hero.ChampionName, champ.SpriteInfo, SpriteHelper.DownloadType.Champion);
        //    champ.SpriteInfo = await taskInfo;
        //    if (!champ.SpriteInfo.LoadingFinished)
        //    {
        //        Utility.DelayAction.Add(5000, () => Ui.UpdateChampImage(hero, new Size(champ.SpriteInfo.Sprite.Width, champ.SpriteInfo.Sprite.Width), champ.SpriteInfo, Ui.UpdateMethod.MiniMap));
        //    }
        //    else
        //    {
        //        if (champ.SpriteInfo.Sprite.Bitmap != null)
        //            champ.SpriteInfo.Sprite.UpdateTextureBitmap(CropImage(champ.SpriteInfo.Sprite.Bitmap, champ.SpriteInfo.Sprite.Width));
        //        champ.SpriteInfo.Sprite.GrayScale();
        //        champ.SpriteInfo.Sprite.Scale = new Vector2(((float)24 / champ.SpriteInfo.Sprite.Width) * percentScale, ((float)24 / champ.SpriteInfo.Sprite.Height) * percentScale);
        //        champ.SpriteInfo.Sprite.PositionUpdate = delegate
        //        {
        //            Vector2 serverPos = Drawing.WorldToMinimap(champ.LastPosition);
        //            var mPos = new Vector2((int)(serverPos[0] - 32 * 0.3f), (int)(serverPos[1] - 32 * 0.3f));
        //            return new Vector2(mPos.X, mPos.Y);
        //        };
        //        champ.SpriteInfo.Sprite.VisibleCondition = delegate
        //        {
        //            return Tracker.Trackers.GetActive() && UimTracker.GetActive() && !hero.IsVisible;
        //        };
        //        champ.SpriteInfo.Sprite.Add();
        //    }

        //    champ.Text = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
        //    champ.Text.TextUpdate = delegate
        //    {
        //        if (champ.RecallInfo.Start != 0)
        //        {
        //            float time = Environment.TickCount + champ.RecallInfo.Duration - champ.RecallInfo.Start;
        //            if (time > 0.0f &&
        //                (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Start))
        //            {
        //                return "Recalling";
        //            }
        //        }
        //        if (champ.Timer.InvisibleTime != 0)
        //        {
        //            return champ.Timer.InvisibleTime.ToString();
        //        }
        //        return "";
        //    };
        //    champ.Text.PositionUpdate = delegate
        //    {
        //        return Drawing.WorldToMinimap(champ.LastPosition);
        //    };
        //    champ.Text.VisibleCondition = sender =>
        //    {
        //        bool recall = false;

        //        if (champ.RecallInfo.Start != 0)
        //        {
        //            float time = Environment.TickCount + champ.RecallInfo.Duration - champ.RecallInfo.Start;
        //            if (time > 0.0f &&
        //                (champ.RecallInfo.Status == Packet.S2C.Teleport.Status.Start))
        //            {
        //                recall = true;
        //            }
        //        }
        //        return Tracker.Trackers.GetActive() && UimTracker.GetActive() && recall || champ.Timer.InvisibleTime != 0;
        //    };
        //    champ.Text.OutLined = true;
        //    champ.Text.Centered = true;
        //    champ.Text.Add();

        //    return champ;
        //}

        public static Bitmap CropImage(Bitmap srcBitmap, int imageWidth)
        {
            Bitmap finalImage = new Bitmap(imageWidth, imageWidth);
            System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(0, 0,
                imageWidth, imageWidth);

            using (Bitmap sourceImage = srcBitmap)
            using (Bitmap croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
            using (TextureBrush tb = new TextureBrush(croppedImage))
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.FillEllipse(tb, 0, 0, imageWidth, imageWidth);
                Pen p = new Pen(System.Drawing.Color.Black, 10) { Alignment = PenAlignment.Inset };
                g.DrawEllipse(p, 0, 0, imageWidth, imageWidth);
            }
            return finalImage;
        }

        class InternalUimTracker
        {
            public Obj_AI_Hero Hero;
            public SpriteHelper.SpriteInfo SpriteInfo;
            public Render.Text Text;
            public Packet.S2C.Teleport.Struct RecallInfo;
            public SsTimer Timer;
            public Vector3 LastPosition;

            public InternalUimTracker(Obj_AI_Hero hero)
            {
                Hero = hero;
                Timer = new SsTimer(Hero);
            }
        }

        class SsTimer
        {
            public int InvisibleTime;
            public int VisibleTime;
            public Obj_AI_Hero Hero;

            public SsTimer(Obj_AI_Hero hero)
            {
                Hero = hero;
                Game.OnGameUpdate += Game_OnGameUpdate;
            }

            ~SsTimer()
            {
                Game.OnGameUpdate -= Game_OnGameUpdate;
                Hero = null;
            }

            private void Game_OnGameUpdate(EventArgs args)
            {
                if (Hero.IsVisible)
                {
                    InvisibleTime = 0;
                    VisibleTime = (int)Game.Time;
                }
                else
                {
                    if (VisibleTime != 0)
                    {
                        InvisibleTime = (int)(Game.Time - VisibleTime);
                    }
                    else
                    {
                        InvisibleTime = 0;
                    }
                }
            }
        }
    }
}
