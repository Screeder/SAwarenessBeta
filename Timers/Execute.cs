using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Timers
{
    class Execute
    {
        public static Menu.MenuItemSettings ExecuteTimer = new Menu.MenuItemSettings(typeof(Execute));

        Dictionary<Obj_AI_Hero, int> lastDmg = new Dictionary<Obj_AI_Hero, int>(); 

        public Execute() //TODO: Wait for Dmg Event or working Dmg Packet
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                lastDmg.Add(hero, 0);
            }
            Game.OnUpdate += Game_OnGameUpdate;
            AttackableUnit.OnDamage += AttackableUnit_OnDamage;
        }

        void AttackableUnit_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            throw new NotImplementedException();
        }

        ~Execute()
        {
            Game.OnUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
#if TIMERS
            return Timer.Timers.GetActive() && ExecuteTimer.GetActive();
#else
            return ExecuteTimer.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            ExecuteTimer.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TIMERS_EXECUTE_MAIN"), "SAssembliesTimersExecute"));
            ExecuteTimer.MenuItems.Add(ExecuteTimer.CreateActiveMenuItem("SAssembliesTimersExecuteActive", () => new Execute()));
            return ExecuteTimer;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
