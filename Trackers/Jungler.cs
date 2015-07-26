using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAssemblies.Trackers
{
    class Jungler
    {
        public static Menu.MenuItemSettings JunglerTracker = new Menu.MenuItemSettings(typeof(Jungler));

        private Obj_AI_Hero HeroJungler = null;
        private bool targeting = false;

        public Jungler()
        {
            GameUpdate a = null;
            a = delegate(EventArgs args)
            {
                Init();
                Game.OnUpdate -= a;
            };
            Game.OnUpdate += a;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
        }

        ~Jungler()
        {

        }

        public bool IsActive()
        {
#if TRACKERS
            return Tracker.Trackers.GetActive() && JunglerTracker.GetActive();
#else
            return JunglerTracker.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            JunglerTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_JUNGLER_MAIN"), "SAssembliesTrackersJungler"));
            JunglerTracker.MenuItems.Add(JunglerTracker.CreateActiveMenuItem("SAssembliesTrackersJunglerActive", () => new Jungler()));
            return JunglerTracker;
        }

        private void Init()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy && hero.Spellbook.Spells.Find(inst => inst.Name.ToLower().Contains("smite")) != null)
                {
                    HeroJungler = hero;
                    Render.Text text = new Render.Text(Drawing.Width / 2, Drawing.Height / 2 + 400, "", 20, Color.AliceBlue);
                    text.TextUpdate = delegate
                    {
                        if (targeting)
                        {
                            return MapPositions.GetRegion(hero.ServerPosition.To2D()).ToString() +
                                   "\nJungler is targeting you. CARE!";
                        }
                        return MapPositions.GetRegion(hero.ServerPosition.To2D()).ToString();
                    };
                    text.VisibleCondition = sender =>
                    {
                        return IsActive() && hero.IsVisible && !hero.IsDead;
                    };
                    text.OutLined = true;
                    text.Centered = true;
                    text.Add();
                }
            }
        }

        void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args) //Will work when Jodus implemented it
        {
            if (!IsActive() || HeroJungler == null)
                return;

            if (sender.NetworkId == HeroJungler.NetworkId)
            {
                if (args.Target.NetworkId == ObjectManager.Player.NetworkId)
                {
                    targeting = true;
                }
                else
                {
                    targeting = false;
                }
            }
        }
    }
}
