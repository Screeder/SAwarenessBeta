using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SAwareness.Trackers
{
    class Destination
    {
        public static Menu.MenuItemSettings DestinationTracker = new Menu.MenuItemSettings(typeof(Destination));

        private static Dictionary<Obj_AI_Hero, List<Ability>> Enemies =
            new Dictionary<Obj_AI_Hero, List<Ability>>();

        private int lastGameUpdateTime = 0;

        public Destination()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    var abilities = new List<Ability>();
                    foreach (SpellDataInst spell in hero.Spellbook.Spells)
                    {
                        if (spell.Name.ToLower().Contains("flash"))
                        {
                            abilities.Add(new Ability("SummonerFlash", 400, 0, hero));
                            //AddObject(hero, abilities);
                        }
                    }

                    //abilities.Clear(); //TODO: Check if it delets the flash abilities

                    switch (hero.ChampionName)
                    {
                        case "Ezreal":
                            abilities.Add(new Ability("EzrealArcaneShift", 475, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Fiora":
                            abilities.Add(new Ability("FioraDance", 700, 1, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Kassadin":
                            abilities.Add(new Ability("RiftWalk", 700, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Katarina":
                            abilities.Add(new Ability("KatarinaE", 700, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Leblanc":
                            abilities.Add(new Ability("LeblancSlide", 600, 0.5f, hero));
                            abilities.Add(new Ability("leblancslidereturn", 0, 0, hero));
                            abilities.Add(new Ability("LeblancSlideM", 600, 0.5f, hero));
                            abilities.Add(new Ability("leblancslidereturnm", 0, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Lissandra":
                            abilities.Add(new Ability("LissandraE", 700, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "MasterYi":
                            abilities.Add(new Ability("AlphaStrike", 600, 0.9f, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Shaco":
                            abilities.Add(new Ability("Deceive", 400, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Talon":
                            abilities.Add(new Ability("TalonCutthroat", 700, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Vayne":
                            abilities.Add(new Ability("VayneTumble", 250, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Zed":
                            abilities.Add(new Ability("ZedShadowDash", 999, 0, hero));
                            //AddObject(hero, abilities);
                            break;
                    }
                    if (abilities.Count > 0)
                        AddObject(hero, abilities);
                }
            }
            //Game.OnGameUpdate += Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Destination()
        {
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Hero_OnProcessSpellCast;
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Drawing.OnDraw -= Drawing_OnDraw;
            Enemies = null;
        }

        public bool IsActive()
        {
            return Tracker.Trackers.GetActive() && DestinationTracker.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            DestinationTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_DESTINATION_MAIN"), "SAwarenessTrackersDestination"));
            DestinationTracker.MenuItems.Add(
                DestinationTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersDestinationActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return DestinationTracker;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in Enemies)
            {
                foreach (Ability ability in enemy.Value)
                {
                    if (ability.Casted)
                    {
                        Vector2 startPos = Drawing.WorldToScreen(ability.StartPos);
                        Vector2 endPos = Drawing.WorldToScreen(ability.EndPos);

                        if (ability.EndPos.IsOnScreen())
                        {
                            if (ability.OutOfBush)
                            {
                                Utility.DrawCircle(ability.EndPos, ability.Range, System.Drawing.Color.Red);
                            }
                            else
                            {
                                Utility.DrawCircle(ability.EndPos, ability.Range, System.Drawing.Color.Red);
                                Drawing.DrawLine(startPos[0], startPos[1], endPos[0], endPos[1], 1.0f, System.Drawing.Color.Red);
                            }
                        }
                        if(endPos.IsOnScreen())
                        {
                            Drawing.DrawText(endPos[0], endPos[1], Color.Bisque,
                            enemy.Key.ChampionName + " " + ability.SpellName);
                        }
                    }
                }
            }
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in Enemies)
            {
                if (enemy.Key.ChampionName == "Shaco")
                {
                    if (sender.Type != GameObjectType.obj_LampBulb && sender.Name == "JackintheboxPoof2.troy" && !enemy.Value[0].Casted)
                    {
                        enemy.Value[0].StartPos = sender.Position;
                        enemy.Value[0].EndPos = sender.Position;
                        enemy.Value[0].Casted = true;
                        enemy.Value[0].TimeCasted = (int) Game.Time;
                        enemy.Value[0].OutOfBush = true;
                    }
                }
            }
        }

        private bool AddObject(Obj_AI_Hero hero, List<Ability> abilities)
        {
            if (Enemies.ContainsKey(hero))
                return false;
            Enemies.Add(hero, abilities);
            return true;
            //TODO:Add
        }

        private Vector3 CalculateEndPos(Ability ability, GameObjectProcessSpellCastEventArgs args)
        {
            float dist = Vector3.Distance(args.Start, args.End);
            if (dist <= ability.Range)
            {
                ability.EndPos = args.End;
            }
            else
            {
                Vector3 norm = args.Start - args.End;
                norm.Normalize();
                Vector3 endPos = args.Start - norm*ability.Range;

                //endPos = FindNearestNonWall(); TODO: Add FindNearestNonWall

                ability.EndPos = endPos;
            }
            return ability.EndPos;
        }

        private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;
            if (sender.GetType() == typeof (Obj_AI_Hero))
            {
                var hero = (Obj_AI_Hero) sender;
                if (hero.IsEnemy)
                {
                    Obj_AI_Hero enemy = hero;
                    foreach (var abilities in Enemies)
                    {
                        if (abilities.Key.NetworkId != enemy.NetworkId)
                            continue;
                        int index = 0;
                        foreach (Ability ability in abilities.Value)
                        {
                            if (args.SData.Name.ToLower() == "vayneinquisition")
                            {
                                if (ability.ExtraTicks > 0)
                                {
                                    ability.ExtraTicks = (int) Game.Time + 6 + 2*args.Level;
                                    return;
                                }
                            }
                            if (args.SData.Name.ToLower() == ability.SpellName.ToLower())
                            {
                                switch (ability.SpellName.ToLower())
                                {
                                    case "vaynetumble":
                                        if (Game.Time >= ability.ExtraTicks)
                                            return;
                                        ability.StartPos = args.Start;
                                        ability.EndPos = CalculateEndPos(ability, args);
                                        break;

                                    case "deceive":
                                        ability.OutOfBush = false;
                                        ability.StartPos = args.Start;
                                        ability.EndPos = CalculateEndPos(ability, args);
                                        break;

                                    case "leblancslidem":
                                        abilities.Value[index - 2].Casted = false;
                                        ability.StartPos = abilities.Value[index - 2].StartPos;
                                        ability.EndPos = CalculateEndPos(ability, args);
                                        break;

                                    case "leblancslidereturn":
                                    case "leblancslidereturnm":
                                        if (ability.SpellName == "leblancslidereturn")
                                        {
                                            abilities.Value[index - 1].Casted = false;
                                            abilities.Value[index + 1].Casted = false;
                                            abilities.Value[index + 2].Casted = false;
                                        }
                                        else
                                        {
                                            abilities.Value[index - 3].Casted = false;
                                            abilities.Value[index - 2].Casted = false;
                                            abilities.Value[index - 1].Casted = false;
                                        }
                                        ability.StartPos = args.Start;
                                        ability.EndPos = abilities.Value[index - 1].StartPos;
                                        break;

                                    case "fioraDance":
                                    case "alphaStrike":
                                        //TODO: Get Target
                                        //ability.Target = args.Target;
                                        ability.TargetDead = false;
                                        ability.StartPos = args.Start;
                                        //ability.EndPos = args.Target.Position;
                                        break;

                                    default:
                                        ability.StartPos = args.Start;
                                        ability.EndPos = CalculateEndPos(ability, args);
                                        break;
                                }
                                ability.Casted = true;
                                ability.TimeCasted = (int) Game.Time;
                                return;
                            }
                            index++;
                        }
                    }
                }
            }
        }

        private void Game_OnGameUpdate(object sender, EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
            foreach (var abilities in Enemies)
            {
                foreach (Ability ability in abilities.Value)
                {
                    if (ability.Casted)
                    {
                        if (ability.SpellName == "FioraDance" || ability.SpellName == "AlphaStrike" &&
                            !ability.TargetDead)
                        {
                            if (Game.Time > (ability.TimeCasted + ability.Delay + 0.2))
                                ability.Casted = false;
                                /*else if (ability.Target.IsDead()) TODO: Waiting for adding Target
                            {
                                Vector3 temp = ability.EndPos;
                                ability.EndPos = ability.StartPos;
                                ability.StartPos = temp;
                                ability.TargetDead = true;
                            }*/
                        }
                        else if (ability.Owner.IsDead ||
                                 (!ability.Owner.IsValid && Game.Time > (ability.TimeCasted + /*variable*/ 2)) ||
                                 (ability.Owner.IsVisible &&
                                  Game.Time > (ability.TimeCasted + /*variable*/ 5 + ability.Delay)))
                        {
                            ability.Casted = false;
                        }
                        else if (!ability.OutOfBush && ability.Owner.IsVisible &&
                                 Game.Time > (ability.TimeCasted + ability.Delay))
                        {
                            ability.EndPos = ability.Owner.ServerPosition;
                        }
                    }
                }
            }
        }

        public class Ability
        {
            public bool Casted;
            public float Delay;
            public Vector3 EndPos;
            public int ExtraTicks;
            public bool OutOfBush;
            public Obj_AI_Hero Owner;
            public int Range;
            public String SpellName;
            public Vector3 StartPos;
            public Obj_AI_Hero Target;
            public bool TargetDead;
            public int TimeCasted;

            public Ability(string spellName, int range, float delay, Obj_AI_Hero owner)
            {
                SpellName = spellName;
                Range = range;
                Delay = delay;
                Owner = owner;
            }
        }
    }
}
