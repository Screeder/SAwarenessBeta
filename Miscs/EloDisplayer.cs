using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Rectangle = SharpDX.Rectangle;

namespace SAwareness.Miscs
{
    class EloDisplayer
    {
        public static Menu.MenuItemSettings EloDisplayerMisc = new Menu.MenuItemSettings(typeof(EloDisplayer));

        private static SpriteHelper.SpriteInfo MainFrame;
        private static SpriteHelper.SpriteInfo RunesSprite;

        private Dictionary<Obj_AI_Hero, ChampionEloDisplayer> _allies = new Dictionary<Obj_AI_Hero,ChampionEloDisplayer>();
        private Dictionary<Obj_AI_Hero, ChampionEloDisplayer> _enemies = new Dictionary<Obj_AI_Hero, ChampionEloDisplayer>();
        private Dictionary<Obj_AI_Hero, TeamEloDisplayer> _teams = new Dictionary<Obj_AI_Hero, TeamEloDisplayer>();

        private bool Ranked = false;

        static EloDisplayer()
        {
            MainFrame = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("EloGui", ref MainFrame, SpriteHelper.TextureType.Default);
            MainFrame.Sprite.PositionUpdate = delegate
            {
                return new Vector2(Drawing.Width / 2 - MainFrame.Bitmap.Width / 2, Drawing.Height / 2 - MainFrame.Bitmap.Height / 2);
            };
            MainFrame.Sprite.VisibleCondition = delegate
            {
                return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAwarenessMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
            };
            MainFrame.Sprite.Add(1);
        }

        public EloDisplayer()
        {
            if (GetRegionPrefix().Equals(""))
                return;

            int index = 0;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if(hero.Name.ToLower().Contains("bot"))
                    continue;

                if (hero.IsEnemy)
                {
                    _enemies.Add(hero, new ChampionEloDisplayer());
                }
                else
                {
                    _allies.Add(hero, new ChampionEloDisplayer());
                }
                //GetSummonerInformations(hero, index);
                index++;
            }
            
            Game.OnGameUpdate += Game_OnGameUpdateAsyncSprites;
            Game.OnGameUpdate += Game_OnGameUpdateAsyncTexts;
            Game.OnGameUpdate += Game_OnGameUpdate;
            new Thread(LoadSpritesAsync).Start();
            new Thread(LoadTextsAsync).Start();
            LoadObjectsSync();
        }

        ~EloDisplayer()
        {
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            EloDisplayerMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_ELODISPLAYER_MAIN"), "SAwarenessMiscsEloDisplayer"));
            EloDisplayerMisc.MenuItems.Add(
                EloDisplayerMisc.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessMiscsEloDisplayerKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(9, KeyBindType.Toggle))));
            EloDisplayerMisc.MenuItems.Add(
                EloDisplayerMisc.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessMiscsEloDisplayerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return EloDisplayerMisc;
        }

        void CalculatePositions(bool calcEnemy)
        {
            Dictionary<Obj_AI_Hero, ChampionEloDisplayer> heroes;
            int index = 0;
            int textFontSize = 20;
            int yOffset = 0;
            int yOffsetTeam = 0;
            if (calcEnemy)
            {
                heroes = _enemies;
                yOffset = 430;
                yOffsetTeam = 380;
            }
            else
            {
                heroes = _allies;
                yOffset = 110;
                yOffsetTeam = 360;
            }
            foreach (var hero in heroes)
            {
                if (hero.Value.SummonerIcon != null && hero.Value.SummonerIcon.Sprite != null && hero.Value.SummonerIcon.Sprite.Sprite != null)
                {
                    hero.Value.SummonerIcon.Position = new Vector2(MainFrame.Sprite.X + 70, MainFrame.Sprite.Y + yOffset + (index * hero.Value.SummonerIcon.Sprite.Sprite.Height));
                    hero.Value.SummonerName.Position = new Vector2(hero.Value.SummonerIcon.Position.X + hero.Value.SummonerIcon.Sprite.Sprite.Width + 10, hero.Value.SummonerIcon.Position.Y);
                    hero.Value.ChampionName.Position = new Vector2(hero.Value.SummonerName.Position.X, hero.Value.SummonerName.Position.Y + textFontSize);
                    hero.Value.Divison.Position = new Vector2(hero.Value.SummonerName.Position.X + 125, hero.Value.SummonerName.Position.Y + textFontSize);
                }
                index++;
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;

            CalculatePositions(true);
            CalculatePositions(false);
        }

        void Game_OnGameUpdateAsyncSprites(EventArgs args)
        {
            if (!IsActive())
                return;

            int index = 0;
            foreach (var ally in _allies)
            {
                Game_OnGameUpdateAsyncSpritesSummary(ally, index, false);
                index++;
            }
            index = 0;
            foreach (var enemy in _enemies)
            {
                Game_OnGameUpdateAsyncSpritesSummary(enemy, index, true);
                index++;
            }
        }

        private void Game_OnGameUpdateAsyncSpritesSummary(KeyValuePair<Obj_AI_Hero, ChampionEloDisplayer> heroPair, int index, bool enemy)
        {
            Obj_AI_Hero hero = heroPair.Key;
            ChampionEloDisplayer champ = heroPair.Value;
            String summonerIcon = champ.SummonerIcon.WebsiteContent;

            if (!summonerIcon.Equals("") && (champ.SummonerIcon == null || champ.SummonerIcon.Sprite == null || !champ.SummonerIcon.Sprite.DownloadFinished))
            {
                SpriteHelper.LoadTexture(summonerIcon, ref champ.SummonerIcon.Sprite, @"EloDisplayer\");
            }
            if (champ.SummonerIcon != null && champ.SummonerIcon.Sprite != null && champ.SummonerIcon.Sprite.DownloadFinished && !champ.SummonerIcon.Sprite.LoadingFinished)
            {
                champ.SummonerIcon.Sprite.Sprite.Scale = new Vector2(0.35f, 0.35f);
                champ.SummonerIcon.Sprite.Sprite.PositionUpdate = delegate
                {
                    return champ.SummonerIcon.Position;
                };
                champ.SummonerIcon.Sprite.Sprite.VisibleCondition = delegate
                {
                    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAwarenessMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
                };
                champ.SummonerIcon.Sprite.Sprite.Add(2);
                champ.SummonerIcon.Sprite.LoadingFinished = true;
            }
        }

        void Game_OnGameUpdateAsyncTexts(EventArgs args)
        {
            if (!IsActive())
                return;

            int index = 0;
            foreach (var ally in _allies)
            {
                Game_OnGameUpdateAsyncTextsSummary(ally, index, false);
                index++;
            }
            index = 0;
            foreach (var enemy in _enemies)
            {
                Game_OnGameUpdateAsyncTextsSummary(enemy, index, true);
                index++;
            }
        }

        private void Game_OnGameUpdateAsyncTextsSummary(KeyValuePair<Obj_AI_Hero, ChampionEloDisplayer> heroPair, int index, bool enemy)
        {
            Obj_AI_Hero hero = heroPair.Key;
            ChampionEloDisplayer champ = heroPair.Value;
            if (champ.Divison.FinishedLoading != true)
            {
                champ.Divison.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
                champ.Divison.Text.TextUpdate = delegate
                {
                    if (!champ.Divison.WebsiteContent.Equals(""))
                    {
                        return champ.Divison.WebsiteContent;
                    }
                    return "";
                };
                champ.Divison.Text.PositionUpdate = delegate
                {
                    return champ.Divison.Position;
                };
                champ.Divison.Text.VisibleCondition = sender =>
                {
                    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAwarenessMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
                };
                champ.Divison.Text.OutLined = true;
                champ.Divison.Text.Add(4);
                champ.Divison.FinishedLoading = true;
            }
        }

        private void LoadObjectsSync()
        {
            int index = 0;
            foreach (var ally in _allies)
            {
                LoadObjectsSyncSummary(ally, index, false);
                index++;
            }
            index = 0;
            foreach (var enemy in _enemies)
            {
                LoadObjectsSyncSummary(enemy, index, true);
                index++;
            }
        }

        private void LoadObjectsSyncSummary(KeyValuePair<Obj_AI_Hero, ChampionEloDisplayer> heroPair, int index, bool enemy)
        {
            Obj_AI_Hero hero = heroPair.Key;
            ChampionEloDisplayer champ = heroPair.Value;
            champ.SummonerName.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            champ.SummonerName.Text.TextUpdate = delegate
            {
                return hero.Name;
            };
            champ.SummonerName.Text.PositionUpdate = delegate
            {
                return champ.SummonerName.Position;
            };
            champ.SummonerName.Text.VisibleCondition = sender =>
            {
                return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAwarenessMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
            };
            champ.SummonerName.Text.OutLined = true;
            //champ.SummonerName.Text.Centered = true;
            champ.SummonerName.Text.Add(4);

            champ.ChampionName.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            champ.ChampionName.Text.TextUpdate = delegate
            {
                return hero.ChampionName;
            };
            champ.ChampionName.Text.PositionUpdate = delegate
            {
                return champ.ChampionName.Position;
            };
            champ.ChampionName.Text.VisibleCondition = sender =>
            {
                return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAwarenessMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
            };
            champ.ChampionName.Text.OutLined = true;
            //champ.ChampionName.Text.Centered = true;
            champ.ChampionName.Text.Add(4);
        }

        private void LoadSpritesAsync()
        {
            foreach (var ally in _allies)
            {
                ally.Value.SummonerIcon.WebsiteContent = GetSummonerIcon(ally.Key);
                SpriteHelper.DownloadImageOpGg(ally.Value.SummonerIcon.WebsiteContent, @"EloDisplayer\");
            }
            foreach (var enemy in _enemies)
            {
                enemy.Value.SummonerIcon.WebsiteContent = GetSummonerIcon(enemy.Key);
                SpriteHelper.DownloadImageOpGg(enemy.Value.SummonerIcon.WebsiteContent, @"EloDisplayer\");
            }
        }

        private void LoadTextsAsync()
        {
            foreach (var ally in _allies)
            {
                ally.Value.Divison.WebsiteContent = GetDivision(ally.Key);
            }
            foreach (var enemy in _enemies)
            {
                enemy.Value.Divison.WebsiteContent = GetDivision(enemy.Key);
            }
        }

        private void GetSummonerInformations(Obj_AI_Hero hero, int index) //TODO: Get Positions
        {
            string website = GetLolWebSiteContentOverview(hero);

            //String summonerIcon = GetSummonerIcon(hero);
            //SpriteHelper.DownloadImageOpGg(summonerIcon, @"EloDisplayer\");
            //SummonerIcon[index].Sprite = new SpriteHelper.SpriteInfo();
            //SpriteHelper.LoadTexture(summonerIcon, ref SummonerIcon[index], @"EloDisplayer\");
            //SummonerIcon[index].Sprite.Scale = new Vector2(0.4f, 0.4f);
            //SummonerIcon[index].Sprite.PositionUpdate = delegate
            //{
            //    return new Vector2(MainFrame.Sprite.X + 80, MainFrame.Sprite.Y + 115);
            //};
            //SummonerIcon[index].Sprite.VisibleCondition = delegate
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAwarenessMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
            //};
            //SummonerIcon[index].Sprite.Add(2);

            //ChampionName[index] = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //ChampionName[index].TextUpdate = delegate
            //{
            //    return hero.ChampionName;
            //};
            //ChampionName[index].PositionUpdate = delegate
            //{
            //    return new Vector2(MainFrame.Sprite.X + 200, MainFrame.Sprite.Y + 150);
            //};
            //ChampionName[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAwarenessMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
            //};
            //ChampionName[index].OutLined = true;
            //ChampionName[index].Centered = true;
            //ChampionName[index].Add(4);

            //SummonerName[index] = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //SummonerName[index].TextUpdate = delegate
            //{
            //    return hero.Name;
            //};
            //SummonerName[index].PositionUpdate = delegate
            //{
            //    return new Vector2(MainFrame.Sprite.X + 200, MainFrame.Sprite.Y + 170);
            //};
            //SummonerName[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAwarenessMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
            //};
            //SummonerName[index].OutLined = true;
            //SummonerName[index].Centered = true;
            //SummonerName[index].Add(4);

            //Divison[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //Divison[index].TextUpdate = delegate
            //{
            //    return GetDivision(hero);
            //};
            ////Divison[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //Divison[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //Divison[index].OutLined = true;
            //Divison[index].Centered = true;
            //Divison[index].Add();

            //RankedStatistics[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //RankedStatistics[index].TextUpdate = delegate
            //{
            //    return GetRankedStatistics(hero);
            //};
            ////RankedStatistics[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //RankedStatistics[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //RankedStatistics[index].OutLined = true;
            //RankedStatistics[index].Centered = true;
            //RankedStatistics[index].Add();

            //RecentStatistics[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //RecentStatistics[index].TextUpdate = delegate
            //{
            //    return GetRecentStatistics(hero);
            //};
            ////RecentStatistics[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //RecentStatistics[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //RecentStatistics[index].OutLined = true;
            //RecentStatistics[index].Centered = true;
            //RecentStatistics[index].Add();

            //MMR[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //MMR[index].TextUpdate = delegate
            //{
            //    return GetMmr(hero);
            //};
            ////MMR[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //MMR[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //MMR[index].OutLined = true;
            //MMR[index].Centered = true;
            //MMR[index].Add();

            //Masteries[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //Masteries[index].TextUpdate = delegate
            //{
            //    return GetMasteries(hero);
            //};
            ////Masteries[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //Masteries[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //Masteries[index].OutLined = true;
            //Masteries[index].Centered = true;
            //Masteries[index].Add();

            //Runes[index] = new Render.Text(0, 0, "Click here", 14, SharpDX.Color.Orange);
            //Runes[index].TextUpdate = delegate
            //{
            //    return GetRunes(hero);
            //};
            ////Runes[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //Runes[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //Runes[index].OutLined = true;
            //Runes[index].Centered = true;
            //Runes[index].Add();

            //OverallKDA[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //OverallKDA[index].TextUpdate = delegate
            //{
            //    return GetOverallKDA(hero);
            //};
            ////OverallKDA[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //OverallKDA[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //OverallKDA[index].OutLined = true;
            //OverallKDA[index].Centered = true;
            //OverallKDA[index].Add();

            //ChampionKDA[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //ChampionKDA[index].TextUpdate = delegate
            //{
            //    if (GetChampionKDALastSeason(hero).Equals(""))
            //        return GetChampionKDANormal(hero);
            //    return GetChampionKDALastSeason(hero);
            //};
            ////ChampionKDA[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //ChampionKDA[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //ChampionKDA[index].OutLined = true;
            //ChampionKDA[index].Centered = true;
            //ChampionKDA[index].Add();

            //ChampionGames[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //ChampionGames[index].TextUpdate = delegate
            //{
            //    if (GetChampionGamesLastSeason(hero).Equals(""))
            //        return GetChampionGamesNormal(hero);
            //    return GetChampionGamesLastSeason(hero);
            //};
            ////ChampionGames[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //ChampionGames[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //ChampionGames[index].OutLined = true;
            //ChampionGames[index].Centered = true;
            //ChampionGames[index].Add();

            //ChampionWinRate[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //ChampionWinRate[index].TextUpdate = delegate
            //{
            //    return GetChampionGamesLastSeason(hero);
            //};
            ////ChampionWinRate[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //ChampionWinRate[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //ChampionWinRate[index].OutLined = true;
            //ChampionWinRate[index].Centered = true;
            //ChampionWinRate[index].Add();

            ///////Extra windows

            //RunesSpriteText[index] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            //RunesSpriteText[index].TextUpdate = delegate
            //{
            //    return GetRunes(hero);
            //};
            ////RunesSpriteText[index].PositionUpdate = delegate
            ////{
            ////    return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            ////};
            //RunesSpriteText[index].VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive();
            //};
            //RunesSpriteText[index].OutLined = true;
            //RunesSpriteText[index].Centered = true;
            //RunesSpriteText[index].Add();
        }

        private String GetLolWebSiteContent(String webSite)
        {
            return GetLolWebSiteContent(webSite, null);
        }

        private String GetLolWebSiteContent(String webSite, String param)
        {
            return GetWebSiteContent(GetWebSite() + webSite, param);
        }

        private String GetWebSiteContent(String webSite, String param = null)
        {
            string website = "";
            var request = (HttpWebRequest)WebRequest.Create(webSite);
            if (param != null)
            {
                Byte[] bytes = GetBytes(param);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(bytes, 0, bytes.Length);
                dataStream.Close();
            }
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream receiveStream = response.GetResponseStream();
                        if (receiveStream != null)
                        {
                            if (response.CharacterSet == null)
                            {
                                using (StreamReader readStream = new StreamReader(receiveStream))
                                {
                                    website = readStream.ReadToEnd();
                                }
                            }
                            else
                            {
                                using (
                                    StreamReader readStream = new StreamReader(receiveStream,
                                        Encoding.GetEncoding(response.CharacterSet)))
                                {
                                    website = readStream.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load EloDisplayer Data. Exception: " + ex.ToString());
            }
            return website;
        }

        private String GetLolWebSiteContentOverview(Obj_AI_Hero hero)
        {
            string playerName = GetEncodedPlayerName(hero);
            return GetLolWebSiteContent("summoner/userName=" + playerName);
        }

        private String GetLolWebSiteContentChampions(Obj_AI_Hero hero)
        {
            string playerName = GetEncodedPlayerName(hero);
            return GetLolWebSiteContent("summoner/champions/userName=" + playerName);
        }

        private String GetLolWebSiteContentRunes(Obj_AI_Hero hero)
        {
            string playerName = GetEncodedPlayerName(hero);
            return GetLolWebSiteContent("summoner/rune/userName=" + playerName);
        }

        private String GetEncodedPlayerName(Obj_AI_Hero hero)
        {
            return HttpUtility.UrlEncode(hero.Name);
        }

        private String GetWebSite()
        {
            String prefix = GetRegionPrefix();
            if (prefix == "")
                return "http://op.gg/";
            else
                return "http://" + prefix + ".op.gg/";
        }

        private String GetRegionPrefix()
        {
            switch (Game.Region.ToLower())
            {
                case "euw1":
                    return "euw";

                case "eun":
                    return "eune";

                case "la1":
                    return "lan";

                case "la2":
                    return "las";

                case "oc1":
                    return "oce";

                case "kr":
                    return "";

                default:
                    return Game.Region.ToLower();
            }
        }

        private String GetSummonerIcon(Obj_AI_Hero hero)
        {
            String websiteContent = GetLolWebSiteContentOverview(hero);
            String patternWin = "<div class=\"rectImage\">\\n.*<img src=\"//ss.op.gg/images/profile_icons/(.*?)\">\\n.*</div>";
            return GetMatch(websiteContent, patternWin);
        }

        private String GetDivision(Obj_AI_Hero hero)
        {
            String division = "";
            String websiteContent = GetLolWebSiteContentOverview(hero);
            if (websiteContent.Contains("TierRank") && websiteContent.Contains("LeaguePoints"))
            {
                if (websiteContent.Contains("TypeTeam"))
                {
                    String patternRank = "<span class=\"tierRank\">(.*?)</span>";
                    String patternPoints = "<span class=\"leaguePoints\">(.*?) LP</span>";
                    division = GetMatch(websiteContent, patternRank) + " (" + GetMatch(websiteContent, patternPoints) + " LP)";
                    //TODO: GetBestRank();
                }
                else
                {
                    String patternRank = "<span class=\"tierRank\">(.*?)</span>";
                    String patternPoints = "<span class=\"leaguePoints\">(.*?) LP</span>";
                    division = GetMatch(websiteContent, patternRank) + " (" + GetMatch(websiteContent, patternPoints) + " LP)";
                }
                Ranked = true;
            } 
            else if (websiteContent.Contains("TierRank") && !websiteContent.Contains("LeaguePoints"))
            {
                division = "Unranked";
            }
            else if (!websiteContent.Contains("TierRank"))
            {
                division = "Unranked (<30)";
            }
            return division;
        }

        private String GetRankedStatistics(Obj_AI_Hero hero)
        {
            if (!Ranked)
                return "";

            String websiteContent = GetLolWebSiteContentOverview(hero);
            String patternWin = "<br>\\n.*<span class=\"wins\">(.*?)W</span>";
            String patternLoose = "</span>\\n.*<span class=\"losses\">(.*?)L</span>";
            return GetMatch(websiteContent, patternWin) + "/" +
                  GetMatch(websiteContent, patternLoose);
        }

        private String GetRecentStatistics(Obj_AI_Hero hero)
        {
            String websiteContent = GetLolWebSiteContentOverview(hero);
            String patternWl = "<div class=\"WinRatioTitle\">(\n.*){3}(.*?)\n.*</div>";
            String matchWl = GetMatch(websiteContent, patternWl, 0, 2); //TODO: Check if rly second group or 4
            String patternWin = "\\S(.*?)W";
            String patternLoose = "\\S(.*?)L";
            return GetMatch(matchWl, patternWin) + "/" +
                  GetMatch(matchWl, patternLoose);
        }

        private String GetOverallKDA(Obj_AI_Hero hero)
        {
            String websiteContent = GetLolWebSiteContentOverview(hero);
            String patternKill = "<div class=\"KDA\">\\n.*<div class=\"kda\">\\n.*<span class=\"kill\">(.*?)</span>";
            String patternDeath = "<div class=\"KDA\">\\n.*<div class=\"kda\">\\n.*<span class=\"death\">(.*?)</span>";
            String patternAssist = "<div class=\"KDA\">\\n.*<div class=\"kda\">\\n.*<span class=\"assist\">(.*?)</span>";
            return GetMatch(websiteContent, patternKill) + "/" +
                  GetMatch(websiteContent, patternDeath) + "/" +
                  GetMatch(websiteContent, patternAssist);
        }

        private String GetMmr(Obj_AI_Hero hero) //Need check if serialize and deserialize works//Test with Moopz1//my gives error
        {
            if (!Ranked)
                return "";

            String mmrUrl = GetWebSite() + "summoner/ajax/update.json/";
            try
            {
                ResponseMmr responseMmr = GetJSonResponse<ResponseMmr>(mmrUrl, new RequestMmr(GetEncodedPlayerName(hero)));
                return responseMmr.mmr;
            }
            catch (Exception) {}
            return "";
        }

        class RequestMmr
        {
            public String userName;

            public RequestMmr(String userName)
            {
                this.userName = userName;
            }
        }

        class ResponseMmr
        {
            public String log;
            public Tip tip;
            public String mmr;
            public String Class;

            public ResponseMmr(String log, Tip tip, String mmr, String Class)
            {
                this.log = log;
                this.tip = tip;
                this.mmr = mmr;
                this.Class = Class;
            }

            public class Tip
            {
                public String status;
                public String leagueAverage;
                public String notice;

                public Tip(String status, String leagueAverage, String notice)
                {
                    this.status = status;
                    this.leagueAverage = leagueAverage;
                    this.notice = notice;
                }
            }
        }

        private String GetMasteries(Obj_AI_Hero hero)
        {
            int offense = 0;
            int defense = 0;
            int utility = 0;
            for (int i = 0; i < hero.Masteries.Count(); i++)
            {
                var mastery = hero.Masteries[i];
                if (i < 20)
                {
                    offense += mastery.Points;
                } 
                else if (i <= 20 && i < 39)
                {
                    defense += mastery.Points;
                }
                else
                {
                    utility += mastery.Points;
                }
            }
            GenerateMasteryPage(hero);
            return offense + "/" + defense + "/" + utility;
        }

        private String GetRunes(Obj_AI_Hero hero)
        {
            String runes = "";
            String patternActiveRuneSite = "data-page=(.*?)]";
            String matchActiveRuneSite = GetMatch(GetLolWebSiteContentRunes(hero), patternActiveRuneSite);
            String patternInnerRunePage = "<div class=\"Title\">(.*?)</div>\n.*<div class=\"Stat\">(.*?)</div>";
            String patternOuterRunePage =
                "<div class=\"RunePageWrap\" id=\"SummonerRunePage-" + matchActiveRuneSite + "\"([\\s\\S]*?)</dd>(.*?)</dl>.*\\n</div>(.*?)</div>";
            String matchOuterRunePage = GetMatch(GetLolWebSiteContentRunes(hero), patternOuterRunePage);
            for (int i = 0; ; i++)
            {
                String matchInnerRunePageTitle = GetMatch(matchOuterRunePage, patternOuterRunePage, i, 1);
                String matchInnerRunePageStat = GetMatch(matchOuterRunePage, patternOuterRunePage, i, 2);
                if (matchInnerRunePageTitle.Equals("") || matchInnerRunePageStat.Equals(""))
                {
                    break;
                }
                runes += matchInnerRunePageTitle + ": " + matchInnerRunePageStat + "\n";
            }
            return runes;
        }

        private String GetChampionKDA(Obj_AI_Hero hero, String season)
        {
            String championContent = GetLolWebSiteContent("summoner/champions/ajax/champions.json/", "summonerId=" + GetSummonerId(GetLolWebSiteContentOverview(hero)) + "&season=" + season + "&type=stats");
            String patternChampion = "<div class=\"championName\"> (.*?) </div>";
            String patternKill = "<span class=\"kill\">(.*?)</span>";
            String patternDeath = "<span class=\"death\">(.*?)</span>";
            String patternAssist = "<span class=\"assist\">(.*?)</span>";
            String matchKill = "";
            String matchDeath = "";
            String matchAssist = "";

            for (int i = 0; ; i++)
            {
                String matchChampion = GetMatch(championContent, patternChampion, i);
                if (matchChampion.Contains(hero.ChampionName))
                {
                    matchKill = GetMatch(championContent, patternKill, i);
                    matchDeath = GetMatch(championContent, patternDeath, i);
                    matchAssist = GetMatch(championContent, patternAssist, i);
                    break;
                }
                else if (matchChampion.Equals(""))
                {
                    break;
                }
            }
            return matchKill + "/" + matchDeath + "/" + matchAssist;
        }

        private String GetChampionKDALastSeason(Obj_AI_Hero hero)
        {
            if (!Ranked)
                return "";

            return GetChampionKDA(hero, "4");
        }

        private String GetChampionKDANormal(Obj_AI_Hero hero)
        {
            return GetChampionKDA(hero, "normal");
        }

        private String GetChampionGames(Obj_AI_Hero hero, String season)
        {
            String championContent = GetLolWebSiteContent("summoner/champions/ajax/champions.json/", "summonerId=" + GetSummonerId(GetLolWebSiteContentOverview(hero)) + "&season=" + season + "&type=stats");
            String patternChampion = "<div class=\"championName\"> (.*?) </div>";
            String patternWins = "<span class=\"wins\"> (.*?) </span>";
            String patternLosses = "<span class=\"losses\"> (.*?) </span>";
            String matchWins = "";
            String matchLosses = "";

            for (int i = 0; ; i++)
            {
                String matchChampion = GetMatch(championContent, patternChampion, i);
                if (matchChampion.Contains(hero.ChampionName))
                {
                    matchWins = GetMatch(championContent, patternWins, i);
                    matchLosses = GetMatch(championContent, patternLosses, i);
                    break;
                }
                else if (matchChampion.Equals(""))
                {
                    break;
                }
            }
            return matchWins + "/" + matchLosses;
        }

        private String GetChampionGamesLastSeason(Obj_AI_Hero hero)
        {
            if (!Ranked)
                return "";

            return GetChampionGames(hero, "4");
        }

        private String GetChampionGamesNormal(Obj_AI_Hero hero)
        {
            return GetChampionGames(hero, "normal");
        }

        private void CalculateTeamStats(String websiteContent)
        {
            
        }

        private void GetTeamBans(Obj_AI_Hero hero) //TODO: Create pattern for bans.
        {
            string playerName = HttpUtility.UrlEncode(hero.Name);
            String championContent = GetLolWebSiteContent("summoner/ajax/spectator/", "userName=" + playerName + "&force=true");
            //String patternChampion = "<div class=\"championName\"> (.*?) </div>";
            //String patternKill = "<span class=\"kill\">(.*?)</span>";
            //String patternDeath = "<span class=\"death\">(.*?)</span>";
            //String patternAssist = "<span class=\"assist\">(.*?)</span>";
            //String matchKill = "";
            //String matchDeath = "";
            //String matchAssist = "";

            //for (int i = 0; ; i++)
            //{
            //    String matchChampion = GetMatch(championContent, patternChampion, i);
            //    if (matchChampion.Contains(championName))
            //    {
            //        matchKill = GetMatch(championContent, patternKill, i);
            //        matchDeath = GetMatch(championContent, patternDeath, i);
            //        matchAssist = GetMatch(championContent, patternAssist, i);
            //        break;
            //    }
            //    else if (matchChampion.Equals(""))
            //    {
            //        break;
            //    }
            //}
            //return matchKill + "/" + matchDeath + "/" + matchAssist;
        }

        private void UpdateStatus(String websiteContent)
        {
            String updateUrl = GetWebSite() + "summoner/ajax/update.json/summonerId=" + GetSummonerId(websiteContent);
            WebRequest.Create(updateUrl).GetResponse();
        }

        private String GetSummonerId(String websiteContent)
        {
            //data-summoner-id=19491264
            String pattern = "data-summoner-id=\"(.*?)\"";
            return GetMatch(websiteContent, pattern);
        }


        private String GetMatch(String websiteContent, String pattern, int index = 0, int groupIndex = 1)
        {
            Match websiteMatcher = new Regex(@pattern).Matches(websiteContent)[0];
            Match elementMatch = new Regex(websiteMatcher.Groups[groupIndex].ToString()).Matches(websiteContent)[index];
            return elementMatch.ToString();
        }

        private static T FromJson<T>(string input)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(input);
        }

        private static string ToJson(object input)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(input);
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private T GetJSonResponse<T>(String url, object request)
        {
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            String json = ToJson(request);
            Byte[] bytes = GetBytes(json);
            webRequest.ContentLength = bytes.Length;
            Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();
            WebResponse response = webRequest.GetResponse();
            Stream data = response.GetResponseStream();
            data.Read(bytes, 0, (int)data.Length);
            try
            {
                return FromJson<T>(GetString(bytes));
            }
            finally
            {
                response.Close();
            }
        }

        private Bitmap GenerateMasteryPage(Obj_AI_Hero hero)
        {
            String masteryPage = "http://leaguecraft.com/masteries/iframe/?points=";
            foreach (var mastery in hero.Masteries)
            {
                masteryPage += mastery.Points;
            }
            return CreateScreenShot(masteryPage);
        }

        private Bitmap CreateScreenShot(String url, int width = -1, int height = -1) //For iframe of masteries
        {
            WebBrowser webBrowser = new WebBrowser();
            webBrowser.ScrollBarsEnabled = false;
            webBrowser.ScriptErrorsSuppressed = true;
            webBrowser.Navigate(url);
            while (webBrowser.ReadyState != WebBrowserReadyState.Complete) { Application.DoEvents(); }

            webBrowser.Width = width;
            webBrowser.Height = height;

            if (width == -1)
            {
                webBrowser.Width = webBrowser.Document.Body.ScrollRectangle.Width;
            }

            if (height == -1)
            {
                webBrowser.Height = webBrowser.Document.Body.ScrollRectangle.Height;
            }

            Bitmap bitmap = new Bitmap(webBrowser.Width, webBrowser.Height);
            webBrowser.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, webBrowser.Width, webBrowser.Height));
            webBrowser.Dispose();

            return bitmap;
        }

        private string GetChampionId(Obj_AI_Hero hero)
        {
            switch (hero.ChampionName)
            {
                case "Aatrox":
                    {
                        return "117";
                    }
                case "Ahri":
                    {
                        return "86";
                    }
                case "Akali":
                    {
                        return "74";
                    }
                case "Alistar":
                    {
                        return "11";
                    }
                case "Amumu":
                    {
                        return "31";
                    }
                case "Anivia":
                    {
                        return "33";
                    }
                case "Annie":
                    {
                        return "0";
                    }
                case "Ashe":
                    {
                        return "21";
                    }
                case "Azir":
                    {
                        return "119";
                    }
                case "Blitzcrank":
                    {
                        return "48";
                    }
                case "Brand":
                    {
                        return "58";
                    }
                case "Braum":
                    {
                        return "112";
                    }
                case "Caitlyn":
                    {
                        return "47";
                    }
                case "Cassiopeia":
                    {
                        return "62";
                    }
                case "Cho'Gath":
                    {
                        return "30";
                    }
                case "Corki":
                    {
                        return "41";
                    }
                case "Darius":
                    {
                        return "101";
                    }
                case "Diana":
                    {
                        return "104";
                    }
                case "Dr. Mundo":
                    {
                        return "35";
                    }
                case "Draven":
                    {
                        return "98";
                    }
                case "Elise":
                    {
                        return "55";
                    }
                case "Evelynn":
                    {
                        return "27";
                    }
                case "Ezreal":
                    {
                        return "71";
                    }
                case "Fiddlesticks":
                    {
                        return "8";
                    }
                case "Fiora":
                    {
                        return "95";
                    }
                case "Fizz":
                    {
                        return "88";
                    }
                case "Galio":
                    {
                        return "2";
                    }
                case "Gangplank":
                    {
                        return "40";
                    }
                case "Garen":
                    {
                        return "76";
                    }
                case "Gnar":
                    {
                        return "108";
                    }
                case "Gragas":
                    {
                        return "69";
                    }
                case "Graves":
                    {
                        return "87";
                    }
                case "Hecarim":
                    {
                        return "99";
                    }
                case "Heimerdinger":
                    {
                        return "64";
                    }
                case "Irelia":
                    {
                        return "38";
                    }
                case "Janna":
                    {
                        return "39";
                    }
                case "Jarvan IV":
                    {
                        return "54";
                    }
                case "Jax":
                    {
                        return "23";
                    }
                case "Jayce":
                    {
                        return "102";
                    }
                case "Jinx":
                    {
                        return "113";
                    }
                case "Kalista":
                    {
                        return "121";
                    }
                case "Karma":
                    {
                        return "42";
                    }
                case "Karthus":
                    {
                        return "29";
                    }
                case "Kassadin":
                    {
                        return "37";
                    }
                case "Katarina":
                    {
                        return "50";
                    }
                case "Kayle":
                    {
                        return "9";
                    }
                case "Kennen":
                    {
                        return "75";
                    }
                case "Kha'Zix":
                    {
                        return "100";
                    }
                case "Kog'Maw":
                    {
                        return "81";
                    }
                case "LeBlanc":
                    {
                        return "6";
                    }
                case "Lee Sin":
                    {
                        return "59";
                    }
                case "Leona":
                    {
                        return "77";
                    }
                case "Lissandra":
                    {
                        return "103";
                    }
                case "Lucian":
                    {
                        return "114";
                    }
                case "Lulu":
                    {
                        return "97";
                    }
                case "Lux":
                    {
                        return "83";
                    }
                case "Malphite":
                    {
                        return "49";
                    }
                case "Malzahar":
                    {
                        return "78";
                    }
                case "Maokai":
                    {
                        return "52";
                    }
                case "Master Yi":
                    {
                        return "10";
                    }
                case "Miss Fortune":
                    {
                        return "20";
                    }
                case "Mordekaiser":
                    {
                        return "72";
                    }
                case "Morgana":
                    {
                        return "24";
                    }
                case "Nami":
                    {
                        return "118";
                    }
                case "Nasus":
                    {
                        return "65";
                    }
                case "Nautilus":
                    {
                        return "92";
                    }
                case "Nidalee":
                    {
                        return "66";
                    }
                case "Nocturne":
                    {
                        return "51";
                    }
                case "Nunu":
                    {
                        return "19";
                    }
                case "Olaf":
                    {
                        return "1";
                    }
                case "Orianna":
                    {
                        return "56";
                    }
                case "Pantheon":
                    {
                        return "70";
                    }
                case "Poppy":
                    {
                        return "68";
                    }
                case "Quinn":
                    {
                        return "105";
                    }
                case "Rammus":
                    {
                        return "32";
                    }
                case "Renekton":
                    {
                        return "53";
                    }
                case "Rengar":
                    {
                        return "90";
                    }
                case "Riven":
                    {
                        return "80";
                    }
                case "Rumble":
                    {
                        return "61";
                    }
                case "Ryze":
                    {
                        return "12";
                    }
                case "Sejuani":
                    {
                        return "94";
                    }
                case "Shaco":
                    {
                        return "34";
                    }
                case "Shen":
                    {
                        return "82";
                    }
                case "Shyvana":
                    {
                        return "85";
                    }
                case "Singed":
                    {
                        return "26";
                    }
                case "Sion":
                    {
                        return "13";
                    }
                case "Sivir":
                    {
                        return "14";
                    }
                case "Skarner":
                    {
                        return "63";
                    }
                case "Sona":
                    {
                        return "36";
                    }
                case "Soraka":
                    {
                        return "15";
                    }
                case "Swain":
                    {
                        return "46";
                    }
                case "Syndra":
                    {
                        return "106";
                    }
                case "Talon":
                    {
                        return "79";
                    }
                case "Taric":
                    {
                        return "43";
                    }
                case "Teemo":
                    {
                        return "16";
                    }
                case "Thresh":
                    {
                        return "120";
                    }
                case "Tristana":
                    {
                        return "17";
                    }
                case "Trundle":
                    {
                        return "45";
                    }
                case "Tryndamere":
                    {
                        return "22";
                    }
                case "Twisted Fate":
                    {
                        return "3";
                    }
                case "Twitch":
                    {
                        return "28";
                    }
                case "Udyr":
                    {
                        return "67";
                    }
                case "Urgot":
                    {
                        return "5";
                    }
                case "Varus":
                    {
                        return "91";
                    }
                case "Vayne":
                    {
                        return "60";
                    }
                case "Veigar":
                    {
                        return "44";
                    }
                case "Vel'Koz":
                    {
                        return "111";
                    }
                case "Vi":
                    {
                        return "116";
                    }
                case "Viktor":
                    {
                        return "93";
                    }
                case "Vladimir":
                    {
                        return "7";
                    }
                case "Volibear":
                    {
                        return "89";
                    }
                case "Warwick":
                    {
                        return "18";
                    }
                case "Wukong":
                    {
                        return "57";
                    }
                case "Xerath":
                    {
                        return "84";
                    }
                case "Xin Zhao":
                    {
                        return "4";
                    }
                case "Yasuo":
                    {
                        return "110";
                    }
                case "Yorick":
                    {
                        return "73";
                    }
                case "Zac":
                    {
                        return "109";
                    }
                case "Zed":
                    {
                        return "115";
                    }
                case "Ziggs":
                    {
                        return "96";
                    }
                case "Zilean":
                    {
                        return "25";
                    }
                case "Zyra":
                    {
                        return "107";
                    }
            }
            return "";
        }

        class ChampionEloDisplayer
        {
            public TextInfo SummonerIcon = new TextInfo();
            public TextInfo ChampionName = new TextInfo();
            public TextInfo SummonerName = new TextInfo();
            public TextInfo Divison = new TextInfo();
            public TextInfo RankedStatistics = new TextInfo();
            public TextInfo RecentStatistics = new TextInfo();
            public TextInfo MMR = new TextInfo();
            public TextInfo Masteries = new TextInfo(); //http://leaguecraft.com/masteries/iframe/?points=140003001103130003010202031010000000000000000000000000000
            public TextInfo Runes = new TextInfo(); //http://leaguecraft.com/runes/?marks=1,8,14,14,28,29,29,89,89&seals=16,16,16,16,16,16,16,16,295&glyphs=12,12,75,75,75,75,75,75,75&quints=296,293,288
            public TextInfo OverallKDA = new TextInfo();
            public TextInfo ChampionKDA = new TextInfo();
            public TextInfo ChampionGames = new TextInfo();
            public TextInfo ChampionWinRate = new TextInfo();

            public TextInfo MasteriesSprite = new TextInfo();
            public TextInfo RunesSpriteText = new TextInfo();
        }

        class TeamEloDisplayer
        {
            public TextInfo TeamBans = new TextInfo();
            public TextInfo TeamDivison = new TextInfo();
            public TextInfo TeamRankedStatistics = new TextInfo();
            public TextInfo TeamRecentStatistics = new TextInfo();
            public TextInfo TeamMMR = new TextInfo();
            public TextInfo TeamChampionGames = new TextInfo();
        }

        class TextInfo
        {
            public bool FinishedLoading = false;
            public String WebsiteContent = "";

            public SpriteHelper.SpriteInfo Sprite;
            public Render.Text Text;
            public Vector2 Position;
        }
    }
}
