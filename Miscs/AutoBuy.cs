using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.GameFiles.Tools.LuaObjReader;
using LeagueSharp.Sandbox;
using LeagueSharp.SDK.Core.Extensions;
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

namespace SAssemblies.Miscs
{
    class AutoBuy
    {

        public static Menu.MenuItemSettings AutoBuyMisc = new Menu.MenuItemSettings(typeof(AutoBuy));

        private int lastGameUpdateTime = 0;
        private static List<BuildLevler> builds = new List<BuildLevler>();
        private AutoBuyGUI Gui = new AutoBuyGUI();
        private int lastClickedElementId = -1;
        private bool shiftActive = false;

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
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyLoadChoice", Language.GetString("MISCS_AUTOBUY_SEQUENCE_BUILD_CHOICE"))
                        .SetValue(GetBuildNames())
                            .DontSave()));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyShowBuild", Language.GetString("MISCS_AUTOBUY_SEQUENCE_BUILD_LOAD")).SetValue(false)
                        .DontSave()));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyNewBuild", Language.GetString("MISCS_AUTOBUY_SEQUENCE_CREATE_CHOICE")).SetValue(false)
                        .DontSave()));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyDeleteBuild", Language.GetString("MISCS_AUTOBUY_SEQUENCE_DELETE_CHOICE")).SetValue(false)
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

            foreach (var sprite in AutoBuyGUI.BuildFrame.ItemBuildSprites)
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
            HandleShopFrameMove(message, cursorPos, key);
            HandleBuildFrameMove(message, cursorPos, key);
            HandleShopFrameClick(message, cursorPos, key);
            HandleBuildFrameClick(message, cursorPos, key);
        }

        private void HandleShopFrameMove(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_MOUSEMOVE || !AutoBuyGUI.ShopFrame.ShopSprite.Sprite.Visible)
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
            if (message != WindowsMessages.WM_MOUSEMOVE || !AutoBuyGUI.BuildFrame.BuildSprite.Sprite.Visible)
            {
                return;
            }
            foreach (var spriteInfo in AutoBuyGUI.BuildFrame.ItemBuildSprites)
            {
                if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                {
                    continue;
                }
                if (Common.IsInside(
                    cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                    spriteInfo.Sprite.Sprite.Height))
                {
                    Console.WriteLine("Hover: " + spriteInfo.Item.Name);
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
            if (message != WindowsMessages.WM_LBUTTONUP || !AutoBuyGUI.ShopFrame.ShopSprite.Sprite.Visible)
            {
                return;
            }
            HandleShopFrameItemClick(message, cursorPos, key);
            HandleShopFrameCategoryClick(message, cursorPos, key);
            HandleShopFrameExitClick(message, cursorPos, key);
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
                    if (lastClickedElementId != -1)
                    {
                        AutoBuyGUI.BuildFrame.AddBuildSprite(spriteInfo, lastClickedElementId);
                        AutoBuyGUI.CurrentBuilder.ItemIdList.Insert(lastClickedElementId, spriteInfo.Item.ItemId);
                    }
                    else
                    {
                        AutoBuyGUI.BuildFrame.AddBuildSprite(spriteInfo);
                        AutoBuyGUI.CurrentBuilder.ItemIdList.Add(spriteInfo.Item.ItemId);
                    }
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
            if (AutoBuyGUI.ShopFrame.ShopSprite == null || AutoBuyGUI.ShopFrame.ShopSprite.Sprite == null)
            {
                return;
            }
            if (Common.IsInside(
                    cursorPos, AutoBuyGUI.ShopFrame.ShopSprite.Sprite.Position + new Vector2(970f, 10f), 40f, 40f))
            {
                ResetMenuEntries();
            }
        }

        private void HandleBuildFrameClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_LBUTTONUP || !AutoBuyGUI.BuildFrame.BuildSprite.Sprite.Visible)
            {
                return;
            }
            if (!shiftActive)
            {
                foreach (var spriteInfo in AutoBuyGUI.BuildFrame.ItemBuildSprites)
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
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < AutoBuyGUI.BuildFrame.ItemBuildSprites.Count; i++)
                {
                    var spriteInfo = AutoBuyGUI.BuildFrame.ItemBuildSprites[i];
                    if (spriteInfo == null || spriteInfo.Sprite == null || spriteInfo.Sprite.Sprite == null)
                    {
                        continue;
                    }
                    if (Common.IsInside(
                        cursorPos, spriteInfo.Sprite.Sprite.Position, spriteInfo.Sprite.Sprite.Width,
                        spriteInfo.Sprite.Sprite.Height))
                    {
                        lastClickedElementId = i;
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

            //TODO: Add buy logic 
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
                String patternBuildOrder = "Build Order(.*?)</section>";
                String patternBuildFinal = "Final Items(.*?)</section>";
                String patternBuildItem = "<small class=\"t-overflow\">([\\S\\s]*?)</small>";
                String patternBuildItemCount = "<span class=\"count\"><span class=\"multiple\">([\\S\\s]*?)</span><span class=\"times\"> &times; </span></span>([\\S\\s]*?$)";

                List<BuildLevler> builds = new List<BuildLevler>();

                for (int i = 0; ; i++)
                {
                    List<String> buildItems = new List<string>();
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
                    buildItems.AddRange(startItems);
                    buildItems.AddRange(buildOrderItems);
                    buildItems.AddRange(finalItems);
                    List<ItemId> itemIdList = new List<ItemId>();
                    foreach (var b in buildItems)
                    {
                        ItemId id = ConvertNameToId(b);
                        if (id != ItemId.Unknown)
                        {
                            itemIdList.Add(id);
                        }
                        else
                        {
                            Console.WriteLine("Unknown: " + b);
                        }
                    }
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
            public List<ItemId> ItemIdList = new List<ItemId>(36);
            public bool New = true;
            public bool Web = false;
            public GameMapId MapId = 0;

            public BuildLevler(string name)
            {
                Name = name;
                ChampionName = ObjectManager.Player.ChampionName;
                ItemIdList = new List<ItemId>();

            }

            public BuildLevler(string name, List<ItemId> itemId, GameMapId mapId)
            {
                Name = name;
                ChampionName = ObjectManager.Player.ChampionName;
                ItemIdList = itemId;
                MapId = mapId;
            }

            public BuildLevler()
            {

            }

        }

        class ItemInfo
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
                    String url = "http://ddragon.leagueoflegends.com/cdn/5.8.1/data/en_US/item.json"; //temp fix for riot fault otherwise + version +
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
                            itemInfo.ToItemIds = token.First["from"].Values<int>().Select(x => (ItemId) x).ToList();
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
                        ; //tags -> String, not available after 5.8.1
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
            }

            public class ShopFrame
            {
                public static SpriteHelper.SpriteInfo ShopSprite;
                public static List<AutoBuySpriteInfo> ItemShopSprites = new List<AutoBuySpriteInfo>();
                public static Render.Rectangle Rectangle = new Render.Rectangle(10, 10, 100, 100, new ColorBGRA(SharpDX.Color.Black.ToColor3(), 0.75f));
                public static Vector2 ShopStart = new Vector2(220, 125);
                public static Vector2 ShopIncrement = new Vector2(64, 64);
                public static Size ShopBlockSize = new Size(48, 48);
                public static int ShopMaxRow = 12;
                public static int ShopMaxColumn = 12;

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
                public static List<AutoBuySpriteInfo> ItemBuildSprites = new List<AutoBuySpriteInfo>();
                public static Render.Rectangle Rectangle = new Render.Rectangle(10, 10, 100, 100, new ColorBGRA(SharpDX.Color.Black.ToColor3(), 0.75f));
                public static Vector2 BuildStart = new Vector2(30, 25);
                public static Vector2 BuildIncrement = new Vector2(64, 64);
                public static Size BuildBlockSize = new Size(48, 48);
                public static int BuildMaxRow = 7;
                public static int BuildMaxColumn = 7;

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

                public static void CreateBuildSprites(BuildLevler build)
                {
                    var itemInfos = build.ItemIdList.Select(x => ItemFrames.First(y => y.Key.ItemId == x)).ToList();
                    foreach (var info in ItemBuildSprites)
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
                    ItemBuildSprites.Clear();
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
                            return GetItemSlotPositionBuild(i / BuildMaxRow, i % BuildMaxColumn);
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
                            return Utils.GetCursorPos() - new Vector2(40 + absi.Text.TextLength.Width, 0); 
                        };
                        absi.Text.Text.Visible = false;
                        absi.Text.Text.Add(4);

                        ItemBuildSprites.Add(absi);
                    }
                    Console.WriteLine("CreateBuildSprites: " + ItemBuildSprites.Count);
                }

                public static void AddBuildSprite(AutoBuySpriteInfo referenceSprite, int id = -1)
                {
                    int i = 0 + ItemBuildSprites.Count;
                    var absi = new AutoBuySpriteInfo(referenceSprite.Item);
                    absi.Sprite = new SpriteHelper.SpriteInfo();
                    absi.Sprite.Bitmap = referenceSprite.Sprite.Bitmap;
                    absi.Sprite.Sprite = new Render.Sprite(referenceSprite.Sprite.Bitmap, new Vector2(0, 0));
                    absi.Sprite.Sprite.Scale = new Vector2(0.75f);
                    absi.Sprite.Sprite.PositionUpdate = delegate
                    {
                        return GetItemSlotPositionBuild(i / BuildMaxRow, i % BuildMaxColumn);
                    };
                    absi.Sprite.Sprite.VisibleCondition = delegate
                    {
                        return IsActive() &&
                            (AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyNewBuild").GetValue<bool>() ||
                            AutoBuyMisc.GetMenuItem("SAssembliesMiscsAutoBuyShowBuild").GetValue<bool>());
                    };
                    absi.Sprite.Sprite.Add(2);
                    if (id != -1)
                    {
                        ItemBuildSprites.Insert(id, absi);
                        ReorderBuildSprites();
                    }
                    else
                    {
                        ItemBuildSprites.Add(absi);
                    }
                    Console.WriteLine("AddedBuildSprite: " + referenceSprite.Item.ItemId);
                }

                public static int DeleteBuildSprite(AutoBuySpriteInfo referenceSprite)
                {
                    int ret = ItemBuildSprites.IndexOf(referenceSprite);
                    referenceSprite.Sprite.Sprite.Dispose();
                    ItemBuildSprites.Remove(referenceSprite);
                    Console.WriteLine("RemovedBuildSprite: {0}, {1}, {2}", referenceSprite.Item.ItemId, ret, ItemBuildSprites.Count);
                    ReorderBuildSprites();
                    return ret;
                }

                private static void ReorderBuildSprites()
                {
                    for (int index = 0; index < ItemBuildSprites.Count; index++)
                    {
                        int i = 0 + index;
                        var absi = ItemBuildSprites[i];
                        absi.Sprite.Sprite.PositionUpdate = delegate
                        {
                            return GetItemSlotPositionBuild(i / BuildMaxRow, i % BuildMaxColumn);
                        };
                    }
                }



                public static Vector2 GetItemSlotPositionBuild(int row, int column)
                {
                    return new Vector2(BuildSprite.Sprite.X + BuildStart.X + (BuildIncrement.X * column), BuildSprite.Sprite.Y + BuildStart.Y + (BuildIncrement.Y * row));
                }
            }
        }

    }
}
