using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Sandbox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAssemblies;
using SAssemblies.Miscs;
using SharpDX;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;
using Menu = SAssemblies.Menu;

//add switching items
//add total costs
//add start and main items

namespace SAssemblies.Miscs
{
    class AutoBuy
    {

        public static Menu.MenuItemSettings AutoBuyMisc = new Menu.MenuItemSettings(typeof(AutoBuy));

        private int lastGameUpdateTime = 0;
        private static List<BuildLevler> builds = new List<BuildLevler>();
        private AutoBuyGUI Gui = new AutoBuyGUI();
        private bool shiftActive = false;
        private bool boughtStartItems = false;

        public AutoBuy()
        {
            //LoadLevelFile();
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyLoadChoice").ValueChanged += ChangeBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").ValueChanged += ShowBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").ValueChanged += NewBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyDeleteBuild").ValueChanged += DeleteBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyActive").ValueChanged += Active_OnValueChanged;

            GameUpdate a = null;
            a = delegate(EventArgs args)
            {
                LolBuilder.GetLolBuilderData();
                Game.OnUpdate -= a;
            };
            Game.OnUpdate += a;

            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Drawing.OnDraw += Drawing_OnDraw;
        }

        void Drawing_OnDraw(EventArgs args)
        {
            for (int i = 0; i < AutoBuyGUI.CurrentBuilder.ItemIdList.Count; i++)
            {
                Drawing.DrawText(1400, 800 + (20 * i), System.Drawing.Color.Aqua, AutoBuyGUI.CurrentBuilder.ItemIdList[i].Type.ToString() + ":" + AutoBuyGUI.CurrentBuilder.ItemIdList[i].ItemId.ToString());
            }
        }

        ~AutoBuy()
        {
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyLoadChoice").ValueChanged -= ChangeBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").ValueChanged -= ShowBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").ValueChanged -= NewBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyDeleteBuild").ValueChanged -= DeleteBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyActive").ValueChanged -= Active_OnValueChanged;

            Game.OnUpdate -= Game_OnGameUpdate;
            Game.OnWndProc -= Game_OnWndProc;
            builds = null;
        }

        public static bool IsActive()
        {
#if MISCS
            return Misc.Miscs.GetActive() && AutoBuyMisc.GetActive();
#else
            return AutoBuyMisc.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            LoadBuildFile();
            AutoBuyMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_AUTOBUY_MAIN"), "SAssembliesMiscsAutoBuy"));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyLoadChoice", Language.GetString("MISCS_AUTOBUY_BUILD_CHOICE"))
                        .SetValue(GetBuildNames())
                            .DontSave()));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyShowBuild", Language.GetString("MISCS_AUTOBUY_BUILD_LOAD")).SetValue(false)
                        .DontSave()));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyNewBuild", Language.GetString("MISCS_AUTOBUY_CREATE_BUILD")).SetValue(false)
                        .DontSave()));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyDeleteBuild", Language.GetString("MISCS_AUTOBUY_DELETE_BUILD")).SetValue(false)
                        .DontSave()));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false).DontSave()));
            return AutoBuyMisc;
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //WriteBuildFile();
        }

        void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyLoadChoice").ValueChanged -= ChangeBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").ValueChanged -= ShowBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").ValueChanged -= NewBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyDeleteBuild").ValueChanged -= DeleteBuild_OnValueChanged;
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyActive").ValueChanged -= Active_OnValueChanged;

            foreach (var sprite in AutoBuyGUI.BuildFrame.ItemBuildSpritesStart)
            {
                if (sprite != null && sprite.Sprite != null)
                {
                    sprite.Sprite.Dispose();
                }
            }

            foreach (var sprite in AutoBuyGUI.BuildFrame.ItemBuildSpritesSummary)
            {
                if (sprite != null && sprite.Sprite != null)
                {
                    sprite.Sprite.Dispose();
                }
            }

            foreach (var sprite in AutoBuyGUI.ShopFrame.ItemShopSprites)
            {
                if (sprite != null && sprite.Sprite != null)
                {
                    sprite.Sprite.Dispose();
                }
            }

            foreach (var frame in AutoBuyGUI.ItemFrames)
            {
                frame.Value.Sprite.Dispose();
            }

            Game.OnUpdate -= Game_OnGameUpdate;
            Game.OnWndProc -= Game_OnWndProc;
            //WriteBuildFile();
            builds = null;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (!IsActive() &&
                            (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").GetValue<bool>() ||
                            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").GetValue<bool>()))
                return;
            HandleInput((WindowsMessages)args.Msg, Utils.GetCursorPos(), args.WParam);
        }

        private void HandleInput(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message == WindowsMessages.WM_KEYDOWN && key == 16)
            {
                shiftActive = true;
            }
            if (message == WindowsMessages.WM_KEYUP && key == 16)
            {
                shiftActive = false;
            }
            if (AutoBuyGUI.ShopFrame.ShopSprite == null || AutoBuyGUI.ShopFrame.ShopSprite.Sprite == null || !AutoBuyGUI.ShopFrame.ShopSprite.Sprite.Visible ||
                AutoBuyGUI.BuildFrame.BuildSprite == null || AutoBuyGUI.BuildFrame.BuildSprite.Sprite == null || !AutoBuyGUI.BuildFrame.BuildSprite.Sprite.Visible)
            {
                return;
            }
            HandleShopFrameMove(message, cursorPos, key);
            HandleBuildFrameMove(message, cursorPos, key);
            HandleShopFrameClick(message, cursorPos, key);
            HandleBuildFrameClick(message, cursorPos, key);
        }

        private void HandleShopFrameMove(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_MOUSEMOVE)
            {
                return;
            }
            foreach (var spriteInfo in AutoBuyGUI.ShopFrame.ItemShopSprites)
            {
                if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                {
                    continue;
                }
                if (Common.IsInside(
                    cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                    spriteInfo.Sprite.Sprite.Height))
                {
                    AutoBuyGUI.ShopFrame.Rectangle.Width = spriteInfo.Text.TextLength.Width;
                    AutoBuyGUI.ShopFrame.Rectangle.Height = spriteInfo.Text.TextLength.Height;
                    AutoBuyGUI.ShopFrame.Rectangle.Visible = true;
                    spriteInfo.Text.Text.Visible = true;
                    break;
                }
                else
                {
                    spriteInfo.Text.Text.Visible = false;
                    AutoBuyGUI.ShopFrame.Rectangle.Visible = false;
                }
            }
        }

        private void HandleBuildFrameMove(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_MOUSEMOVE)
            {
                return;
            }
            foreach (var spriteInfo in AutoBuyGUI.BuildFrame.ItemBuildSpritesStart)
            {
                if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                {
                    continue;
                }
                if (Common.IsInside(
                    cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                    spriteInfo.Sprite.Sprite.Height))
                {
                    AutoBuyGUI.BuildFrame.Rectangle.X = spriteInfo.Text.Text.X;
                    AutoBuyGUI.BuildFrame.Rectangle.Y = spriteInfo.Text.Text.Y;
                    AutoBuyGUI.BuildFrame.Rectangle.Width = spriteInfo.Text.TextLength.Width;
                    AutoBuyGUI.BuildFrame.Rectangle.Height = spriteInfo.Text.TextLength.Height;
                    AutoBuyGUI.BuildFrame.Rectangle.Visible = true;
                    spriteInfo.Text.Text.Visible = true;
                    return;
                }
                else
                {
                    spriteInfo.Text.Text.Visible = false;
                    AutoBuyGUI.BuildFrame.Rectangle.Visible = false;
                }
            }

            foreach (var spriteInfo in AutoBuyGUI.BuildFrame.ItemBuildSpritesSummary)
            {
                if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                {
                    continue;
                }
                if (Common.IsInside(
                    cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                    spriteInfo.Sprite.Sprite.Height))
                {
                    AutoBuyGUI.BuildFrame.Rectangle.X = spriteInfo.Text.Text.X;
                    AutoBuyGUI.BuildFrame.Rectangle.Y = spriteInfo.Text.Text.Y;
                    AutoBuyGUI.BuildFrame.Rectangle.Width = spriteInfo.Text.TextLength.Width;
                    AutoBuyGUI.BuildFrame.Rectangle.Height = spriteInfo.Text.TextLength.Height;
                    AutoBuyGUI.BuildFrame.Rectangle.Visible = true;
                    spriteInfo.Text.Text.Visible = true;
                    break;
                }
                else
                {
                    spriteInfo.Text.Text.Visible = false;
                    AutoBuyGUI.BuildFrame.Rectangle.Visible = false;
                }
            }
        }

        private void HandleShopFrameClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_LBUTTONUP)
            {
                return;
            }
            HandleShopFrameItemClick(message, cursorPos, key);
            HandleShopFrameCategoryClick(message, cursorPos, key);
            HandleShopFrameExitClick(message, cursorPos, key);
            HandleShopFrameStartFinalClick(message, cursorPos, key);
        }

        private void HandleShopFrameItemClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            foreach (var spriteInfo in AutoBuyGUI.ShopFrame.ItemShopSprites)
            {
                if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                {
                    continue;
                }
                if (Common.IsInside(
                    cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                    spriteInfo.Sprite.Sprite.Height))
                {
                    if (AutoBuyGUI.BuildFrame.LastClickedElementId != -1)
                    {
                        //if (AutoBuyGUI.BuildFrame.AddBuildSprite(spriteInfo, AutoBuyGUI.BuildFrame.LastClickedElementId))
                        //{
                        //    AutoBuyGUI.CurrentBuilder.ItemIdList.Insert(AutoBuyGUI.BuildFrame.LastClickedElementId, 
                        //        new BuildLevler.BuildItem(spriteInfo.Item.ItemId, AutoBuyGUI.ShopFrame.StartItemActive
                        //        ? BuildLevler.BuildItem.ItemType.Start : BuildLevler.BuildItem.ItemType.Summary));
                        //}
                        //else
                        //{
                        //    int lastId = AutoBuyGUI.CurrentBuilder.ItemIdList.Count;
                        //    var lastBId = AutoBuyGUI.ShopFrame.StartItemActive ?
                        //        AutoBuyGUI.CurrentBuilder.ItemIdList.LastOrDefault(
                        //        x => x.Type == BuildLevler.BuildItem.ItemType.Start) :
                        //        AutoBuyGUI.CurrentBuilder.ItemIdList.LastOrDefault(
                        //        x => x.Type == BuildLevler.BuildItem.ItemType.Summary);
                        //    if (lastBId != null)
                        //    {
                        //        lastId = AutoBuyGUI.CurrentBuilder.ItemIdList.IndexOf(lastBId) + 1;
                        //    }
                            
                        //    AutoBuyGUI.CurrentBuilder.ItemIdList.Insert(lastId, new BuildLevler.BuildItem(spriteInfo.Item.ItemId, AutoBuyGUI.ShopFrame.StartItemActive
                        //        ? BuildLevler.BuildItem.ItemType.Start : BuildLevler.BuildItem.ItemType.Summary));
                        //}
                        AutoBuyGUI.CurrentBuilder.ItemIdList.Insert(AutoBuyGUI.BuildFrame.LastClickedElementId,
                            new BuildLevler.BuildItem(spriteInfo.Item.ItemId, AutoBuyGUI.ShopFrame.StartItemActive
                            ? BuildLevler.BuildItem.ItemType.Start : BuildLevler.BuildItem.ItemType.Summary));

                        AutoBuyGUI.BuildFrame.LastClickedElementId = -1;
                    }
                    else
                    {
                        //AutoBuyGUI.BuildFrame.AddBuildSprite(spriteInfo);
                        int lastId = AutoBuyGUI.CurrentBuilder.ItemIdList.Count;
                        var lastBId = AutoBuyGUI.ShopFrame.StartItemActive ?
                            AutoBuyGUI.CurrentBuilder.ItemIdList.LastOrDefault(
                            x => x.Type == BuildLevler.BuildItem.ItemType.Start) :
                            AutoBuyGUI.CurrentBuilder.ItemIdList.LastOrDefault(
                            x => x.Type == BuildLevler.BuildItem.ItemType.Summary);
                        if (lastBId != null)
                        {
                            lastId = AutoBuyGUI.CurrentBuilder.ItemIdList.IndexOf(lastBId) + 1;
                        }

                        AutoBuyGUI.CurrentBuilder.ItemIdList.Insert(lastId, new BuildLevler.BuildItem(spriteInfo.Item.ItemId, AutoBuyGUI.ShopFrame.StartItemActive
                            ? BuildLevler.BuildItem.ItemType.Start : BuildLevler.BuildItem.ItemType.Summary));
                    }
                    AutoBuyGUI.BuildFrame.CreateBuildSprites(AutoBuyGUI.CurrentBuilder);
                    break;
                }
            }
        }

        private void HandleShopFrameCategoryClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            for (int i = 0; i < 29; i++)
            {
                if (AutoBuyGUI.ShopFrame.ShopSprite == null || AutoBuyGUI.ShopFrame.ShopSprite.Sprite == null)
                {
                    continue;
                }
                if (Common.IsInside(
                    cursorPos, AutoBuyGUI.ShopFrame.ShopSprite.Sprite.Position + new Vector2(15, 120 + (17.5f * i)), 190, 17.5f))
                {
                    AutoBuyGUI.ShopFrame.CreateShopSprites((ItemInfo.ItemType) i + 1);
                    break;
                }
            }
        }

        private void HandleShopFrameExitClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (Common.IsInside(
                    cursorPos, AutoBuyGUI.ShopFrame.ShopSprite.Sprite.Position + new Vector2(970f, 10f), 40f, 40f))
            {
                ResetMenuEntries();
            }
        }

        private void HandleShopFrameStartFinalClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (Common.IsInside(
                    cursorPos, AutoBuyGUI.ShopFrame.ShopSprite.Sprite.Position + AutoBuyGUI.ShopFrame.StartItemsStart, AutoBuyGUI.ShopFrame.StartFinalItemsIncrement.X, AutoBuyGUI.ShopFrame.StartFinalItemsIncrement.Y))
            {
                if (!AutoBuyGUI.ShopFrame.StartItemActive)
                {
                    AutoBuyGUI.ShopFrame.StartItemActive = true;
                    AutoBuyGUI.BuildFrame.LastClickedElementId = -1; 
                }
            }
            if (Common.IsInside(
                    cursorPos, AutoBuyGUI.ShopFrame.ShopSprite.Sprite.Position + AutoBuyGUI.ShopFrame.FinalItemsStart, AutoBuyGUI.ShopFrame.StartFinalItemsIncrement.X, AutoBuyGUI.ShopFrame.StartFinalItemsIncrement.Y))
            {
                if (AutoBuyGUI.ShopFrame.StartItemActive)
                {
                    AutoBuyGUI.ShopFrame.StartItemActive = false;
                    AutoBuyGUI.BuildFrame.LastClickedElementId = -1;
                }
            }
        }

        private void HandleBuildFrameClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_LBUTTONUP)
            {
                return;
            }
            if (!shiftActive)
            {
                foreach (var spriteInfo in AutoBuyGUI.BuildFrame.ItemBuildSpritesStart)
                {
                    if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                    {
                        continue;
                    }
                    if (Common.IsInside(
                        cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                        spriteInfo.Sprite.Sprite.Height))
                    {
                        AutoBuyGUI.CurrentBuilder.ItemIdList.RemoveAt(AutoBuyGUI.BuildFrame.DeleteBuildSprite(spriteInfo));
                        AutoBuyGUI.BuildFrame.CreateBuildSprites(AutoBuyGUI.CurrentBuilder);
                        break;
                    }
                }
                foreach (var spriteInfo in AutoBuyGUI.BuildFrame.ItemBuildSpritesSummary)
                {
                    if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                    {
                        continue;
                    }
                    if (Common.IsInside(
                        cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                        spriteInfo.Sprite.Sprite.Height))
                    {
                        AutoBuyGUI.CurrentBuilder.ItemIdList.RemoveAt(AutoBuyGUI.BuildFrame.DeleteBuildSprite(spriteInfo));
                        AutoBuyGUI.BuildFrame.CreateBuildSprites(AutoBuyGUI.CurrentBuilder);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < AutoBuyGUI.BuildFrame.ItemBuildSpritesStart.Count; i++)
                {
                    if (!AutoBuyGUI.ShopFrame.StartItemActive)
                    {
                        break;
                    }
                    var spriteInfo = AutoBuyGUI.BuildFrame.ItemBuildSpritesStart[i];
                    if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                    {
                        continue;
                    }
                    if (Common.IsInside(
                        cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                        spriteInfo.Sprite.Sprite.Height))
                    {
                        AutoBuyGUI.BuildFrame.LastClickedElementId = i;
                        break;
                    }
                }
                for (int i = 0; i < AutoBuyGUI.BuildFrame.ItemBuildSpritesSummary.Count; i++)
                {
                    if (AutoBuyGUI.ShopFrame.StartItemActive)
                    {
                        break;
                    }
                    var spriteInfo = AutoBuyGUI.BuildFrame.ItemBuildSpritesSummary[i];
                    if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                    {
                        continue;
                    }
                    if (Common.IsInside(
                        cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                        spriteInfo.Sprite.Sprite.Height))
                    {
                        AutoBuyGUI.BuildFrame.LastClickedElementId = AutoBuyGUI.BuildFrame.ItemBuildSpritesStart.Count + i;
                        break;
                    }
                }
            }
        }

        private void ResetMenuEntries()
        {
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild")
                .SetValue(false);
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild")
                .SetValue(false);
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyDeleteBuild")
                .SetValue(false);
        }

        private void ChangeBuild_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild")
                .GetValue<bool>() || AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild")
                .GetValue<bool>() || AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyDeleteBuild")
                .GetValue<bool>())
            {
                onValueChangeEventArgs.Process = false;
                return;
            }

            StringList list = onValueChangeEventArgs.GetNewValue<StringList>();
            BuildLevler curLevler = null;
            foreach (BuildLevler levler in builds.ToArray())
            {
                if (levler.Name.Contains(list.SList[list.SelectedIndex]))
                {
                    curLevler = levler;
                }
            }
            if (curLevler != null)
            {
                AutoBuyGUI.CurrentBuilder = new BuildLevler(curLevler.Name, curLevler.ItemIdList, Game.MapId);
                AutoBuyGUI.BuildFrame.CreateBuildSprites(AutoBuyGUI.CurrentBuilder);
            }
            else
            {
                AutoBuyGUI.CurrentBuilder = new BuildLevler();
                AutoBuyGUI.BuildFrame.CreateBuildSprites(AutoBuyGUI.CurrentBuilder);
            }
        }

        private void ShowBuild_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild")
                .GetValue<bool>() || AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyDeleteBuild")
                .GetValue<bool>())
            {
                onValueChangeEventArgs.Process = false;
                return;
            }

            if (onValueChangeEventArgs.GetNewValue<bool>())
            {
                StringList list =
                    AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyLoadChoice")
                        .GetValue<StringList>();
                BuildLevler curBuilder = null;
                foreach (BuildLevler levler in builds.ToArray())
                {
                    if (list.SList[list.SelectedIndex].Equals(""))
                        continue;
                    if (levler.Name.Contains(list.SList[list.SelectedIndex]))
                    {
                        curBuilder = levler;
                        break;
                    }
                }
                if (curBuilder != null)
                {
                    AutoBuyGUI.CurrentBuilder = new BuildLevler(curBuilder.Name, curBuilder.ItemIdList, Game.MapId);
                    AutoBuyGUI.BuildFrame.CreateBuildSprites(AutoBuyGUI.CurrentBuilder);
                }
                else
                {
                    onValueChangeEventArgs.Process = false;
                    //SequenceLevlerGUI.CurrentLevler = new SequenceLevler();
                }
                //gui.CurrentLevler = curLevler ?? new SequenceLevler();
            }
        }

        private void NewBuild_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild")
                .GetValue<bool>() || AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyDeleteBuild")
                .GetValue<bool>())
            {
                onValueChangeEventArgs.Process = false;
                return;
            }

            if (onValueChangeEventArgs.GetNewValue<bool>())
            {
                AutoBuyGUI.CurrentBuilder = new BuildLevler();
                AutoBuyGUI.CurrentBuilder.Name = GetFreeSequenceName();
                AutoBuyGUI.CurrentBuilder.ChampionName = ObjectManager.Player.ChampionName;
                AutoBuyGUI.BuildFrame.CreateBuildSprites(AutoBuyGUI.CurrentBuilder);
            }
        }

        private void DeleteBuild_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild")
                .GetValue<bool>() || AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild")
                .GetValue<bool>())
            {
                onValueChangeEventArgs.Process = false;
                return;
            }

            if (onValueChangeEventArgs.GetNewValue<bool>())
            {
                DeleteSequence();
                AutoBuyGUI.CurrentBuilder = new BuildLevler();
                AutoBuyGUI.BuildFrame.CreateBuildSprites(AutoBuyGUI.CurrentBuilder);
                onValueChangeEventArgs.Process = false;
            }
        }

        private void Active_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            //if (!onValueChangeEventArgs.GetNewValue<bool>())
            //{
            //    WriteLevelFile();
            //}
        }

        private static void LoadBuildFile()
        {
            string loc = Path.Combine(new[]
            {
                SandboxConfig.DataDirectory, "Assemblies", "cache",
                "SAssemblies", "AutoBuilder", "autobuild.conf"
            });
            try
            {
                builds = JsonConvert.DeserializeObject<List<BuildLevler>>(File.ReadAllText(loc));
            }
            catch (Exception)
            {
                //Console.WriteLine("Couldn't load autolevel.conf.");
            }
        }

        private static void WriteBuildFile()
        {
            string loc = Path.Combine(new[]
            {
                SandboxConfig.DataDirectory, "Assemblies", "cache",
                "SAssemblies", "AutoBuilder", "autobuild.conf"
            });
            try
            {
                String output = JsonConvert.SerializeObject(builds.Where(x => !x.Web));
                Directory.CreateDirectory(
                    Path.Combine(SandboxConfig.DataDirectory, "Assemblies", "cache", "SAssemblies", "AutoBuilder"));
                if (output.Contains("[]"))
                {
                    throw new Exception("[], your latest changes are not getting saved!");
                }
                else
                {
                    File.WriteAllText(loc, output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't save autolevel.conf. Ex; {0}", ex);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            if (AutoBuyGUI.CurrentBuilder == null || AutoBuyGUI.CurrentBuilder.ItemIdList.Count == 0)
            {
                return;
            }

            var itemList = ObjectManager.Player.InventoryItems.ToList();
            var summaryItemList = AutoBuyGUI.CurrentBuilder.ItemIdList.Where(x => x.Type == BuildLevler.BuildItem.ItemType.Summary).ToList();
            foreach (var item in summaryItemList)
            {
                if (ObjectManager.Player.InventoryItems.Any(x => x.Id == item.ItemId))
                {
                    boughtStartItems = true;
                }
            }
            
            if (!boughtStartItems)
            {
                var startItemList = AutoBuyGUI.CurrentBuilder.ItemIdList.Where(x => x.Type == BuildLevler.BuildItem.ItemType.Start).ToList();
                foreach (var item in itemList)
                {
                    var nItem = startItemList.FirstOrDefault(x => x.ItemId == item.Id);
                    if (nItem != null)
                    {
                        if (item.Stacks > 1)
                        {
                            for (int i = 0; i < item.Stacks; i++)
                            {
                                var internItem = startItemList.FirstOrDefault(x => x.ItemId == item.Id);
                                startItemList.Remove(internItem);
                            }
                        }
                        else
                        {
                            startItemList.Remove(nItem);
                        }
                        
                        Console.WriteLine("Removing: {0}; Times: {1}", nItem.ItemId, item.Stacks);
                    }
                }
                if (startItemList.Count == 0)
                {
                    boughtStartItems = true;
                }
                else
                {
                    Console.WriteLine(startItemList.Count);
                    foreach (var item in startItemList)
                    {
                        ObjectManager.Player.BuyItem(item.ItemId);
                        Console.WriteLine(item.ItemId);
                    }
                }
            }

            foreach (var item in itemList)
            {
                var nItem = summaryItemList.FirstOrDefault(x => x.ItemId == item.Id);
                if (nItem != null)
                {
                    if (item.Stacks > 1)
                    {
                        for (int i = 0; i < item.Stacks; i++)
                        {
                            var internItem = summaryItemList.FirstOrDefault(x => x.ItemId == item.Id);
                            summaryItemList.Remove(internItem);
                        }
                    }
                    else
                    {
                        summaryItemList.Remove(nItem);
                    }

                    Console.WriteLine("Removing: {0}; Times: {1}", nItem.ItemId, item.Stacks);
                }
            }
            if (summaryItemList.Count != 0)
            {
                Console.WriteLine(summaryItemList.Count);
                var item = summaryItemList[0];
                var bItem = ItemInfo.GetItemList().Find(x => x.ItemId == item.ItemId);
                if (bItem != null)
                {
                    Console.WriteLine(itemList.Count(x => x.Slot <= 6 && x.Slot >= 0));
                    if (itemList.Count(x => x.Slot <= 6 && x.Slot >= 0) == 7)
                    {
                        SellUselessItems();
                    }
                    if (ObjectManager.Player.BuyItem(item.ItemId) && ItemInfo.CanBuyItem(bItem))
                    {
                        Console.WriteLine("Bought: "+ item.ItemId);
                    }
                    else
                    {
                        Console.WriteLine("Can't buy: " + item.ItemId);
                        BuyNextItem(bItem.FromItemIds);
                    }
                }
            }
        }

        private static void SellUselessItems()
        {
            var itemList = ObjectManager.Player.InventoryItems.ToList();
            var uselessItemList = new List<ItemId>() { 
                ItemId.Mana_Potion, 
                ItemId.Health_Potion, 
                ItemId.Crystalline_Flask, 
                ItemId.Dorans_Blade, 
                ItemId.Dorans_Ring, 
                ItemId.Dorans_Shield, 
                ItemId.Vision_Ward,
                ItemId.Stealth_Ward
            };
            Console.WriteLine("Selling Useless Items");
            foreach (var id in uselessItemList)
            {
                var item = itemList.FirstOrDefault(x => x.Id == id);
                if (item == null)
                {
                    continue;
                }
                if (item.Stacks > 1)
                {
                    for (int i = 0; i < item.Stacks; i++)
                    {
                        var internItem = itemList.FirstOrDefault(x => x.Id == item.Id);
                        ObjectManager.Player.SellItem(internItem.Slot);
                        Console.WriteLine("Sold: " + item.Id);
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Sold: " + item.Id);
                    ObjectManager.Player.SellItem(item.Slot);
                    break;
                }
            }
        }

        private static void BuyNextItem(List<ItemId> fromItemList)
        {
            var itemList = ObjectManager.Player.InventoryItems.ToList();
            foreach (var inventoryItem in itemList)
            {
                var nItem = fromItemList.FirstOrDefault(x => x == inventoryItem.Id);
                if (nItem != ItemId.Unknown)
                {
                    if (inventoryItem.Stacks > 1)
                    {
                        for (int i = 0; i < inventoryItem.Stacks; i++)
                        {
                            var internItem = fromItemList.FirstOrDefault(x => x == inventoryItem.Id);
                            fromItemList.Remove(internItem);
                        }
                    }
                    else
                    {
                        fromItemList.Remove(nItem);
                    }

                    Console.WriteLine("Removing: {0}; Times: {1}", nItem, inventoryItem.Stacks);
                }
            }
            foreach (var itemId in fromItemList)
            {
                var lItem = ItemInfo.GetItemList().Find(x => x.ItemId == itemId);
                if (lItem != null)
                {
                    if (ObjectManager.Player.BuyItem(lItem.ItemId) && ItemInfo.CanBuyItem(lItem))
                    {
                        Console.WriteLine("Bought: " + lItem.ItemId);
                    }
                    else
                    {
                        Console.WriteLine("Can't buy: " + lItem.ItemId);
                        BuyNextItem(lItem.FromItemIds);
                    }
                }
            }
        }

        public static StringList GetBuildNames()
        {
            StringList list = new StringList();
            if (builds == null)
            {
                builds = new List<BuildLevler>();
            }
            if (builds.Count == 0)
            {
                list.SList = new[] { "" };
            }
            else
            {
                List<String> elements = new List<string>();
                foreach (BuildLevler levler in builds)
                {
                    if (levler.ChampionName.Contains(ObjectManager.Player.ChampionName))
                    {
                        elements.Add(levler.Name);
                    }
                }
                if (elements.Count == 0)
                {
                    list.SList = new[] { "" };
                }
                else
                {
                    list = new StringList(elements.ToArray());
                }
            }
            return list;
        }

        private void DeleteSequence()
        {
            StringList list = AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyLoadChoice").GetValue<StringList>();
            foreach (BuildLevler levler in builds.ToArray())
            {
                if (levler.Name.Contains(list.SList[list.SelectedIndex]))
                {
                    builds.Remove(levler);
                    List<String> temp = list.SList.ToList();
                    temp.RemoveAt(list.SelectedIndex);
                    if (temp.Count == 0)
                    {
                        temp.Add("");
                    }
                    if (list.SelectedIndex > 0)
                    {
                        list.SelectedIndex -= 1;
                    }
                    list.SList = temp.ToArray();
                    AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyLoadChoice").SetValue<StringList>(list);
                    break;
                }
            }
        }

        private static void SaveSequence(bool newEntry)
        {
            StringList list = AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyLoadChoice").GetValue<StringList>();
            if (AutoBuyGUI.CurrentBuilder.New)
            {
                AutoBuyGUI.CurrentBuilder.New = false;
                builds.Add(AutoBuyGUI.CurrentBuilder);
                List<String> temp = list.SList.ToList();
                if (temp.Count == 1)
                {
                    if (temp[0].Equals(""))
                    {
                        temp.RemoveAt(0);
                    }
                    else
                    {
                        list.SelectedIndex += 1;
                    }
                }
                else
                {
                    list.SelectedIndex += 1;
                }
                temp.Add(AutoBuyGUI.CurrentBuilder.Name);
                list.SList = temp.ToArray();
            }
            else
            {
                foreach (var levler in builds.ToArray())
                {
                    if (levler.Name.Equals(AutoBuyGUI.CurrentBuilder.Name))
                    {
                        builds[list.SelectedIndex] = AutoBuyGUI.CurrentBuilder;
                    }
                }
            }
            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyLoadChoice").SetValue<StringList>(list);
        }

        private String GetFreeSequenceName()
        {
            List<int> endings = new List<int>();
            List<BuildLevler> sequences = new List<BuildLevler>();
            for (int i = 0; i < builds.Count; i++)
            {
                if (builds[i].ChampionName.Contains(ObjectManager.Player.ChampionName))
                {
                    String ending = builds[i].Name.Substring(ObjectManager.Player.ChampionName.Length);
                    try
                    {
                        endings.Add(Convert.ToInt32(ending));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            for (int i = 0; i < 10000; i++)
            {
                if (!endings.Contains(i))
                {
                    return ObjectManager.Player.ChampionName + i;
                }
            }
            return ObjectManager.Player.ChampionName + 0;
        }

        private class LolBuilder
        {

            public static void GetLolBuilderData()
            {
                String lolBuilderData = null;
                try
                {
                    lolBuilderData = Website.GetWebSiteContent("http://lolbuilder.net/" + ObjectManager.Player.ChampionName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                String patternBuilds = "id=\"build-content-([\\S\\s]*?)-([\\S\\s]*?)\">([\\S\\s]*?)<div class=\"tab-pane";//<div class=\"build-body\"([\\S\\s]*?)id=\"build-content-([\\S\\s]*?)";
                String patternBuildStart = "starting-item-sets row(.*?)</section>";
                String patternBuildStartInner = "col-lg-6 col-md-6 col-sm-12 starting-item-set([\\S\\s]*?)col-lg-6 col-md-6 col-sm-12 starting-item-set";
                String patternBuildSummary = "Build Summary(.*?)<div class=\"row\">";
                String patternBuildOrder = "Build Order(.*?)</section>";
                String patternBuildFinal = "Final Items(.*?)</section>";
                String patternBuildItem = "<small class=\"t-overflow\">([\\S\\s]*?)</small>";
                String patternBuildItemCount = "<span class=\"count\"><span class=\"multiple\">([\\S\\s]*?)</span><span class=\"times\"> &times; </span></span>([\\S\\s]*?$)";

                List<BuildLevler> builds = new List<BuildLevler>();

                for (int i = 0; ; i++)
                {
                    List<String> summaryItems = new List<string>();
                    List<String> startItems = new List<string>();
                    List<String> buildOrderItems = new List<string>();
                    List<String> finalItems = new List<string>();
                    GameMapId mapId = 0;
                    String matchBuilds = Website.GetMatch(lolBuilderData, patternBuilds, i, 3);
                    if (matchBuilds.Equals(""))
                    {
                        break;
                    }
                    String matchBuildsMapId = Website.GetMatch(lolBuilderData, patternBuilds, i, 2);
                    switch (matchBuildsMapId)
                    {
                        case "sr":
                            mapId = GameMapId.SummonersRift;
                            break;
                            
                        case "tt":
                            mapId = GameMapId.TwistedTreeline;
                            break;

                        case "cs":
                            mapId = GameMapId.CrystalScar;
                            break;

                        case "ha":
                            mapId = GameMapId.HowlingAbyss;
                            break;
                    }
                    if (mapId != Game.MapId)
                    {
                        continue;
                    }

                    String matchBuildStart = Website.GetMatch(matchBuilds, patternBuildStart, 0);
                    if (!matchBuildStart.Equals(""))
                    {
                        String matchBuildStartInner = Website.GetMatch(matchBuildStart, patternBuildStartInner);
                        if (!matchBuildStartInner.Equals(""))
                        {
                            for (int j = 0; ; j++)
                            {
                                String matchBuildItem = Website.GetMatch(matchBuildStartInner, patternBuildItem, j);
                                if (matchBuildItem.Equals(""))
                                {
                                    break;
                                }
                                String matchBuildItemCount = Website.GetMatch(matchBuildItem, patternBuildItemCount);
                                if (matchBuildItemCount.Equals(""))
                                {
                                    startItems.Add(matchBuildItem);
                                }
                                else
                                {
                                    String matchBuildItemName = Website.GetMatch(matchBuildItem, patternBuildItemCount, 0, 2);
                                    try
                                    {
                                        int count = Convert.ToInt32(matchBuildItemCount);
                                        for (int k = 0; k < count; k++)
                                        {
                                            startItems.Add(matchBuildItemName);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; ; j++)
                            {
                                String matchBuildItem = Website.GetMatch(matchBuildStart, patternBuildItem, j);
                                if (matchBuildItem.Equals(""))
                                {
                                    break;
                                }
                                String matchBuildItemCount = Website.GetMatch(matchBuildItem, patternBuildItemCount);
                                if (matchBuildItemCount.Equals(""))
                                {
                                    startItems.Add(matchBuildItem);
                                }
                                else
                                {
                                    String matchBuildItemName = Website.GetMatch(matchBuildItem, patternBuildItemCount, 0, 2);
                                    try
                                    {
                                        int count = Convert.ToInt32(matchBuildItemCount);
                                        for (int k = 0; k < count; k++)
                                        {
                                            startItems.Add(matchBuildItemName);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                    }
                                }
                            }
                        }
                    }
                    String matchBuildSummary = Website.GetMatch(matchBuilds, patternBuildSummary, 0);
                    if (!matchBuildSummary.Equals(""))
                    {
                        for (int j = 0; ; j++)
                        {
                            String matchBuildItem = Website.GetMatch(matchBuildSummary, patternBuildItem, j);
                            if (matchBuildItem.Equals(""))
                            {
                                break;
                            }
                            String matchBuildItemCount = Website.GetMatch(matchBuildItem, patternBuildItemCount);
                            if (matchBuildItemCount.Equals(""))
                            {
                                summaryItems.Add(matchBuildItem);
                            }
                            else
                            {
                                String matchBuildItemName = Website.GetMatch(matchBuildItem, patternBuildItemCount, 0, 2);
                                try
                                {
                                    int count = Convert.ToInt32(matchBuildItemCount);
                                    for (int k = 0; k < count; k++)
                                    {
                                        summaryItems.Add(matchBuildItemName);
                                    }
                                }
                                catch (Exception e)
                                {
                                }
                            }
                        }
                    }
                    String matchBuildOrder = Website.GetMatch(matchBuilds, patternBuildOrder, 0);
                    if (!matchBuildOrder.Equals(""))
                    {
                        for (int j = 0; ; j++)
                        {
                            String matchBuildItem = Website.GetMatch(matchBuildOrder, patternBuildItem, j);
                            if (matchBuildItem.Equals(""))
                            {
                                break;
                            }
                            String matchBuildItemCount = Website.GetMatch(matchBuildItem, patternBuildItemCount);
                            if (matchBuildItemCount.Equals(""))
                            {
                                buildOrderItems.Add(matchBuildItem);
                            }
                            else
                            {
                                String matchBuildItemName = Website.GetMatch(matchBuildItem, patternBuildItemCount, 0, 2);
                                try
                                {
                                    int count = Convert.ToInt32(matchBuildItemCount);
                                    for (int k = 0; k < count; k++)
                                    {
                                        buildOrderItems.Add(matchBuildItemName);
                                    }
                                }
                                catch (Exception e)
                                {
                                }
                            }
                        }
                    }
                    String matchBuildFinal = Website.GetMatch(matchBuilds, patternBuildFinal, 0);
                    if (!matchBuildFinal.Equals(""))
                    {
                        for (int j = 0; ; j++)
                        {
                            String matchBuildItem = Website.GetMatch(matchBuildFinal, patternBuildItem, j);
                            if (matchBuildItem.Equals(""))
                            {
                                break;
                            }
                            String matchBuildItemCount = Website.GetMatch(matchBuildItem, patternBuildItemCount);
                            if (matchBuildItemCount.Equals(""))
                            {
                                finalItems.Add(matchBuildItem);
                            }
                            else
                            {
                                String matchBuildItemName = Website.GetMatch(matchBuildItem, patternBuildItemCount, 0, 2);
                                try
                                {
                                    int count = Convert.ToInt32(matchBuildItemCount);
                                    for (int k = 0; k < count; k++)
                                    {
                                        finalItems.Add(matchBuildItemName);
                                    }
                                }
                                catch (Exception e)
                                {
                                }
                            }
                        }
                    }
                    List<BuildLevler.BuildItem> itemIdList = new List<BuildLevler.BuildItem>();
                    foreach (var b in startItems)
                    {
                        ItemId id = ConvertNameToId(b);
                        if (id != ItemId.Unknown)
                        {
                            itemIdList.Add(new BuildLevler.BuildItem(id, BuildLevler.BuildItem.ItemType.Start));
                        }
                        else
                        {
                            Console.WriteLine("Unknown: " + b);
                        }
                    }
                    //buildItems.AddRange(buildOrderItems);
                    foreach (var b in summaryItems)
                    {
                        ItemId id = ConvertNameToId(b);
                        if (id != ItemId.Unknown)
                        {
                            itemIdList.Add(new BuildLevler.BuildItem(id, BuildLevler.BuildItem.ItemType.Summary));
                        }
                        else
                        {
                            Console.WriteLine("Unknown: " + b);
                        }
                    }
                    //foreach (var b in finalItems)
                    //{
                    //    ItemId id = ConvertNameToId(b);
                    //    if (id != ItemId.Unknown)
                    //    {
                    //        itemIdList.Add(new BuildLevler.BuildItem(id, BuildLevler.BuildItem.ItemType.Final));
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine("Unknown: " + b);
                    //    }
                    //}
                    BuildLevler build = new BuildLevler(ObjectManager.Player.ChampionName + " LolBuilder " + i, itemIdList, mapId);
                    build.New = true;
                    AutoBuyGUI.CurrentBuilder = build;
                    SaveSequence(build.New);
                }
            }

            private static ItemId ConvertNameToId(String itemName)
            {
                var firstOrDefault = ItemInfo.GetItemList().FirstOrDefault(x => x.Name.ToLower().Equals(itemName.ToLower()));
                if (firstOrDefault != null)
                {
                    return firstOrDefault.ItemId;
                }
                return ItemId.Unknown;
            }
        }

        [Serializable]
        class BuildLevler
        {
            public String Name;
            public String ChampionName;
            public List<BuildItem> ItemIdList = new List<BuildItem>(36);
            public bool New = true;
            public bool Web = false;
            public GameMapId MapId = 0;

            public BuildLevler(string name)
            {
                Name = name;
                ChampionName = ObjectManager.Player.ChampionName;
                ItemIdList = new List<BuildItem>();

            }

            public BuildLevler(string name, List<BuildItem> itemId, GameMapId mapId)
            {
                Name = name;
                ChampionName = ObjectManager.Player.ChampionName;
                ItemIdList = itemId;
                MapId = mapId;
            }

            public BuildLevler()
            {

            }

            public class BuildItem
            {
                public enum ItemType
                {
                    Unknown,
                    Start,
                    Summary,
                    Final
                }

                public ItemId ItemId;
                public ItemType Type;

                public BuildItem(ItemId itemId, ItemType type)
                {
                    ItemId = itemId;
                    Type = type;
                }
            }

        }

        class ItemInfo // TODO: Add ItemData as main method to retriev item infos
        {
            public enum ItemType
            {
                Nothing,
                Jungle,
                Lane,
                Consumable,
                Vision,
                GoldPer,
                Armor,
                SpellBlock,
                Health,
                HealthRegen,
                AttackSpeed,
                CriticalStrike,
                Damage,
                Lifesteal,
                CooldownReduction,
                Mana,
                ManaRegen,
                SpellDamage,
                Boots,
                NonBootsMovement,
                SpellVamp,
                ArmorPenetration,
                MagicPenetration,
                Active,
                Aura,
                Slow,
                Stealth,
                Trinket,
                Tenacity,
                OnHit
            }

            public ItemId ItemId;
            public String Name;
            public String Description;
            public String PlainDescription;
            public int Price;
            public List<ItemType> ItemTypeList;
            public List<GameMapId> MapId;
            public List<ItemId> FromItemIds = new List<ItemId>();
            public List<ItemId> ToItemIds = new List<ItemId>();
            public int TotalGold;
            public bool Purchasable;
            public String PictureName;

            private static Dictionary<String, ItemType> dicItemType;
            private static List<ItemInfo> _items = null;

            private static ItemType StringToItemType(String sItemType)
            {
                if (dicItemType == null)
                {
                    dicItemType = new Dictionary<string, ItemType>();
                    for (int i = 0; i < Enum.GetValues(typeof(ItemType)).Length; i++)
                    {
                        String name = Enum.GetName(typeof(ItemType), i);
                        if (name != null)
                        {
                            dicItemType.Add(name.ToLower(), (ItemType)i);
                        }
                    }
                }
                ItemType returnValue;
                if (dicItemType.TryGetValue(sItemType.ToLower(), out returnValue))
                {
                    return returnValue;
                }
                else
                {
                    return ItemType.Nothing;
                }
            }

            public static bool CanBuyItem(ItemInfo item)
            {
                int price = item.Price;
                var need = GetPrice(item.FromItemIds);
                Console.WriteLine("ItemGold for: {0}; Required: {1}; Need: {2}", item.ItemId, price, need);
                return ObjectManager.Player.Gold >= price + need;
            }

            private static int GetPrice(List<ItemId> fromItemList)
            {
                int need = 0;
                var itemList = ObjectManager.Player.InventoryItems.ToList();
                foreach (var inventoryItem in itemList)
                {
                    var nItem = fromItemList.FirstOrDefault(x => x == inventoryItem.Id);
                    if (nItem != ItemId.Unknown)
                    {
                        if (inventoryItem.Stacks > 1)
                        {
                            for (int i = 0; i < inventoryItem.Stacks; i++)
                            {
                                var internItem = fromItemList.FirstOrDefault(x => x == inventoryItem.Id);
                                fromItemList.Remove(internItem);
                            }
                        }
                        else
                        {
                            fromItemList.Remove(nItem);
                        }

                        Console.WriteLine("Removing: {0}; Times: {1}", nItem, inventoryItem.Stacks);
                    }
                }
                Console.WriteLine("GetPriceCount: " + fromItemList.Count);
                foreach (var itemId in fromItemList)
                {
                    var lItem = ItemInfo.GetItemList().Find(x => x.ItemId == itemId);
                    if (lItem != null)
                    {
                        need += lItem.Price;
                        need += GetPrice(lItem.FromItemIds);
                    }
                }
                return need;
            }

            public static List<ItemInfo> GetItemList() //Does not download the item.json correctly, it is missing the tags attribute somehow!!!
            {
                if (_items != null)
                {
                    return _items;
                }
                String name = "";
                String version = "";
                try
                {
                    String json = new WebClient().DownloadString("http://ddragon.leagueoflegends.com/realms/euw.json");
                    version = (string)new JavaScriptSerializer().Deserialize<Dictionary<String, Object>>(json)["v"];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot download file: {0}, Exception: {1}", name, ex);
                    return null;
                }
                List<ItemInfo> itemInfoList = new List<ItemInfo>();
                try
                {
                    String url = "http://ddragon.leagueoflegends.com/cdn/5.12.1/data/en_US/item.json"; //temp fix for riot fault otherwise + version +
                    String json = new WebClient().DownloadString(url);
                    JObject data = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject<Object>(json);
                    JToken token = data["data"].First;
                    do
                    {
                        ItemInfo itemInfo = new ItemInfo();
                        itemInfo.ItemId = (ItemId) Convert.ToInt32(((JProperty) token).Name);
                        itemInfo.Name = token.First["name"].ToString();
                        itemInfo.Description = token.First["description"].ToString();
                        itemInfo.PlainDescription = token.First["plaintext"].ToString();
                        if (token.First["from"] != null)
                        {
                            itemInfo.FromItemIds = token.First["from"].Values<int>().Select(x => (ItemId) x).ToList();
                        }
                        if (token.First["into"] != null)
                        {
                            itemInfo.ToItemIds = token.First["into"].Values<int>().Select(x => (ItemId) x).ToList();
                        }
                        itemInfo.Price = token.First["gold"]["base"].Value<int>();
                        itemInfo.TotalGold = token.First["gold"]["total"].Value<int>();
                        itemInfo.Purchasable = token.First["gold"]["purchasable"].Value<bool>();
                        itemInfo.ItemTypeList =
                            token.First["tags"].Values<String>().Select(ItemInfo.StringToItemType).ToList();
                        ; //tags -> String, not available after 5.12.1
                        itemInfo.MapId = new List<GameMapId>(); //null = everything, maps -> MapId: false
                        if (token.First["maps"] != null)
                        {
                            JToken mapToken = token.First["maps"].First;
                            do
                            {
                                GameMapId gmId = (GameMapId) Convert.ToInt32(((JProperty) mapToken).Name);
                                if ((int) itemInfo.ItemId == 3187)
                                    Console.WriteLine(gmId);
                                if (gmId == (GameMapId) 1)
                                {
                                    gmId = (GameMapId) 11;
                                }
                                itemInfo.MapId.Add(gmId);
                            } while ((mapToken = mapToken.Next) != null);
                        }
                        itemInfo.PictureName = token.First["image"]["full"].ToString();
                        itemInfoList.Add(itemInfo);
                    } while ((token = token.Next) != null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot download file: {0}, Exception: {1}", name, ex);
                    return null;
                }
                _items = itemInfoList;
                return _items;
            }
        }

        class AutoBuyGUI
        {
            public static Dictionary<ItemInfo, AutoBuySpriteInfo> ItemFrames = new Dictionary<ItemInfo, AutoBuySpriteInfo>();
            public static BuildLevler CurrentBuilder = new BuildLevler();
            public static ShopFrame Shop = new ShopFrame();
            public static BuildFrame Build = new BuildFrame();

            static AutoBuyGUI()
            {
                new Thread(LoadSpritesAsync).Start();
            }

            private static void LoadSpritesAsync()
            {
                var itemList = ItemInfo.GetItemList();
                foreach (var item in itemList)
                {
                    SpriteHelper.DownloadImageRiot(((int)item.ItemId).ToString(), SpriteHelper.ChampionType.Item, SpriteHelper.DownloadType.Item, @"AutoBuy");
                }
            }

            public AutoBuyGUI()
            {
                Game.OnUpdate += Game_OnUpdate;
            }

            void Game_OnUpdate(EventArgs args)
            {
                if (!IsActive())
                    return;

                Shop.Game_OnUpdate();
                Build.Game_OnUpdate();

                var itemList = ItemInfo.GetItemList();
                for (int i = 0; i < itemList.Count; i++)
                {
                    var item = itemList[i];
                    if (!ItemFrames.ContainsKey(item))
                    {
                        ItemFrames.Add(item, new AutoBuySpriteInfo(item));
                    }
                }
                foreach (var itemFrame in ItemFrames)
                {
                    Game_OnUpdateItemFrames(itemFrame);
                }

                Game.OnUpdate -= Game_OnUpdate;
            }

            private void Game_OnUpdateItemFrames(KeyValuePair<ItemInfo, AutoBuySpriteInfo> sprite)
            {
                var item = sprite.Key;
                if (sprite.Value.Sprite == null || !ItemFrames[item].Sprite.DownloadFinished)
                {
                    SpriteHelper.LoadTexture(item.PictureName, ref sprite.Value.Sprite, @"AutoBuy");
                }
                if (sprite.Value.Sprite != null && sprite.Value.Sprite.DownloadFinished && !sprite.Value.Sprite.LoadingFinished)
                {
                    sprite.Value.Sprite.LoadingFinished = true;
                }
            }

            public class AutoBuySpriteInfo
            {
                public ItemInfo Item;
                public SpriteHelper.SpriteInfo Sprite;
                public SpriteHelper.SpriteInfo Text;

                public AutoBuySpriteInfo(ItemInfo item)
                {
                    Item = item;
                }

                public AutoBuySpriteInfo(ItemInfo item, SpriteHelper.SpriteInfo sprite)
                {
                    Item = item;
                    Sprite = sprite;
                }
            }

            public class ShopFrame
            {
                public static SpriteHelper.SpriteInfo ShopSprite;
                public static List<AutoBuySpriteInfo> ItemShopSprites = new List<AutoBuySpriteInfo>();
                public static Render.Rectangle Rectangle = new Render.Rectangle(10, 10, 100, 100, new ColorBGRA(SharpDX.Color.Black.ToColor3(), 0.75f));
                public static Render.Line LineLeft = new Render.Line(new Vector2(10, 10), new Vector2(100, 100), 1, SharpDX.Color.LightYellow);
                public static Render.Line LineRight = new Render.Line(new Vector2(10, 10), new Vector2(100, 100), 1, SharpDX.Color.LightYellow);
                public static Vector2 ShopStart = new Vector2(220, 125);
                public static Vector2 ShopIncrement = new Vector2(64, 64);
                public static Size ShopBlockSize = new Size(48, 48);
                public static int ShopMaxRow = 12;
                public static int ShopMaxColumn = 12;
                public static bool StartItemActive = false;
                public static Vector2 StartItemsStart = new Vector2(278, 628);
                public static Vector2 FinalItemsStart = new Vector2(512, 628);
                public static Vector2 StartFinalItemsIncrement = new Vector2(32, 32);

                public void Game_OnUpdate()
                {
                    if (ShopSprite == null || !ShopSprite.DownloadFinished)
                    {
                        SpriteHelper.LoadTexture("ShopFrame", ref ShopSprite, SpriteHelper.TextureType.Default);
                    }
                    if (ShopSprite != null && ShopSprite.DownloadFinished && !ShopSprite.LoadingFinished)
                    {
                        ShopSprite.Sprite.PositionUpdate = delegate
                        {
                            return new Vector2(Drawing.Width / 6, Drawing.Height / 2 - ShopSprite.Bitmap.Height / 2);
                        };
                        ShopSprite.Sprite.VisibleCondition = delegate
                        {
                            return IsActive() &&
                                (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").GetValue<bool>() ||
                                AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").GetValue<bool>());
                        };
                        ShopSprite.Sprite.Add(1);
                        ShopSprite.LoadingFinished = true;
                    }

                    Rectangle.PositionUpdate = delegate
                    {
                        return Utils.GetCursorPos() + new Vector2(40, 0); 
                    };
                    Rectangle.Visible = false;
                    Rectangle.Add(3);

                    LineLeft.StartPositionUpdate = delegate
                    {
                        if (StartItemActive)
                        {
                            return new Vector2(
                                ShopSprite.Sprite.X + StartItemsStart.X, ShopSprite.Sprite.Y + StartItemsStart.Y);
                        }
                        else
                        {
                            return new Vector2(
                                ShopSprite.Sprite.X + FinalItemsStart.X, ShopSprite.Sprite.Y + FinalItemsStart.Y);
                        }
                    };
                    LineLeft.EndPositionUpdate = delegate
                    {
                        if (StartItemActive)
                        {
                            return new Vector2(
                                ShopSprite.Sprite.X + StartItemsStart.X + StartFinalItemsIncrement.X, ShopSprite.Sprite.Y + StartItemsStart.Y + StartFinalItemsIncrement.Y);
                        }
                        else
                        {
                            return new Vector2(
                                ShopSprite.Sprite.X + FinalItemsStart.X + StartFinalItemsIncrement.X, ShopSprite.Sprite.Y + FinalItemsStart.Y + +StartFinalItemsIncrement.Y);
                        }
                    };
                    LineLeft.VisibleCondition = delegate
                    {
                        return IsActive() &&
                            (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").GetValue<bool>() ||
                            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").GetValue<bool>());
                    };
                    LineLeft.Add(4);

                    LineRight.StartPositionUpdate = delegate
                    {
                        if (StartItemActive)
                        {
                            return new Vector2(
                                ShopSprite.Sprite.X + StartItemsStart.X + StartFinalItemsIncrement.X, ShopSprite.Sprite.Y + StartItemsStart.Y);
                        }
                        else
                        {
                            return new Vector2(
                                ShopSprite.Sprite.X + FinalItemsStart.X + StartFinalItemsIncrement.X, ShopSprite.Sprite.Y + FinalItemsStart.Y);
                        }
                    };
                    LineRight.EndPositionUpdate = delegate
                    {
                        if (StartItemActive)
                        {
                            return new Vector2(
                                ShopSprite.Sprite.X + StartItemsStart.X, ShopSprite.Sprite.Y + StartItemsStart.Y + StartFinalItemsIncrement.Y);
                        }
                        else
                        {
                            return new Vector2(
                                ShopSprite.Sprite.X + FinalItemsStart.X, ShopSprite.Sprite.Y + FinalItemsStart.Y + +StartFinalItemsIncrement.Y);
                        }
                    };
                    LineRight.VisibleCondition = delegate
                    {
                        return IsActive() &&
                            (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").GetValue<bool>() ||
                            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").GetValue<bool>());
                    };
                    LineRight.Add(4);
                }

                public static void CreateShopSprites(ItemInfo.ItemType type)
                {
                    var itemInfos = ItemFrames.Where(x => x.Key.ItemTypeList.Any(y => y == type) && (x.Key.MapId.All(y => y != Game.MapId) || x.Key.MapId.Count == 0) &&
                        x.Key.Purchasable == true).ToList();
                    foreach (var info in ItemShopSprites)
                    {
                        if (info.Sprite != null && info.Sprite.Sprite != null)
                        {
                            info.Sprite.Sprite.Remove();
                            info.Sprite.Sprite.Dispose();
                        }
                        if (info.Text != null && info.Text.Text != null)
                        {
                            info.Text.Text.Remove();
                            info.Text.Text.Dispose();
                        }
                    }
                    ItemShopSprites.Clear();
                    for (int index = 0; index < itemInfos.Count; index++)
                    {
                        int i = 0 + index;
                        var itemInfo = itemInfos[i];
                        var absi = new AutoBuySpriteInfo(itemInfo.Key);
                        absi.Sprite = new SpriteHelper.SpriteInfo();

                        absi.Sprite.Bitmap = itemInfo.Value.Sprite.Bitmap;
                        absi.Sprite.Sprite = new Render.Sprite(itemInfo.Value.Sprite.Bitmap, new Vector2(0, 0));
                        absi.Sprite.Sprite.Scale = new Vector2(0.75f);
                        absi.Sprite.Sprite.PositionUpdate = delegate
                        {
                            return GetItemSlotPositionShop(i / ShopMaxRow, i % ShopMaxColumn);
                        };
                        absi.Sprite.Sprite.VisibleCondition = delegate
                        {
                            return IsActive() &&
                                (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").GetValue<bool>() ||
                                AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").GetValue<bool>());
                        };
                        absi.Sprite.Sprite.Add(2);

                        String text = itemInfo.Key.Name + "\n" + itemInfo.Key.PlainDescription;
                        absi.Text = new SpriteHelper.SpriteInfo();
                        absi.Text.Text = new Render.Text(text, 0, 0, 20, SharpDX.Color.White);
                        Font f = new Font(Drawing.Direct3DDevice, absi.Text.Text.TextFontDescription);
                        absi.Text.TextLength = f.MeasureText(null, text, FontDrawFlags.NoClip);
                        f.Dispose();
                        absi.Text.Text.PositionUpdate = delegate
                        {
                            return Utils.GetCursorPos() + new Vector2(40, 0); 
                        };
                        absi.Text.Text.Visible = false;
                        absi.Text.Text.Add(4);

                        ItemShopSprites.Add(absi);
                    }
                }

                public static Vector2 GetItemSlotPositionShop(int row, int column)
                {
                    return new Vector2(ShopSprite.Sprite.X + ShopStart.X + (ShopIncrement.X * column), ShopSprite.Sprite.Y + ShopStart.Y + (ShopIncrement.Y * row));
                }

            }

            public class BuildFrame
            {
                public static SpriteHelper.SpriteInfo BuildSprite;
                public static List<AutoBuySpriteInfo> ItemBuildSpritesStart = new List<AutoBuySpriteInfo>();
                public static List<AutoBuySpriteInfo> ItemBuildSpritesSummary = new List<AutoBuySpriteInfo>();
                public static Render.Rectangle Rectangle = new Render.Rectangle(10, 10, 100, 100, new ColorBGRA(SharpDX.Color.Black.ToColor3(), 0.75f));
                public static Vector2 BuildStart = new Vector2(30, 50);
                public static Vector2 BuildFinal = new Vector2(30, 130);
                public static Vector2 BuildIncrement = new Vector2(64, 64);
                public static Size BuildBlockSize = new Size(48, 48);
                public static int BuildMaxRow = 7;
                public static int BuildMaxColumn = 7;
                public static int LastClickedElementId = -1;

                public void Game_OnUpdate()
                {
                    if (BuildSprite == null || !BuildSprite.DownloadFinished)
                    {
                        SpriteHelper.LoadTexture("BuildFrame", ref BuildSprite, SpriteHelper.TextureType.Default);
                    }
                    if (BuildSprite != null && BuildSprite.DownloadFinished && !BuildSprite.LoadingFinished)
                    {
                        BuildSprite.Sprite.PositionUpdate = delegate
                        {
                            return new Vector2(ShopFrame.ShopSprite.Sprite.Position.X + ShopFrame.ShopSprite.Bitmap.Width, ShopFrame.ShopSprite.Sprite.Position.Y + ShopFrame.ShopSprite.Sprite.Height / 2 - BuildSprite.Sprite.Height / 2);
                        };
                        BuildSprite.Sprite.VisibleCondition = delegate
                        {
                            return IsActive() &&
                                (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").GetValue<bool>() ||
                                AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").GetValue<bool>());
                        };
                        BuildSprite.Sprite.Add(1);
                        BuildSprite.LoadingFinished = true;
                    }
                    Rectangle.Visible = false;
                    Rectangle.Add(3);
                }

                //public static void CreateBuildSprites(BuildLevler build)
                //{
                //    var itemInfos = build.ItemIdList.Select(x => ItemFrames.First(y => y.Key.ItemId == x.ItemId)).ToList();
                //    foreach (var info in ItemBuildSprites)
                //    {
                //        if (info.Sprite != null && info.Sprite.Sprite != null)
                //        {
                //            info.Sprite.Sprite.Remove();
                //            info.Sprite.Sprite.Dispose();
                //        }
                //        if (info.Text != null && info.Text.Text != null)
                //        {
                //            info.Text.Text.Remove();
                //            info.Text.Text.Dispose();
                //        }
                //    }
                //    ItemBuildSprites.Clear();
                //    for (int index = 0; index < itemInfos.Count; index++)
                //    {
                //        int i = 0 + index;
                //        var itemInfo = itemInfos[i];
                //        var absi = new AutoBuySpriteInfo(itemInfo.Key);
                //        var type = build.ItemIdList.First(x => x.ItemId == itemInfo.Key.ItemId).Type;

                //        absi.Sprite = new SpriteHelper.SpriteInfo();
                //        absi.Sprite.Bitmap = itemInfo.Value.Sprite.Bitmap;
                //        absi.Sprite.Sprite = new Render.Sprite(itemInfo.Value.Sprite.Bitmap, new Vector2(0, 0));
                //        absi.Sprite.Sprite.Scale = new Vector2(0.75f);
                //        absi.Sprite.Sprite.PositionUpdate = delegate
                //        {
                //            if (type == BuildLevler.BuildItem.ItemType.Start)
                //            {
                //                return GetItemSlotPositionBuild(i / BuildMaxRow, i % BuildMaxColumn);
                //            }
                //            else
                //            {
                //                return GetItemSlotPositionBuild(i / BuildMaxRow, i % BuildMaxColumn);
                //            }
                            
                //        };
                //        absi.Sprite.Sprite.VisibleCondition = delegate
                //        {
                //            return IsActive() &&
                //                (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").GetValue<bool>() ||
                //                AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").GetValue<bool>());
                //        };
                //        absi.Sprite.Sprite.Add(2);

                //        String text = itemInfo.Key.Name + "\n" + itemInfo.Key.PlainDescription;
                //        absi.Text = new SpriteHelper.SpriteInfo();
                //        absi.Text.Text = new Render.Text(text, 0, 0, 20, SharpDX.Color.White);
                //        Font f = new Font(Drawing.Direct3DDevice, absi.Text.Text.TextFontDescription);
                //        absi.Text.TextLength = f.MeasureText(null, text, FontDrawFlags.NoClip);
                //        f.Dispose();
                //        absi.Text.Text.PositionUpdate = delegate
                //        {
                //            return Utils.GetCursorPos() - new Vector2(40 + absi.Text.TextLength.Width, 0); 
                //        };
                //        absi.Text.Text.Visible = false;
                //        absi.Text.Text.Add(4);

                //        ItemBuildSprites.Add(absi);
                //    }
                //    Console.WriteLine("CreateBuildSprites: " + ItemBuildSprites.Count);
                //}

                public static void CreateBuildSprites(BuildLevler build)
                {
                    var itemInfosStart = build.ItemIdList.Select(x => ItemFrames.FirstOrDefault(y => y.Key.ItemId == x.ItemId && x.Type == BuildLevler.BuildItem.ItemType.Start)).ToList();
                    foreach (var info in ItemBuildSpritesStart)
                    {
                        if (info.Sprite != null && info.Sprite.Sprite != null)
                        {
                            info.Sprite.Sprite.Remove();
                            info.Sprite.Sprite.Dispose();
                        }
                        if (info.Text != null && info.Text.Text != null)
                        {
                            info.Text.Text.Remove();
                            info.Text.Text.Dispose();
                        }
                    }
                    ItemBuildSpritesStart.Clear();
                    for (int index = 0; index < itemInfosStart.Count; index++)
                    {
                        int i = 0 + index;
                        var itemInfo = itemInfosStart[i];
                        if (itemInfo.Key != null) //Default value is null if not found
                        {
                            Console.WriteLine("Start: " + itemInfo.Key.Name);

                            AddBuildSprite(new AutoBuySpriteInfo(itemInfo.Key, itemInfo.Value.Sprite), i, BuildLevler.BuildItem.ItemType.Start);
                        }
                    }

                    var itemInfosSummary = build.ItemIdList.Select(x => ItemFrames.FirstOrDefault(y => y.Key.ItemId == x.ItemId && x.Type == BuildLevler.BuildItem.ItemType.Summary)).ToList();
                    foreach (var info in ItemBuildSpritesSummary)
                    {
                        if (info.Sprite != null && info.Sprite.Sprite != null)
                        {
                            info.Sprite.Sprite.Remove();
                            info.Sprite.Sprite.Dispose();
                        }
                        if (info.Text != null && info.Text.Text != null)
                        {
                            info.Text.Text.Remove();
                            info.Text.Text.Dispose();
                        }
                    }
                    ItemBuildSpritesSummary.Clear();
                    for (int index = 0; index < itemInfosSummary.Count; index++)
                    {
                        int i = 0 + index;
                        var itemInfo = itemInfosSummary[i];

                        if (itemInfo.Key != null) //Default value is null if not found
                        {
                            Console.WriteLine("Summary: " + itemInfo.Key.Name);

                            AddBuildSprite(new AutoBuySpriteInfo(itemInfo.Key, itemInfo.Value.Sprite), i, BuildLevler.BuildItem.ItemType.Summary);
                        }
                    }
                    ReorderBuildSprites();
                    Console.WriteLine("CreateBuildSprites: " + ItemBuildSpritesStart.Count);
                    Console.WriteLine("CreateBuildSprites: " + ItemBuildSpritesSummary.Count);
                }

                public static bool AddBuildSprite(AutoBuySpriteInfo referenceSprite, int id = -1, BuildLevler build = null) // null reference
                {
                    var absi = new AutoBuySpriteInfo(referenceSprite.Item);
                    List<AutoBuySpriteInfo> list = null;
                    BuildLevler.BuildItem.ItemType type = BuildLevler.BuildItem.ItemType.Unknown;
                    if (build != null)
                    {
                        var item = build.ItemIdList.FirstOrDefault(x => x.ItemId == referenceSprite.Item.ItemId);
                        if (item != null)
                        {
                            type = item.Type;
                            Console.WriteLine("Found Build item type");
                        }
                    }
                    else
                    {
                        type = AutoBuyGUI.ShopFrame.StartItemActive
                            ? BuildLevler.BuildItem.ItemType.Start
                            : BuildLevler.BuildItem.ItemType.Summary;
                    }
                    return AddBuildSprite(referenceSprite, id, type);
                }

                public static bool AddBuildSprite(AutoBuySpriteInfo referenceSprite, int id = -1, BuildLevler.BuildItem.ItemType type = BuildLevler.BuildItem.ItemType.Unknown) // null reference
                {
                    var absi = new AutoBuySpriteInfo(referenceSprite.Item);
                    List<AutoBuySpriteInfo> list = null;
                    switch (type)
                    {
                        case BuildLevler.BuildItem.ItemType.Start:
                            list = ItemBuildSpritesStart;
                            break;

                        case BuildLevler.BuildItem.ItemType.Summary:
                            list = ItemBuildSpritesSummary;
                            break;
                    }
                    Console.WriteLine("Type: " + type);
                    if (list == null)
                        return false;
                    int i = 0 + list.Count;
                    absi.Sprite = new SpriteHelper.SpriteInfo();
                    Console.WriteLine("Add Sprite; id = {0}; i = {1}", id, i);
                    absi.Sprite.Bitmap = referenceSprite.Sprite.Bitmap;
                    absi.Sprite.Sprite = new Render.Sprite(absi.Sprite.Bitmap, new Vector2(0, 0));
                    absi.Sprite.Sprite.Scale = new Vector2(0.75f);
                    //absi.Sprite.Sprite.PositionUpdate = delegate
                    //{
                    //    return GetItemSlotPositionBuild(i / BuildMaxRow, i % BuildMaxColumn, type);
                    //};
                    absi.Sprite.Sprite.VisibleCondition = delegate
                    {
                        return IsActive() &&
                            (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").GetValue<bool>() ||
                            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").GetValue<bool>());
                    };
                    absi.Sprite.Sprite.Add(2);

                    String text = referenceSprite.Item.Name + "\n" + referenceSprite.Item.PlainDescription;
                    absi.Text = new SpriteHelper.SpriteInfo();
                    absi.Text.Text = new Render.Text(text, 0, 0, 20, SharpDX.Color.White);
                    Font f = new Font(Drawing.Direct3DDevice, absi.Text.Text.TextFontDescription);
                    absi.Text.TextLength = f.MeasureText(null, text, FontDrawFlags.NoClip);
                    f.Dispose();
                    absi.Text.Text.PositionUpdate = delegate
                    {
                        return Utils.GetCursorPos() - new Vector2(40 + absi.Text.TextLength.Width, 0);
                    };
                    absi.Text.Text.Visible = false;
                    absi.Text.Text.Add(4);

                    Console.WriteLine("AddedBuildSprite: " + referenceSprite.Item.ItemId);
                    if (id != -1 && id <= list.Count)
                    {
                        list.Insert(id, absi);
                        ReorderBuildSprites();
                        return true;
                    }
                    else
                    {
                        list.Add(absi);
                        return false;
                    }
                }

                public static int DeleteBuildSprite(AutoBuySpriteInfo referenceSprite)
                {
                    List<AutoBuySpriteInfo> list = null;
                    int startId = 0;
                    BuildLevler.BuildItem.ItemType type = CurrentBuilder.ItemIdList.First(x => x.ItemId == referenceSprite.Item.ItemId).Type;
                    switch (type)
                    {
                        case BuildLevler.BuildItem.ItemType.Start:
                            list = ItemBuildSpritesStart;
                            break;

                        case BuildLevler.BuildItem.ItemType.Summary:
                            list = ItemBuildSpritesSummary;
                            startId = ItemBuildSpritesStart.Count;
                            break;
                    }
                    int ret = list.IndexOf(referenceSprite) + startId;
                    if (referenceSprite.Sprite != null && referenceSprite.Sprite.Sprite != null)
                    {
                        referenceSprite.Sprite.Sprite.Dispose();
                    }
                    if (referenceSprite.Text != null && referenceSprite.Text.Text != null)
                    {
                        referenceSprite.Text.Text.Dispose();
                    }
                    list.Remove(referenceSprite);
                    Console.WriteLine("RemovedBuildSprite: {0}, {1}, {2}", referenceSprite.Item.ItemId, ret, list.Count);
                    ReorderBuildSprites();
                    Rectangle.Visible = false;
                    return ret;
                }

                private static void ReorderBuildSprites()
                {
                    Console.WriteLine("ReOrderBuildSprites Start: " + ItemBuildSpritesStart.Count);
                    for (int index = 0; index < ItemBuildSpritesStart.Count; index++)
                    {
                        int i = 0 + index;
                        var absi = ItemBuildSpritesStart[i];
                        if (absi == null)
                        {
                            Console.WriteLine("Start: Failed");
                        }
                        absi.Sprite.Sprite.PositionUpdate = delegate
                        {
                            return GetItemSlotPositionBuild(i / BuildMaxRow, i % BuildMaxColumn, BuildLevler.BuildItem.ItemType.Start);
                        };
                    }

                    Console.WriteLine("ReOrderBuildSprites Final: " + ItemBuildSpritesSummary.Count);
                    for (int index = 0; index < ItemBuildSpritesSummary.Count; index++)
                    {
                        int i = 0 + index;
                        var absi = ItemBuildSpritesSummary[i];
                        absi.Sprite.Sprite.PositionUpdate = delegate
                        {
                            return GetItemSlotPositionBuild(i / BuildMaxRow, i % BuildMaxColumn, BuildLevler.BuildItem.ItemType.Summary);
                        };
                    }
                }

                public static Vector2 GetItemSlotPositionBuild(int row, int column, BuildLevler.BuildItem.ItemType type)
                {
                    if (type == BuildLevler.BuildItem.ItemType.Start)
                    {
                        return new Vector2(BuildSprite.Sprite.X + BuildStart.X + (BuildIncrement.X * column), BuildSprite.Sprite.Y + BuildStart.Y + (BuildIncrement.Y * row));
                    }
                    else if (type == BuildLevler.BuildItem.ItemType.Final)
                    {
                        return new Vector2(BuildSprite.Sprite.X + BuildFinal.X + (BuildIncrement.X * column), BuildSprite.Sprite.Y + BuildFinal.Y + (BuildIncrement.Y * row));
                    }
                    else if (type == BuildLevler.BuildItem.ItemType.Summary)
                    {
                        return new Vector2(BuildSprite.Sprite.X + BuildFinal.X + (BuildIncrement.X * column), BuildSprite.Sprite.Y + BuildFinal.Y + (BuildIncrement.Y * row));
                    }
                    else
                    {
                        return new Vector2(0, 0);
                    }
                }
            }
        }

    }
}
