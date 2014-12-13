using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness
{
    class EloDisplayer
    {
        private SpriteHelper.SpriteInfo MainFrame;
        private Render.Text[] ChampionName = new Render.Text[10];
        private Render.Text[] SummonerName = new Render.Text[10];
        private Render.Text[] Divison = new Render.Text[10];
        private Render.Text[] RankedStatistics = new Render.Text[10];
        private Render.Text[] RecentStatistics = new Render.Text[10];
        private Render.Text[] MMR = new Render.Text[10];
        private Render.Text[] Masteries = new Render.Text[10]; //http://leaguecraft.com/masteries/iframe/?points=140003001103130003010202031010000000000000000000000000000
        private Render.Text[] Runes = new Render.Text[10]; //http://leaguecraft.com/runes/?marks=1,8,14,14,28,29,29,89,89&seals=16,16,16,16,16,16,16,16,295&glyphs=12,12,75,75,75,75,75,75,75&quints=296,293,288
        private Render.Text[] OverallKDA = new Render.Text[10];
        private Render.Text[] ChampionKDA = new Render.Text[10];
        private Render.Text[] ChampionGames = new Render.Text[10];
        private Render.Text[] ChampionWinRate = new Render.Text[10];

        private Render.Text[] TeamBans = new Render.Text[2];
        private Render.Text[] TeamDivison = new Render.Text[2];
        private Render.Text[] TeamRankedStatistics = new Render.Text[2];
        private Render.Text[] TeamRecentStatistics = new Render.Text[2];
        private Render.Text[] TeamMMR = new Render.Text[2];
        private Render.Text[] TeamChampionGames = new Render.Text[10];


        public EloDisplayer()
        {
            MainFrame = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("EloGui", ref MainFrame, SpriteHelper.TextureType.Default);
            MainFrame.Sprite.PositionUpdate = delegate
            {
                return new Vector2(Drawing.Width / 2 - MainFrame.Bitmap.Width / 2, Drawing.Height / 2 - MainFrame.Bitmap.Height / 2);
            };
            MainFrame.Sprite.VisibleCondition = delegate
            {
                return Menu.Misc.GetActive() && Menu.AutoLevler.GetActive();
            };
            MainFrame.Sprite.Add();

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                GetSummonerInformations(hero);
            }
        }

        ~EloDisplayer()
        {
        }

        public bool IsActive()
        {
            return Menu.Misc.GetActive() && Menu.EloDisplayer.GetActive();
        }

        private void GetSummonerInformations(Obj_AI_Hero hero)
        {
            
        }

        private void GetDivision()
        {
            
        }

        private void GetRankedStatistics()
        {
            
        }

        private void GetRecentStatistics()
        {
            
        }

        private void GetMMR()
        {
            
        }

        private void GetMasteries()
        {
            
        }

        private void GetRunes()
        {
            
        }

        private void GetOverallKDA()
        {
            
        }

        private void GetChampionKDA()
        {
            
        }

        private void GetChampionGames()
        {
            
        }

        private void GetChampionWinRate()
        {
            
        }

        private void CalculateTeamStats()
        {
            
        }
    }
}
