using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evade;
using LeagueSharp;
using LeagueSharp.Common;
using SAssemblies;
using SharpDX;
using Menu = SAssemblies.Menu;

namespace SAssemblies.Activators
{
    class Activator
    {
        public static Dictionary<Obj_AI_Hero, List<IncomingDamage>> Damages =
            new Dictionary<Obj_AI_Hero, List<IncomingDamage>>();

        public static List<Skillshot> DetectedSkillshots = new List<Skillshot>();

        public static float _lastItemCleanseUse = 0;

        public static Menu.MenuItemSettings Activators = new Menu.MenuItemSettings();

        private Activator()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsEnemy)
                {
                    Damages.Add(hero, new List<IncomingDamage>());
                }
            }
            Damages.Add(new Obj_AI_Hero(), new List<IncomingDamage>());

            //Evade
            SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
            SkillshotDetector.OnDeleteMissile += OnDeleteMissile;
        }

        ~Activator()
        {
            SkillshotDetector.OnDetectSkillshot -= OnDetectSkillshot;
            SkillshotDetector.OnDeleteMissile -= OnDeleteMissile;
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SActivators", "SAssembliesSActivators", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            Language.SetLanguage();
            Activators.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Activators", "SAssembliesActivators"));
            Activators.MenuItems.Add(Activators.Menu.AddItem(new MenuItem("SAssembliesActivatorsActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return Activators;
        }

        public static List<BuffInstance> GetActiveCcBuffs(List<BuffType> buffs)
        {
            return GetActiveCcBuffs(ObjectManager.Player, buffs);
        }

        private static List<BuffInstance> GetActiveCcBuffs(Obj_AI_Hero hero, List<BuffType> buffs)
        {
            var nBuffs = new List<BuffInstance>();
            foreach (BuffInstance buff in hero.Buffs)
            {
                foreach (BuffType buffType in buffs)
                {
                    if (buff.Type == buffType)
                        nBuffs.Add(buff);
                }
            }
            return nBuffs;
        }

        private BuffInstance GetNegativBuff(Obj_AI_Hero hero)
        {
            foreach (BuffInstance buff in hero.Buffs)
            {
                if (buff.Name.Contains("fallenonetarget") || buff.Name.Contains("SoulShackles") ||
                    buff.Name.Contains("zedulttargetmark") || buff.Name.Contains("fizzmarinerdoombomb") ||
                    buff.Name.Contains("varusrsecondary"))
                    return buff;
            }
            return null;
        }

        public static bool IsCCd(Obj_AI_Hero hero)
        {
            var cc = new List<BuffType>
            {
                BuffType.Taunt,
                BuffType.Blind,
                BuffType.Charm,
                BuffType.Fear,
                BuffType.Polymorph,
                BuffType.Stun,
                BuffType.Silence,
                BuffType.Snare
            };

            return cc.Any(hero.HasBuffOfType);
        }

        private static double CheckForHit(Obj_AI_Hero hero)
        {
            List<IncomingDamage> damageList = Damages[Damages.Last().Key];
            double maxDamage = 0;
            foreach (IncomingDamage incomingDamage in damageList)
            {
                var pred = new PredictionInput();
                pred.Type = SkillshotType.SkillshotLine;
                pred.Radius = 50;
                pred.From = incomingDamage.StartPos;
                pred.RangeCheckFrom = incomingDamage.StartPos;
                pred.Range = incomingDamage.StartPos.Distance(incomingDamage.EndPos);
                pred.Collision = false;
                pred.Unit = hero;
                if (Prediction.GetPrediction(pred).Hitchance >= HitChance.Low)
                    maxDamage += incomingDamage.Dmg;
            }
            return maxDamage;
        }

        public static double CalcMaxDamage(Obj_AI_Hero hero, bool turret = true, bool minion = false, Obj_AI_Base source = null)
        {
            List<IncomingDamage> damageList = Damages[hero];
            double maxDamage = 0;
            foreach (IncomingDamage incomingDamage in damageList)
            {
                if (!turret && incomingDamage.Turret)
                    continue;
                if (!minion && incomingDamage.Minion)
                    continue;
                if (source != null)
                {
                    if(source.NetworkId == incomingDamage.Source.NetworkId)
                        maxDamage += incomingDamage.Dmg;
                }
                else
                {
                    maxDamage += incomingDamage.Dmg;
                }
            }
            return maxDamage /* + CheckForHit(hero)*/;
        }

        public static double CalcMaxDamage(Obj_AI_Hero hero, Obj_AI_Base source)
        {
            List<IncomingDamage> damageList = Damages[hero];
            double maxDamage = 0;
            foreach (IncomingDamage incomingDamage in damageList)
            {
                if (incomingDamage.Turret)
                    continue;
                if (incomingDamage.Minion)
                    continue;
                if (source != null)
                {
                    if (source.NetworkId == incomingDamage.Source.NetworkId)
                        maxDamage += incomingDamage.Dmg;
                }
                else
                {
                    maxDamage += incomingDamage.Dmg;
                }
            }
            return maxDamage /* + CheckForHit(hero)*/;
        }

        public static Obj_AI_Hero GetHighestAdEnemy()
        {
            Obj_AI_Hero highestAd = null;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    if (hero.IsValidTarget() && hero.Distance(ObjectManager.Player.ServerPosition) <= 650)
                    {
                        if (highestAd == null)
                        {
                            highestAd = hero;
                        }
                        else if (highestAd.BaseAttackDamage + highestAd.FlatPhysicalDamageMod <
                                 hero.BaseAttackDamage + hero.FlatPhysicalDamageMod)
                        {
                            highestAd = hero;
                        }
                    }
                }
            }
            return highestAd;
        }

        public static bool IsFleeing(Obj_AI_Hero hero)
        {
            if (hero.IsValid &&
                hero.ServerPosition.Distance(ObjectManager.Player.Position) >
                hero.Position.Distance(ObjectManager.Player.Position))
            {
                return true;
            }
            return false;
        }

        public static bool ImFleeing(Obj_AI_Hero hero)
        {
            if (hero.IsValid &&
                hero.Position.Distance(ObjectManager.Player.ServerPosition) >
                hero.Position.Distance(ObjectManager.Player.Position))
            {
                return true;
            }
            return false;
        }

        private static void OnDetectSkillshot(Skillshot skillshot)
        {
            bool alreadyAdded = false;

            foreach (Skillshot item in DetectedSkillshots)
            {
                if (item.SpellData.SpellName == skillshot.SpellData.SpellName &&
                    (item.Unit.NetworkId == skillshot.Unit.NetworkId &&
                     (skillshot.Direction).AngleBetween(item.Direction) < 5 &&
                     (skillshot.Start.Distance(item.Start) < 100 || skillshot.SpellData.FromObjects.Length == 0)))
                {
                    alreadyAdded = true;
                }
            }

            //Check if the skillshot is from an ally.
            if (skillshot.Unit.Team == ObjectManager.Player.Team)
            {
                return;
            }

            //Check if the skillshot is too far away.
            if (skillshot.Start.Distance(ObjectManager.Player.ServerPosition.To2D()) >
                (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
            {
                return;
            }

            //Add the skillshot to the detected skillshot list.
            if (!alreadyAdded)
            {
                //Multiple skillshots like twisted fate Q.
                if (skillshot.DetectionType == DetectionType.ProcessSpell)
                {
                    if (skillshot.SpellData.MultipleNumber != -1)
                    {
                        var originalDirection = skillshot.Direction;

                        for (int i = -(skillshot.SpellData.MultipleNumber - 1) / 2;
                            i <= (skillshot.SpellData.MultipleNumber - 1) / 2;
                            i++)
                        {
                            var end = skillshot.Start +
                                      skillshot.SpellData.Range *
                                      originalDirection.Rotated(skillshot.SpellData.MultipleAngle * i);
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                                skillshot.Unit);

                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "UFSlash")
                    {
                        skillshot.SpellData.MissileSpeed = 1600 + (int)skillshot.Unit.MoveSpeed;
                    }

                    if (skillshot.SpellData.Invert)
                    {
                        var newDirection = -(skillshot.End - skillshot.Start).Normalized();
                        var end = skillshot.Start + newDirection * skillshot.Start.Distance(skillshot.End);
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.Centered)
                    {
                        var start = skillshot.Start - skillshot.Direction * skillshot.SpellData.Range;
                        var end = skillshot.Start + skillshot.Direction * skillshot.SpellData.Range;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                    {
                        int angle = 60;
                        var edge1 =
                            (skillshot.End - skillshot.Unit.ServerPosition.To2D()).Rotated(
                                -angle / 2 * (float)Math.PI / 180);
                        var edge2 = edge1.Rotated(angle * (float)Math.PI / 180);

                        foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            Vector2 v = minion.ServerPosition.To2D() - skillshot.Unit.ServerPosition.To2D();
                            if (minion.Name == "Seed" && edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0 &&
                                minion.Distance(skillshot.Unit) < 800 &&
                                (minion.Team != ObjectManager.Player.Team))
                            {
                                Vector2 start = minion.ServerPosition.To2D();
                                var end = skillshot.Unit.ServerPosition.To2D()
                                    .Extend(
                                        minion.ServerPosition.To2D(),
                                        skillshot.Unit.Distance(minion) > 200 ? 1300 : 1000);

                                var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                                    skillshot.Unit);
                                DetectedSkillshots.Add(skillshotToAdd);
                            }
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "AlZaharCalloftheVoid")
                    {
                        Vector2 start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                        Vector2 end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsQ")
                    {
                        var d1 = skillshot.Start.Distance(skillshot.End);
                        float d2 = d1 * 0.4f;
                        float d3 = d2 * 0.69f;


                        var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                        var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");

                        Vector2 bounce1Pos = skillshot.End + skillshot.Direction * d2;
                        Vector2 bounce2Pos = bounce1Pos + skillshot.Direction * d3;

                        bounce1SpellData.Delay =
                            (int)(skillshot.SpellData.Delay + d1 * 1000f / skillshot.SpellData.MissileSpeed + 500);
                        bounce2SpellData.Delay =
                            (int)(bounce1SpellData.Delay + d2 * 1000f / bounce1SpellData.MissileSpeed + 500);

                        var bounce1 = new Skillshot(
                            skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos,
                            skillshot.Unit);
                        var bounce2 = new Skillshot(
                            skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos,
                            skillshot.Unit);

                        DetectedSkillshots.Add(bounce1);
                        DetectedSkillshots.Add(bounce2);
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsR")
                    {
                        skillshot.SpellData.Delay =
                            (int)(1500 + 1500 * skillshot.End.Distance(skillshot.Start) / skillshot.SpellData.Range);
                    }

                    if (skillshot.SpellData.SpellName == "JarvanIVDragonStrike")
                    {
                        var endPos = new Vector2();

                        foreach (Skillshot s in DetectedSkillshots)
                        {
                            if (s.Unit.NetworkId == skillshot.Unit.NetworkId && s.SpellData.Slot == SpellSlot.E)
                            {
                                endPos = s.End;
                            }
                        }

                        foreach (Obj_AI_Minion m in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (m.BaseSkinName == "jarvanivstandard" && m.Team == skillshot.Unit.Team &&
                                skillshot.IsDanger(m.Position.To2D()))
                            {
                                endPos = m.Position.To2D();
                            }
                        }

                        if (!endPos.IsValid())
                        {
                            return;
                        }

                        skillshot.End = endPos + 200 * (endPos - skillshot.Start).Normalized();
                        skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                    }
                }

                if (skillshot.SpellData.SpellName == "OriannasQ")
                {
                    var endCSpellData = SpellDatabase.GetByName("OriannaQend");

                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, endCSpellData, skillshot.StartTick, skillshot.Start, skillshot.End,
                        skillshot.Unit);

                    DetectedSkillshots.Add(skillshotToAdd);
                }


                //Dont allow fow detection.
                if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
                {
                    return;
                }
#if DEBUG
                Console.WriteLine(Environment.TickCount + "Adding new skillshot: " + skillshot.SpellData.SpellName);
#endif

                DetectedSkillshots.Add(skillshot);
            }
        }

        private static void OnDeleteMissile(Skillshot skillshot, Obj_SpellMissile missile)
        {
            if (skillshot.SpellData.SpellName == "VelkozQ")
            {
                var spellData = SpellDatabase.GetByName("VelkozQSplit");
                var direction = skillshot.Direction.Perpendicular();
                if (DetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") == 0)
                {
                    for (int i = -1; i <= 1; i = i + 2)
                    {
                        var skillshotToAdd = new Skillshot(
                            DetectionType.ProcessSpell, spellData, Environment.TickCount, missile.Position.To2D(),
                            missile.Position.To2D() + i * direction * spellData.Range, skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                    }
                }
            }
        }

        private void GetIncomingDamage_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            foreach (var damage in Damages)
            {
                foreach (IncomingDamage incomingDamage in damage.Value.ToArray())
                {
                    if (incomingDamage.TimeHit < Game.Time)
                        damage.Value.Remove(incomingDamage);
                }
                if (sender.NetworkId == damage.Key.NetworkId)
                    continue;
                //if (args.Target.Type == GameObjectType.obj_LampBulb || args.Target.Type == GameObjectType.Unknown)
                //    //No target, find it later
                //{
                //    try
                //    {
                //        double spellDamage = sender.GetSpellDamage((Obj_AI_Base) args.Target, args.SData.Name);
                //        if (spellDamage != 0.0f)
                //            Damages[Damages.Last().Key].Add(new IncomingDamage(args.SData.Name, sender, args.Start,
                //                args.End, spellDamage,
                //                IncomingDamage.CalcTimeHit(args.TimeCast, sender, damage.Key, args.End)));
                //    }
                //    catch (InvalidOperationException)
                //    {
                //        //Cannot find spell
                //    }
                //    catch (InvalidCastException)
                //    {
                //        //TODO Need a workaround to get the spelldamage for args.Target
                //        return;
                //    }
                //}
                if (args.SData.Name.ToLower().Contains("attack") && args.Target.NetworkId == damage.Key.NetworkId)
                {
                    double aaDamage = sender.GetAutoAttackDamage((Obj_AI_Base)args.Target);
                    if (aaDamage != 0.0f)
                        if (sender.Type == GameObjectType.obj_AI_Minion)
                        {
                            Damages[damage.Key].Add(new IncomingDamage(args.SData.Name, sender, args.Start, args.End,
                                aaDamage, IncomingDamage.CalcTimeHit(args.TimeCast, sender, damage.Key, args.End),
                                args.Target, false, true));
                        }
                        else if (sender.Type == GameObjectType.obj_AI_Turret)
                        {
                            Damages[damage.Key].Add(new IncomingDamage(args.SData.Name, sender, args.Start, args.End,
                                aaDamage, IncomingDamage.CalcTimeHit(args.TimeCast, sender, damage.Key, args.End),
                                args.Target, true));
                        }
                        else
                        {
                            Damages[damage.Key].Add(new IncomingDamage(args.SData.Name, sender, args.Start, args.End,
                                aaDamage, IncomingDamage.CalcTimeHit(args.TimeCast, sender, damage.Key, args.End),
                                args.Target));
                        }
                    continue;
                }
                if (sender.Type == GameObjectType.obj_AI_Hero && args.Target.NetworkId == damage.Key.NetworkId)
                {
                    try
                    {
                        double spellDamage = sender.GetSpellDamage((Obj_AI_Base)args.Target, args.SData.Name);
                        if (spellDamage != 0.0f)
                            Damages[damage.Key].Add(new IncomingDamage(args.SData.Name, sender, args.Start, args.End,
                                spellDamage, IncomingDamage.CalcTimeHit(args.TimeCast, sender, damage.Key, args.End),
                                args.Target));
                    }
                    catch (InvalidOperationException)
                    {
                        //Cannot find spell
                    }
                }
                if (sender.Type == GameObjectType.obj_AI_Turret && args.Target.NetworkId == damage.Key.NetworkId)
                    Damages[damage.Key].Add(new IncomingDamage(args.SData.Name, sender, args.Start, args.End, 300,
                        IncomingDamage.CalcTimeHit(args.TimeCast, sender, damage.Key, args.End), args.Target, true));
            }
        }

        private void GetIncomingDamage_OnGameUpdate()
        {
            DetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());
            var tempDamages =
                new Dictionary<Obj_AI_Hero, List<IncomingDamage>>(Damages);
            foreach (var damage in Damages)
            {
                Obj_AI_Hero hero = damage.Key;

                if (hero == null || !hero.IsValid)
                    continue;

                foreach (Skillshot skillshot in DetectedSkillshots)
                {
                    if (skillshot.IsAboutToHit(50, hero))
                    {
                        try
                        {
                            double spellDamage = skillshot.Unit.GetSpellDamage((Obj_AI_Base)hero,
                                skillshot.SpellData.SpellName);
                            bool exists = false;
                            foreach (IncomingDamage incomingDamage in tempDamages[hero])
                            {
                                if (incomingDamage.SpellName.Contains(skillshot.SpellData.SpellName))
                                {
                                    exists = true;
                                    break;
                                }
                            }
                            if (spellDamage != 0.0f && !exists)
                                continue;
                            tempDamages[hero].Add(new IncomingDamage(skillshot.SpellData.SpellName, skillshot.Unit,
                                skillshot.Start.To3D(), skillshot.End.To3D(), spellDamage, Game.Time + 0.05, hero));
                        }
                        catch (InvalidOperationException)
                        {
                            //Cannot find spell
                        }
                    }
                }
                tempDamages = BuffDamage(hero, tempDamages);
            }
            Damages = tempDamages;
        }

        private static Dictionary<Obj_AI_Hero, List<IncomingDamage>> BuffDamage(Obj_AI_Hero hero,
            Dictionary<Obj_AI_Hero, List<IncomingDamage>> tempDamages) //TODO: Add Ignite
        {
            foreach (BuffInstance buff in hero.Buffs)
            {
                if (buff.Type == BuffType.Poison || buff.Type == BuffType.Damage)
                {
                    foreach (Database.Spell spell in Database.GetSpellList())
                    {
                        if (string.Equals(spell.Name, buff.DisplayName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            try
                            {
                                DamageSpell damageSpell = null;
                                Obj_AI_Hero enemy = null;
                                foreach (Obj_AI_Hero champ in ObjectManager.Get<Obj_AI_Hero>())
                                {
                                    if (champ.IsEnemy)
                                    {
                                        foreach (SpellDataInst spellDataInst in champ.Spellbook.Spells)
                                        {
                                            if (string.Equals(spellDataInst.Name, spell.Name,
                                                StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                damageSpell = Damage.Spells[champ.ChampionName].FirstOrDefault(s =>
                                                {
                                                    if (s.Slot == spellDataInst.Slot)
                                                        return 0 == s.Stage;
                                                    return false;
                                                }) ??
                                                              Damage.Spells[champ.ChampionName].FirstOrDefault(
                                                                  s => s.Slot == spellDataInst.Slot);
                                                if (damageSpell != null)
                                                {
                                                    enemy = champ;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                double spellDamage = enemy.GetSpellDamage(hero, spell.Name);
                                bool exists = false;
                                foreach (IncomingDamage incomingDamage in tempDamages[hero])
                                {
                                    if (incomingDamage.SpellName.Contains(spell.Name))
                                    {
                                        exists = true;
                                        break;
                                    }
                                }
                                if (spellDamage != 0.0f && !exists)
                                    tempDamages[hero].Add(new IncomingDamage(spell.Name, enemy, new Vector3(),
                                        new Vector3(), spellDamage, buff.EndTime, hero));
                            }
                            catch (InvalidOperationException)
                            {
                                //Cannot find spell
                            }
                        }
                    }
                }
            }
            return tempDamages;
        }
    }



    public class IncomingDamage
    {
        public double Dmg;
        public Vector3 EndPos;
        public bool Minion;
        public Obj_AI_Base Source;
        public String SpellName;
        public Vector3 StartPos;
        public GameObject Target;
        public double TimeHit;
        public bool Turret;

        public IncomingDamage(String spellName, Obj_AI_Base source, Vector3 startPos, Vector3 endPos, double dmg,
            double timeHit, GameObject target = null, bool turret = false, bool minion = false)
        {
            SpellName = spellName;
            Source = source;
            StartPos = startPos;
            EndPos = endPos;
            Dmg = dmg;
            TimeHit = timeHit;
            Target = target;
            Turret = turret;
            Minion = minion;
        }

        public static double CalcTimeHit(double extraTimeForCast, Obj_AI_Base sender, Obj_AI_Base hero,
            Vector3 endPos) //TODO: Fix Time for animations etc
        {
            return Game.Time + (extraTimeForCast / 1000) * (sender.ServerPosition.Distance(endPos) / 1000) +
                   (hero.ServerPosition.Distance(sender.ServerPosition) / 1000);
        }

        public static double CalcTimeHit(double startTime, double extraTimeForCast, Obj_AI_Base sender,
            Obj_AI_Base hero, Vector3 endPos) //TODO: Fix Time for animations etc
        {
            return startTime + (extraTimeForCast / 1000) * (sender.ServerPosition.Distance(endPos) / 1000) +
                   (hero.ServerPosition.Distance(sender.ServerPosition) / 1000);
        }
    }
}
