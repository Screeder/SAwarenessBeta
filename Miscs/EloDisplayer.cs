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
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;
using Rectangle = SharpDX.Rectangle;

namespace SAssemblies.Miscs
{
    class EloDisplayer
    {
        public static Menu.MenuItemSettings EloDisplayerMisc = new Menu.MenuItemSettings(typeof(EloDisplayer));

        private static SpriteHelper.SpriteInfo MainFrame;
        private static SpriteHelper.SpriteInfo RunesSprite;

        private Dictionary<Obj_AI_Hero, ChampionEloDisplayer> _allies = new Dictionary<Obj_AI_Hero,ChampionEloDisplayer>();
        private Dictionary<Obj_AI_Hero, ChampionEloDisplayer> _enemies = new Dictionary<Obj_AI_Hero, ChampionEloDisplayer>();
        private Dictionary<Obj_AI_Hero, TeamEloDisplayer> _teams = new Dictionary<Obj_AI_Hero, TeamEloDisplayer>();

        private int lastGameUpdateTime = 0;
        private int lastGameUpdateSpritesTime = 0;
        private int lastGameUpdateTextsTime = 0;

        private TextInfo Header = new TextInfo();
        private TextInfo SummarizedAlly = new TextInfo();
        private TextInfo SummarizedEnemy = new TextInfo();

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
                return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
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
                if (hero.Name.ToLower().Contains("bot"))
                    continue;

                if (hero.IsEnemy)
                {
                    _enemies.Add(hero, new ChampionEloDisplayer());
                }
                else
                {
                    _allies.Add(hero, new ChampionEloDisplayer());
                }
                index++;
            }
            //foreach (var enemy in _enemies)
            //{
            //    UpdateStatus(enemy.Value.GetLolWebSiteContentOverview(enemy.Key));
            //}
            //foreach (var ally in _allies)
            //{
            //    UpdateStatus(ally.Value.GetLolWebSiteContentOverview(ally.Key));
            //}
            //for (int i = 0; i < 1; i++)
            //{
            //    _enemies.Add(ObjectManager.Player, new ChampionEloDisplayer());
            //    _allies.Add(ObjectManager.Player, new ChampionEloDisplayer());
            //}

            //foreach (var enemy in _enemies)
            //{
            //    var t = new Thread(new ParameterizedThreadStart(GenerateMasteryPage));
            //    t.SetApartmentState(ApartmentState.STA);
            //    t.Start(enemy);
            //}
            //foreach (var ally in _allies)
            //{
            //    var t = new Thread(new ParameterizedThreadStart(GenerateMasteryPage));
            //    t.SetApartmentState(ApartmentState.STA);
            //    t.Start(ally);
            //}

            Game.OnUpdate += Game_OnGameUpdateAsyncSprites;
            Game.OnUpdate += Game_OnGameUpdateAsyncTexts;
            Game.OnUpdate += Game_OnGameUpdate;
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
            EloDisplayerMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_ELODISPLAYER_MAIN"), "SAssembliesMiscsEloDisplayer"));
            EloDisplayerMisc.MenuItems.Add(
                EloDisplayerMisc.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAssembliesMiscsEloDisplayerKey", Language.GetString("GLOBAL_KEY")).SetValue(new KeyBind(9, KeyBindType.Toggle))));
            EloDisplayerMisc.MenuItems.Add(
                EloDisplayerMisc.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAssembliesMiscsEloDisplayerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
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

            Header.Position = new Vector2(_allies.First().Value.SummonerName.Position.X, _allies.First().Value.SummonerName.Position.Y - yOffset / 1.3f);
            if (_allies.Count != 0)
            {
                SummarizedAlly.Position = new Vector2(_allies.First().Value.SummonerName.Position.X, _allies.First().Value.SummonerName.Position.Y);
            }
            if (_enemies.Count != 0)
            {
                SummarizedEnemy.Position = new Vector2(_enemies.First().Value.SummonerName.Position.X, _enemies.First().Value.SummonerName.Position.Y);
            }

            foreach (var hero in heroes)
            {
                if (hero.Value.SummonerIcon != null && hero.Value.SummonerIcon.Sprite != null && hero.Value.SummonerIcon.Sprite.Sprite != null)
                {
                    hero.Value.SummonerIcon.Position = new Vector2(MainFrame.Sprite.X + 70, MainFrame.Sprite.Y + yOffset + (index * hero.Value.SummonerIcon.Sprite.Sprite.Height));
                    hero.Value.SummonerName.Position = new Vector2(hero.Value.SummonerIcon.Position.X + hero.Value.SummonerIcon.Sprite.Sprite.Width + 10, hero.Value.SummonerIcon.Position.Y);
                    hero.Value.ChampionName.Position = new Vector2(hero.Value.SummonerName.Position.X, hero.Value.SummonerName.Position.Y + textFontSize);
                    hero.Value.Divison.Position = new Vector2(hero.Value.SummonerName.Position.X + 150, hero.Value.SummonerName.Position.Y + textFontSize / 2);
                    hero.Value.RankedStatistics.Position = new Vector2(hero.Value.Divison.Position.X + 150, hero.Value.Divison.Position.Y - textFontSize / 2);
                    hero.Value.MMR.Position = new Vector2(hero.Value.RankedStatistics.Position.X, hero.Value.RankedStatistics.Position.Y + textFontSize);
                    hero.Value.RecentStatistics.Position = new Vector2(hero.Value.MMR.Position.X + 150, hero.Value.MMR.Position.Y - textFontSize);
                    hero.Value.ChampionGames.Position = new Vector2(hero.Value.RecentStatistics.Position.X, hero.Value.RecentStatistics.Position.Y + textFontSize);
                    hero.Value.OverallKDA.Position = new Vector2(hero.Value.ChampionGames.Position.X + 150, hero.Value.ChampionGames.Position.Y - textFontSize);
                    hero.Value.ChampionKDA.Position = new Vector2(hero.Value.OverallKDA.Position.X, hero.Value.OverallKDA.Position.Y + textFontSize);
                    hero.Value.Masteries.Position = new Vector2(hero.Value.ChampionKDA.Position.X + 150, hero.Value.ChampionKDA.Position.Y - textFontSize / 2);
                    hero.Value.Runes.Position = new Vector2(hero.Value.Masteries.Position.X + 150, hero.Value.Masteries.Position.Y);
                }
                index++;
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            CalculatePositions(true);
            CalculatePositions(false);
        }

        void Game_OnGameUpdateAsyncSprites(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateSpritesTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateSpritesTime = Environment.TickCount;

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
                    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
                        && champ.IsFinished();
                };
                champ.SummonerIcon.Sprite.Sprite.Add(2);
                champ.SummonerIcon.Sprite.LoadingFinished = true;
            }

            //if (!summonerIcon.Equals("") && (champ.MasteriesSprite == null || champ.MasteriesSprite.Sprite == null || !champ.MasteriesSprite.Sprite.DownloadFinished))
            //{
            //    SpriteHelper.LoadTexture(summonerIcon, ref champ.MasteriesSprite.Sprite, @"EloDisplayer\");
            //}
            //if (champ.MasteriesSprite != null && champ.MasteriesSprite.Sprite != null && champ.MasteriesSprite.Sprite.DownloadFinished && !champ.MasteriesSprite.Sprite.LoadingFinished)
            //{
            //    champ.MasteriesSprite.Sprite.Sprite.Scale = new Vector2(0.35f, 0.35f);
            //    champ.MasteriesSprite.Sprite.Sprite.PositionUpdate = delegate
            //    {
            //        return champ.MasteriesSprite.Position;
            //    };
            //    champ.MasteriesSprite.Sprite.Sprite.VisibleCondition = delegate
            //    {
            //        return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //    };
            //    champ.MasteriesSprite.Sprite.Sprite.Add(2);
            //    champ.MasteriesSprite.Sprite.LoadingFinished = true;
            //}
            
        }

        void Game_OnGameUpdateAsyncTexts(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTextsTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTextsTime = Environment.TickCount;

            if (Header.FinishedLoading != true)
            {
                String SummonerName = "SummonerName";
                String ChampionName = "ChampionName";
                String Divison = "Divison";
                String RankedStatistics = "RankedStatistics";
                String MMR = "MMR";
                String RecentStatistics = "RecentStatistics";
                String ChampionGames = "ChampionGames";
                String OverallKDA = "OverallKDA";
                String ChampionKDA = "ChampionKDA";
                String Masteries = "Masteries";
                String Runes = "Runes";

                List<String> line1 = CalcNeededWhitespaces(
                    new List<string>(new String[] { SummonerName, Divison, RankedStatistics, RecentStatistics, OverallKDA }));
                List<String> line2 = CalcNeededWhitespaces(
                    new List<string>(new String[] { ChampionName, "", MMR, ChampionGames, ChampionKDA }));

                String text = String.Format("{0}{1}{2}{3}{4}{5}\n{6}{7}{8}{9}{10}{11}",
                    line1[0], line1[1], line1[2], line1[3], line1[4], Masteries,
                    line2[0], line2[1], line2[2], line2[3], line2[4], Runes);
                Header.Text = new Render.Text(0, 0, text, 20, SharpDX.Color.Orange);
                Header.Text.PositionUpdate = delegate
                {
                    return Header.Position;
                };
                Header.Text.VisibleCondition = sender =>
                {
                    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
                };
                Header.Text.OutLined = true;
                Header.Text.Add(4);
                Header.FinishedLoading = true;
            }

            string sText = "";
            int index = 0;
            foreach (var ally in _allies)
            {
                Game_OnGameUpdateAsyncTextsSummary(ally, ref sText);
                index++;
            }
            SummarizedAlly.WebsiteContent = sText;
            if (SummarizedAlly.FinishedLoading != true)
            {
                SummarizedAlly.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
                SummarizedAlly.Text.TextUpdate = delegate
                {
                    return SummarizedAlly.WebsiteContent;
                };
                SummarizedAlly.Text.PositionUpdate = delegate
                {
                    return SummarizedAlly.Position;
                };
                SummarizedAlly.Text.VisibleCondition = sender =>
                {
                    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
                };
                SummarizedAlly.Text.OutLined = true;
                //SummarizedAlly.Text.Add(4);
                SummarizedAlly.FinishedLoading = true;
            }

            sText = "";
            index = 0;
            foreach (var enemy in _enemies)
            {
                Game_OnGameUpdateAsyncTextsSummary(enemy, ref sText);
                index++;
            }
            SummarizedEnemy.WebsiteContent = sText;
            if (SummarizedEnemy.FinishedLoading != true)
            {
                SummarizedEnemy.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
                SummarizedEnemy.Text.TextUpdate = delegate
                {
                    return SummarizedEnemy.WebsiteContent;
                };
                SummarizedEnemy.Text.PositionUpdate = delegate
                {
                    return SummarizedEnemy.Position;
                };
                SummarizedEnemy.Text.VisibleCondition = sender =>
                {
                    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active;
                };
                SummarizedEnemy.Text.OutLined = true;
                //SummarizedEnemy.Text.Add(4);
                SummarizedEnemy.FinishedLoading = true;
            }
        }

        private void Game_OnGameUpdateAsyncTextsSummary(KeyValuePair<Obj_AI_Hero, ChampionEloDisplayer> heroPair, ref String text)
        {
            Obj_AI_Hero hero = heroPair.Key;
            ChampionEloDisplayer champ = heroPair.Value;


            String text1 = hero.Name;
            String text2 = champ.Divison.WebsiteContent;
            String text3 = champ.RankedStatistics.WebsiteContent;
            String text4 = champ.RecentStatistics.WebsiteContent;
            String text5 = champ.OverallKDA.WebsiteContent;
            String text6 = GetMasteries(hero) + " | ";
            String text7 = hero.ChampionName;
            String text8 = "";
            String text9 = champ.MMR.WebsiteContent;
            String text10 = champ.ChampionGames.WebsiteContent;
            String text11 = champ.ChampionKDA.WebsiteContent;
            String text12 = "Click here!";
            List<String> line3 = CalcNeededWhitespaces(
                        new List<string>(new String[] { text1, text2, text3, text4, text5 }));
            List<String> line4 = CalcNeededWhitespaces(
                new List<string>(new String[] { text7, text8, text9, text10, text11 }));

            text = String.Format("{0}{1}{2}{3}{4}{5}\n{6}{7}{8}{9}{10}{11}\n",
                line3[0], line3[1], line3[2], line3[3], line3[4], text6,
                line4[0], line4[1], line4[2], line4[3], line4[4], text12);

            //if (champ.Divison.FinishedLoading != true)
            //{
            //    champ.Divison.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //    champ.Divison.Text.TextUpdate = delegate
            //    {
            //        if (!champ.Divison.WebsiteContent.Equals(""))
            //        {
            //            return champ.Divison.WebsiteContent;
            //        }
            //        return "";
            //    };
            //    champ.Divison.Text.PositionUpdate = delegate
            //    {
            //        return champ.Divison.Position;
            //    };
            //    champ.Divison.Text.VisibleCondition = sender =>
            //    {
            //        return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //    };
            //    champ.Divison.Text.OutLined = true;
            //    champ.Divison.Text.Add(4);
            //    champ.Divison.FinishedLoading = true;
            //}

            //if (champ.RankedStatistics.FinishedLoading != true)
            //{
            //    champ.RankedStatistics.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //    champ.RankedStatistics.Text.TextUpdate = delegate
            //    {
            //        if (!champ.RankedStatistics.WebsiteContent.Equals(""))
            //        {
            //            return champ.RankedStatistics.WebsiteContent;
            //        }
            //        return "";
            //    };
            //    champ.RankedStatistics.Text.PositionUpdate = delegate
            //    {
            //        return champ.RankedStatistics.Position;
            //    };
            //    champ.RankedStatistics.Text.VisibleCondition = sender =>
            //    {
            //        return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //    };
            //    champ.RankedStatistics.Text.OutLined = true;
            //    champ.RankedStatistics.Text.Add(4);
            //    champ.RankedStatistics.FinishedLoading = true;
            //}

            //if (champ.MMR.FinishedLoading != true)
            //{
            //    champ.MMR.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //    champ.MMR.Text.TextUpdate = delegate
            //    {
            //        if (!champ.MMR.WebsiteContent.Equals(""))
            //        {
            //            return champ.MMR.WebsiteContent;
            //        }
            //        return "";
            //    };
            //    champ.MMR.Text.PositionUpdate = delegate
            //    {
            //        return champ.MMR.Position;
            //    };
            //    champ.MMR.Text.VisibleCondition = sender =>
            //    {
            //        return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //    };
            //    champ.MMR.Text.OutLined = true;
            //    champ.MMR.Text.Add(4);
            //    champ.MMR.FinishedLoading = true;
            //}

            //if (champ.RecentStatistics.FinishedLoading != true)
            //{
            //    champ.RecentStatistics.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //    champ.RecentStatistics.Text.TextUpdate = delegate
            //    {
            //        if (!champ.RecentStatistics.WebsiteContent.Equals(""))
            //        {
            //            return champ.RecentStatistics.WebsiteContent;
            //        }
            //        return "";
            //    };
            //    champ.RecentStatistics.Text.PositionUpdate = delegate
            //    {
            //        return champ.RecentStatistics.Position;
            //    };
            //    champ.RecentStatistics.Text.VisibleCondition = sender =>
            //    {
            //        return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //    };
            //    champ.RecentStatistics.Text.OutLined = true;
            //    champ.RecentStatistics.Text.Add(4);
            //    champ.RecentStatistics.FinishedLoading = true;
            //}

            //if (champ.ChampionGames.FinishedLoading != true)
            //{
            //    champ.ChampionGames.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //    champ.ChampionGames.Text.TextUpdate = delegate
            //    {
            //        if (!champ.ChampionGames.WebsiteContent.Equals(""))
            //        {
            //            return champ.ChampionGames.WebsiteContent;
            //        }
            //        return "";
            //    };
            //    champ.ChampionGames.Text.PositionUpdate = delegate
            //    {
            //        return champ.ChampionGames.Position;
            //    };
            //    champ.ChampionGames.Text.VisibleCondition = sender =>
            //    {
            //        return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //    };
            //    champ.ChampionGames.Text.OutLined = true;
            //    champ.ChampionGames.Text.Add(4);
            //    champ.ChampionGames.FinishedLoading = true;
            //}

            //if (champ.OverallKDA.FinishedLoading != true)
            //{
            //    champ.OverallKDA.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //    champ.OverallKDA.Text.TextUpdate = delegate
            //    {
            //        if (!champ.OverallKDA.WebsiteContent.Equals(""))
            //        {
            //            return champ.OverallKDA.WebsiteContent;
            //        }
            //        return "";
            //    };
            //    champ.OverallKDA.Text.PositionUpdate = delegate
            //    {
            //        return champ.OverallKDA.Position;
            //    };
            //    champ.OverallKDA.Text.VisibleCondition = sender =>
            //    {
            //        return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //    };
            //    champ.OverallKDA.Text.OutLined = true;
            //    champ.OverallKDA.Text.Add(4);
            //    champ.OverallKDA.FinishedLoading = true;
            //}

            //if (champ.ChampionKDA.FinishedLoading != true)
            //{
            //    champ.ChampionKDA.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //    champ.ChampionKDA.Text.TextUpdate = delegate
            //    {
            //        if (!champ.ChampionKDA.WebsiteContent.Equals(""))
            //        {
            //            return champ.ChampionKDA.WebsiteContent;
            //        }
            //        return "";
            //    };
            //    champ.ChampionKDA.Text.PositionUpdate = delegate
            //    {
            //        return champ.ChampionKDA.Position;
            //    };
            //    champ.ChampionKDA.Text.VisibleCondition = sender =>
            //    {
            //        return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //    };
            //    champ.ChampionKDA.Text.OutLined = true;
            //    champ.ChampionKDA.Text.Add(4);
            //    champ.ChampionKDA.FinishedLoading = true;
            //}

            
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
            //champ.SummonerName.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //champ.SummonerName.Text.TextUpdate = delegate
            //{
            //    return hero.Name;
            //};
            //champ.SummonerName.Text.PositionUpdate = delegate
            //{
            //    return champ.SummonerName.Position;
            //};
            //champ.SummonerName.Text.VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //};
            //champ.SummonerName.Text.OutLined = true;
            //champ.SummonerName.Text.Add(4);

            //champ.ChampionName.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //champ.ChampionName.Text.TextUpdate = delegate
            //{
            //    return hero.ChampionName;
            //};
            //champ.ChampionName.Text.PositionUpdate = delegate
            //{
            //    return champ.ChampionName.Position;
            //};
            //champ.ChampionName.Text.VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //};
            //champ.ChampionName.Text.OutLined = true;
            //champ.ChampionName.Text.Add(4);

            //champ.Masteries.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //champ.Masteries.Text.TextUpdate = delegate
            //{
            //    return GetMasteries(hero);
            //};
            //champ.Masteries.Text.PositionUpdate = delegate
            //{
            //    return champ.Masteries.Position;
            //};
            //champ.Masteries.Text.VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //};
            //champ.Masteries.Text.OutLined = true;
            //champ.Masteries.Text.Add(4);

            //champ.Runes.Text = new Render.Text(0, 0, "", 20, SharpDX.Color.Orange);
            //champ.Runes.Text.TextUpdate = delegate
            //{
            //    return "Click here!";
            //};
            //champ.Runes.Text.PositionUpdate = delegate
            //{
            //    return champ.Runes.Position;
            //};
            //champ.Runes.Text.VisibleCondition = sender =>
            //{
            //    return Misc.Miscs.GetActive() && EloDisplayerMisc.GetActive() && EloDisplayerMisc.GetMenuItem("SAssembliesMiscsEloDisplayerKey").GetValue<KeyBind>().Active
            //            && champ.IsFinished();
            //};
            //champ.Runes.Text.OutLined = true;
            //champ.Runes.Text.Add(4);
        }

        private void LoadSpritesAsync()
        {
            foreach (var ally in _allies)
            {
                ally.Value.SummonerIcon.WebsiteContent = GetSummonerIcon(ally.Key, ally.Value);
                //SpriteHelper.DownloadImageOpGg(ally.Value.SummonerIcon.WebsiteContent, @"EloDisplayer\");
                SpriteHelper.DownloadImageRiot(ally.Value.SummonerIcon.WebsiteContent, SpriteHelper.ChampionType.None, SpriteHelper.DownloadType.ProfileIcon, @"EloDisplayer\");
            }
            foreach (var enemy in _enemies)
            {
                enemy.Value.SummonerIcon.WebsiteContent = GetSummonerIcon(enemy.Key, enemy.Value);
                //SpriteHelper.DownloadImageOpGg(enemy.Value.SummonerIcon.WebsiteContent, @"EloDisplayer\");
                SpriteHelper.DownloadImageRiot(enemy.Value.SummonerIcon.WebsiteContent, SpriteHelper.ChampionType.None, SpriteHelper.DownloadType.ProfileIcon, @"EloDisplayer\");
            }
        }

        private void LoadTextsAsync()
        {
            foreach (var ally in _allies)
            {
                ally.Value.Divison.WebsiteContent = GetDivision(ally.Key, ally.Value, ref ally.Value.Ranked);
                ally.Value.RankedStatistics.WebsiteContent = GetRankedStatistics(ally.Key, ally.Value, ally.Value.Ranked);
                ally.Value.MMR.WebsiteContent = GetMmr(ally.Key, ally.Value.Ranked);
                ally.Value.RecentStatistics.WebsiteContent = GetRecentStatistics(ally.Key, ally.Value);
                ally.Value.ChampionGames.WebsiteContent = GetChampionGamesLastSeason(ally.Key, ally.Value, ally.Value.Ranked);
                if (ally.Value.ChampionGames.WebsiteContent.Equals("0/0"))
                {
                    ally.Value.ChampionGames.WebsiteContent = GetChampionGamesNormal(ally.Key, ally.Value);
                }
                ally.Value.OverallKDA.WebsiteContent = GetOverallKDA(ally.Key, ally.Value);
                ally.Value.ChampionKDA.WebsiteContent = GetChampionKDALastSeason(ally.Key, ally.Value, ally.Value.Ranked);
                if (ally.Value.ChampionKDA.WebsiteContent.Equals("0/0/0"))
                {
                    ally.Value.ChampionKDA.WebsiteContent = GetChampionKDANormal(ally.Key, ally.Value);
                }
            }
            foreach (var enemy in _enemies)
            {
                enemy.Value.Divison.WebsiteContent = GetDivision(enemy.Key, enemy.Value, ref enemy.Value.Ranked);
                enemy.Value.RankedStatistics.WebsiteContent = GetRankedStatistics(enemy.Key, enemy.Value, enemy.Value.Ranked);
                enemy.Value.MMR.WebsiteContent = GetMmr(enemy.Key, enemy.Value.Ranked);
                enemy.Value.RecentStatistics.WebsiteContent = GetRecentStatistics(enemy.Key, enemy.Value);
                enemy.Value.ChampionGames.WebsiteContent = GetChampionGamesLastSeason(enemy.Key, enemy.Value, enemy.Value.Ranked);
                if (enemy.Value.ChampionGames.WebsiteContent.Equals("0/0"))
                {
                    enemy.Value.ChampionGames.WebsiteContent = GetChampionGamesNormal(enemy.Key, enemy.Value);
                }
                enemy.Value.OverallKDA.WebsiteContent = GetOverallKDA(enemy.Key, enemy.Value);
                enemy.Value.ChampionKDA.WebsiteContent = GetChampionKDALastSeason(enemy.Key, enemy.Value, enemy.Value.Ranked);
                if (enemy.Value.ChampionKDA.WebsiteContent.Equals("0/0/0"))
                {
                    enemy.Value.ChampionKDA.WebsiteContent = GetChampionKDANormal(enemy.Key, enemy.Value);
                }
            }
        }

        public static String GetLolWebSiteContent(String webSite)
        {
            return GetLolWebSiteContent(webSite, null);
        }

        public static String GetLolWebSiteContent(String webSite, String param)
        {
            return GetWebSiteContent(GetWebSite() + webSite, param);
        }

        public static String GetWebSiteContent(String webSite, String param = null)
        {
            string website = "";
            var request = (HttpWebRequest)WebRequest.Create(webSite);
            TryAddCookie(request, new Cookie("customLocale", "en_US", "", GetWebSiteWithoutHttp()));
            if (param != null)
            {
                Byte[] bytes = Encoding.ASCII.GetBytes(param);//GetBytes(param);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                //Stream dataStream = request.GetRequestStream();
                //dataStream.Write(bytes, 0, bytes.Length);
                //dataStream.Close();
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
                                    website = @readStream.ReadToEnd();
                                }
                            }
                            else
                            {
                                using (
                                    StreamReader readStream = new StreamReader(receiveStream,
                                        Encoding.GetEncoding(response.CharacterSet)))
                                {
                                    website = @readStream.ReadToEnd();
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

        public static String GetLolWebSiteContentOverview(Obj_AI_Hero hero)
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

        public static String GetEncodedPlayerName(Obj_AI_Hero hero)
        {
            return HttpUtility.UrlEncode(hero.Name);
        }

        public static String GetWebSite()
        {
            String prefix = GetRegionPrefix();
            if (prefix == "")
                return "http://op.gg/";
            else
                return "http://" + prefix + ".op.gg/";
        }

        public static String GetWebSiteWithoutHttp()
        {
            String prefix = GetRegionPrefix();
            if (prefix == "")
                return "op.gg";
            else
                return prefix + ".op.gg";
        }

        public static String GetRegionPrefix()
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

        private String GetSummonerIcon(Obj_AI_Hero hero, ChampionEloDisplayer elo)
        {
            String websiteContent = elo.GetLolWebSiteContentOverview(hero);
            String patternWin = "<div class=\"rectImage\"><img src=\"//(.*?)op.gg/images/profile_icons/profileIcon(.*?)\\.jpg\"></div>";
            return GetMatch(websiteContent, patternWin, 0, 2) + ".png";
        }

        private String GetDivision(Obj_AI_Hero hero, ChampionEloDisplayer elo, ref bool ranked) //Bugged
        {
            String division = "";
            String websiteContent = elo.GetLolWebSiteContentOverview(hero);
            String patternTierRank = "<div class=\"TierRank\">";
            String patternLeaguePoints = "<div class=\"LeaguePoints\">";
            if (!GetMatch(websiteContent, patternTierRank).Equals("") && !GetMatch(websiteContent, patternLeaguePoints).Equals(""))
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
                ranked = true;
            }
            else if (!GetMatch(websiteContent, patternTierRank).Equals("") && GetMatch(websiteContent, patternLeaguePoints).Equals(""))
            {
                division = "Unranked";
            }
            else if (GetMatch(websiteContent, patternTierRank).Equals(""))
            {
                division = "Unranked (<30)";
            }
            return division;
        }

        private String GetRankedStatistics(Obj_AI_Hero hero, ChampionEloDisplayer elo, bool ranked)
        {
            if (!ranked)
                return "0/0";

            String websiteContent = elo.GetLolWebSiteContentOverview(hero);
            String patternWin = @"<br><span class=""wins"">(.*?)W</span>";
            String patternLoose = @"</span><span class=""losses"">(.*?)L</span>";
            return GetMatch(websiteContent, patternWin) + "W/" +
                  GetMatch(websiteContent, patternLoose) + "L";
        }

        private String GetRecentStatistics(Obj_AI_Hero hero, ChampionEloDisplayer elo)
        {
            String websiteContent = elo.GetLolWebSiteContentOverview(hero);
            String patternWl = @"<div class=""WinRatioTitle"">(.*?)</div>";
            String matchWl = GetMatch(websiteContent, patternWl);
            String patternWin = @"(\d*?)W";
            String patternLoose = @"(\d*?)L";
            return GetMatch(matchWl, patternWin, 0, 0) + "/" +
                  GetMatch(matchWl, patternLoose, 0, 0);
        }

        private String GetOverallKDA(Obj_AI_Hero hero, ChampionEloDisplayer elo)
        {
            String websiteContent = elo.GetLolWebSiteContentOverview(hero);
            String patternKill = "<div class=\"KDA\"><div class=\"kda\"><span class=\"kill\">(.*?)</span>";
            String patternDeath = "<div class=\"KDA\"><div class=\"kda\">(.*?)<span class=\"death\">(.*?)</span>";
            String patternAssist = "<div class=\"KDA\"><div class=\"kda\">(.*?)<span class=\"assist\">(.*?)</span>";
            return GetMatch(websiteContent, patternKill) + "/" +
                  GetMatch(websiteContent, patternDeath, 0, 2) + "/" +
                  GetMatch(websiteContent, patternAssist, 0, 2);
        }

        private String GetMmr(Obj_AI_Hero hero, bool ranked)
        {
            if (!ranked)
                return "0";

            String websiteContent = GetLolWebSiteContent("summoner/ajax/mmr.json/", "userName=" + GetEncodedPlayerName(hero));
            String patternMmr = "\"mmr\":\"(.*?)\"";
            String patternAverageMmr = "<b>(.*?)<\\\\/b>";
            return GetMatch(websiteContent, patternMmr) + "/" +
                   GetMatch(websiteContent, patternAverageMmr);
        }

        private String GetMasteries(Obj_AI_Hero hero)
        {
            int offense = 0;
            int defense = 0;
            int utility = 0;
            for (int i = 0; i < hero.Masteries.Count(); i++)
            {
                if (hero.Masteries[i] != null)
                {
                    var mastery = hero.Masteries[i];
                    if (mastery.Page == MasteryPage.Offense)
                    {
                        offense += mastery.Points;
                    }
                    else if (mastery.Page == MasteryPage.Defense)
                    {
                        defense += mastery.Points;
                    }
                    else
                    {
                        utility += mastery.Points;
                    }
                }
            }
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

        private String GetChampionKDA(Obj_AI_Hero hero, ChampionEloDisplayer elo, String season)
        {
            String championContent = elo.GetLolWebSiteContentChampion(hero, GetSummonerId(elo.GetLolWebSiteContentOverview(hero)), season);
            String patternChampion = "<div class=\"championName\">(.*?)<\\\\/div>";
            String patternKill = "<span class=\"kill\">(.*?)<\\\\/span>";
            String patternDeath = "<span class=\"death\">(.*?)<\\\\/span>";
            String patternAssist = "<span class=\"assist\">(.*?)<\\\\/span>";
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
            if (matchKill.Equals("") && matchDeath.Equals("") && matchAssist.Equals(""))
                return "0/0/0";
            return matchKill + "/" + matchDeath + "/" + matchAssist;
        }

        private String GetChampionKDALastSeason(Obj_AI_Hero hero, ChampionEloDisplayer elo, bool ranked)
        {
            if (!ranked)
                return "0/0/0";

            return GetChampionKDA(hero, elo, "4");
        }

        private String GetChampionKDANormal(Obj_AI_Hero hero, ChampionEloDisplayer elo)
        {
            return GetChampionKDA(hero, elo, "normal");
        }

        private String GetChampionGames(Obj_AI_Hero hero, ChampionEloDisplayer elo, String season)
        {
            String championContent = elo.GetLolWebSiteContentChampion(hero, GetSummonerId(elo.GetLolWebSiteContentOverview(hero)), season);
            String patternChampion = "<div class=\"championName\">(.*?)<\\\\/div>";
            String patternWins = "<span class=\"wins\">(.*?)<\\\\/span>";
            String patternLosses = "<span class=\"losses\">(.*?)<\\\\/span>";
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
            if (matchWins.Equals("") && matchLosses.Equals(""))
                return "0/0";
            return matchWins + "/" + matchLosses;
        }

        private String GetChampionGamesLastSeason(Obj_AI_Hero hero, ChampionEloDisplayer elo, bool ranked)
        {
            if (!ranked)
                return "0/0";

            return GetChampionGames(hero, elo,"4");
        }

        private String GetChampionGamesNormal(Obj_AI_Hero hero, ChampionEloDisplayer elo)
        {
            return GetChampionGames(hero, elo, "normal");
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
            try
            {
                string replacement = Regex.Replace(websiteContent, @"\t|\n|\r", "");
                replacement = Regex.Replace(replacement, @"\\t|\\n|\\r", "");
                replacement = Regex.Replace(replacement, @"\\""", "\"");
                Match websiteMatcher = new Regex(pattern).Matches(replacement)[index];
                //Match elementMatch = new Regex(websiteMatcher.Groups[groupIndex].ToString()).Matches(replacement)[0];
                //return elementMatch.ToString();
                return websiteMatcher.Groups[groupIndex].ToString();
            }
            catch (Exception e)
            {
                //Console.WriteLine("Cannot get value for {0}, pattern {1}, index {2}, groupIndex {3}\n Exception: ", @websiteContent, @pattern, index, groupIndex, e);
            }
            return "";
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
            Byte[] bytes = Encoding.ASCII.GetBytes(json);
            webRequest.ContentLength = bytes.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            String content = "";
            using (var response = (HttpWebResponse)webRequest.GetResponse())
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
                                content = @readStream.ReadToEnd();
                            }
                        }
                        else
                        {
                            using (
                                StreamReader readStream = new StreamReader(receiveStream,
                                    Encoding.GetEncoding(response.CharacterSet)))
                            {
                                content = @readStream.ReadToEnd();
                            }
                        }
                    }
                }
            }
            try
            {
                return FromJson<T>(content);
            }
            catch (Exception)
            {
                return (T) new Object();
            }
        }

        private void GenerateMasteryPage(object hero)
        {
            String masteryPage = "http://leaguecraft.com/masteries/iframe/?points=";
            foreach (var mastery in ((KeyValuePair<Obj_AI_Hero, ChampionEloDisplayer>)hero).Key.Masteries)
            {
                masteryPage += mastery.Points;
            }
            ((KeyValuePair<Obj_AI_Hero, ChampionEloDisplayer>)hero).Value.MasteriesSprite = new TextInfo();
            SpriteHelper.LoadTexture(CreateScreenShot(masteryPage), ref ((KeyValuePair<Obj_AI_Hero, ChampionEloDisplayer>)hero).Value.MasteriesSprite.Sprite);
            return;
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

        public static bool TryAddCookie(WebRequest webRequest, Cookie cookie)
        {
            HttpWebRequest httpRequest = webRequest as HttpWebRequest;
            if (httpRequest == null)
            {
                return false;
            }

            if (httpRequest.CookieContainer == null)
            {
                httpRequest.CookieContainer = new CookieContainer();
            }

            httpRequest.CookieContainer.Add(cookie);
            return true;
        }

        public static List<String> CalcNeededWhitespaces(List<String> strings)
        {
            Font font = new Font(
                    Drawing.Direct3DDevice,
                    new FontDescription
                    {
                        FaceName = "Calibri",
                        Height = 20,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default
                    });
            Drawing.OnPreReset += args => font.OnLostDevice();
            Drawing.OnPostReset += args => font.OnResetDevice();
            List<String> ws = new List<String>();
            foreach (var s in strings)
            {
                if(s == null)
                    continue;
                Rectangle rec = font.MeasureText(null, s, FontDrawFlags.Center);
                StringBuilder sb = new StringBuilder(s);
                sb = sb.Append("!");
                while (rec.Width < 150)
                {
                    sb = sb.Insert(s.Length, " ");
                    rec = font.MeasureText(null, sb.ToString(), FontDrawFlags.Center);
                }
                sb.Replace("!", "");
                ws.Add(sb.ToString());
            }
            font.Dispose();
            return ws;
        }

        class ChampionEloDisplayer
        {
            public bool Ranked = false;
            public TextInfo Summarized = new TextInfo();
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

            public TextInfo MasteriesSprite = new TextInfo();
            public TextInfo RunesSprite = new TextInfo();
            public TextInfo RunesSpriteText = new TextInfo();

            private String websiteContentOverview = "";
            private String websiteContentChampion = "";
            private String _currentSeason = "";

            public String GetLolWebSiteContentOverview(Obj_AI_Hero hero)
            {
                if (websiteContentOverview == "")
                {
                    websiteContentOverview = EloDisplayer.GetLolWebSiteContentOverview(hero);
                }
                return websiteContentOverview;
            }

            public String GetLolWebSiteContentChampion(Obj_AI_Hero hero, String summonerId, String season)
            {
                if (websiteContentChampion == "" || _currentSeason == "" || !_currentSeason.Equals(season))
                {
                    _currentSeason = season;
                    websiteContentChampion = EloDisplayer.GetLolWebSiteContent("summoner/champions/ajax/champions.json/", "summonerId=" + summonerId + "&season=" + season + "&type=stats");
                }
                return websiteContentChampion;
            }

            public bool IsFinished()
            {
                return true;
            }
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

        internal class TextInfo
        {
            public bool FinishedLoading = false;
            public String WebsiteContent = "";

            public SpriteHelper.SpriteInfo Sprite;
            public Render.Text Text;
            public Vector2 Position;
        }
    }
}
