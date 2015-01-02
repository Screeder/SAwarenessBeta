using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Timers
{
    class Immune //TODO: Maybe add Packetcheck
    {
        public static Menu.MenuItemSettings ImmuneTimer = new Menu.MenuItemSettings(typeof(Immune));

        private static Dictionary<Ability, Render.Text> Abilities = new Dictionary<Ability, Render.Text>();

        public Immune()
        {
            Abilities.Add(new Ability("zhonyas_ring_activate", 2.5f), null); //Zhonya
            Abilities.Add(new Ability("Aatrox_Passive_Death_Activate", 3f), null); //Aatrox Passive
            Abilities.Add(new Ability("LifeAura", 4f), null); //Zil und GA
            Abilities.Add(new Ability("nickoftime_tar", 7f), null); //Zil before death
            Abilities.Add(new Ability("eyeforaneye", 2f), null); // Kayle
            Abilities.Add(new Ability("UndyingRage_buf", 5f), null); //Tryn
            Abilities.Add(new Ability("EggTimer", 6f), null); //Anivia

            foreach (var ability in Abilities.ToList())
            {
                Render.Text text = new Render.Text(new Vector2(0, 0), "", 28, SharpDX.Color.Goldenrod);
                text.OutLined = true;
                text.Centered = true;
                text.TextUpdate = delegate
                {
                    float endTime = ability.Key.TimeCasted - (int)Game.ClockTime + ability.Key.Delay;
                    var m = (float)Math.Floor(endTime / 60);
                    var s = (float)Math.Ceiling(endTime % 60);
                    return (s < 10 ? m + ":0" + s : m + ":" + s);
                };
                text.PositionUpdate = delegate
                {
                    Vector2 hpPos = new Vector2();
                    if (ability.Key.Target != null)
                    {
                        hpPos = ability.Key.Target.HPBarPosition;
                    }
                    if (ability.Key.Owner != null)
                    {
                        hpPos = ability.Key.Owner.HPBarPosition;
                    }
                    hpPos.X = hpPos.X + 80;
                    return hpPos;
                };
                text.VisibleCondition = delegate
                {
                    return Timer.Timers.GetActive() && ImmuneTimer.GetActive() &&
                            ability.Key.Casted && ability.Key.TimeCasted > 0;
                };
                text.Add();
                Abilities[ability.Key] = text;
            }

            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~Immune()
        {
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Abilities = null;
        }

        public bool IsActive()
        {
            return Timer.Timers.GetActive() && ImmuneTimer.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            ImmuneTimer.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TIMERS_IMMUNE_MAIN"), "SAwarenessTimersImmune"));
            ImmuneTimer.MenuItems.Add(
                ImmuneTimer.Menu.AddItem(new MenuItem("SAwarenessTimersImmuneActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return ImmuneTimer;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var ability in Abilities)
            {
                if ((ability.Key.TimeCasted + ability.Key.Delay) < Game.ClockTime)
                {
                    ability.Key.Casted = false;
                    ability.Key.TimeCasted = 0;
                }
            }
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsEnemy)
                {
                    foreach (var ability in Abilities)
                    {
                        if (sender.Name.Contains(ability.Key.SpellName) &&
                            /*variable*/ Vector3.Distance(sender.Position, ObjectManager.Player.ServerPosition) <= 4000)
                        {
                            ability.Key.Owner = hero;
                            ability.Key.Casted = true;
                            ability.Key.TimeCasted = (int)Game.ClockTime;
                            if (Vector3.Distance(sender.Position, hero.ServerPosition) <= 100)
                                ability.Key.Target = hero;
                        }
                    }
                }
            }
        }

        public class Ability
        {
            public bool Casted;
            public float Delay;
            public Obj_AI_Hero Owner;
            public int Range;
            public String SpellName;
            public Obj_AI_Hero Target;
            public int TimeCasted;

            public Ability(string spellName, float delay)
            {
                SpellName = spellName;
                Delay = delay;
            }
        }
    }
}
