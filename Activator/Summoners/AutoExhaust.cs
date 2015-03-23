using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAssemblies.Activators
{
    class AutoExhaust
    {

        private static SpellSlot _exhaust = SummonerSpells.GetExhaustSlot();
        public static Menu.MenuItemSettings AutoExhaustActivator = new Menu.MenuItemSettings(typeof(AutoExhaust));

        public AutoExhaust()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += ObjAIBase_OnProcessSpellCast;
        }

        ~AutoExhaust()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast -= ObjAIBase_OnProcessSpellCast;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoExhaustActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoExhaust", "SAssembliesSActivatorsAutoExhaust", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoExhaustActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOEXHAUST_MAIN"), "SAssembliesActivatorsAutoExhaust"));
            AutoExhaustActivator.MenuItems.Add(
                AutoExhaustActivator.Menu.AddItem(
                    new MenuItem("SAssembliesSActivatorsAutoExhaustAutoCast", Language.GetString("ACTIVATORS_AUTOEXHAUST_AUTOCAST")).SetValue(
                        new KeyBind(32, KeyBindType.Press))));
            AutoExhaustActivator.MenuItems.Add(
                AutoExhaustActivator.Menu.AddItem(
                    new MenuItem("SAssembliesSActivatorsAutoExhaustMinEnemies", Language.GetString("ACTIVATORS_AUTOEXHAUST_MIN_ENEMIES")).SetValue(
                        new Slider(3, 5, 1))));
            AutoExhaustActivator.MenuItems.Add(
                AutoExhaustActivator.Menu.AddItem(
                    new MenuItem("SAssembliesSActivatorsAutoExhaustAllyPercent", Language.GetString("ACTIVATORS_AUTOEXHAUST_ALLY_PERCENT")).SetValue(
                        new Slider(20, 100, 1))));
            AutoExhaustActivator.MenuItems.Add(
                AutoExhaustActivator.Menu.AddItem(
                    new MenuItem("SAssembliesSActivatorsAutoExhaustSelfPercent", Language.GetString("ACTIVATORS_AUTOEXHAUST_SELF_PERCENT")).SetValue(
                        new Slider(20, 100, 1))));
            AutoExhaustActivator.MenuItems.Add(
                AutoExhaustActivator.Menu.AddItem(
                    new MenuItem("SAssembliesSActivatorsAutoExhaustUseUltSpells", Language.GetString("ACTIVATORS_AUTOEXHAUST_ULTSPELLS")).SetValue(
                        false)));
            AutoExhaustActivator.MenuItems.Add(
                AutoExhaustActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoExhaustActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoExhaustActivator;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || _exhaust == SpellSlot.Unknown || ObjectManager.Player.Spellbook.GetSpell(_exhaust).State != SpellState.Ready ||
                ObjectManager.Player.IsDead || ObjectManager.Player.InFountain() ||
                ObjectManager.Player.HasBuff("Recall") || ObjectManager.Player.HasBuff("SummonerTeleport") ||
                ObjectManager.Player.HasBuff("RecallImproved") ||
                ObjectManager.Player.ServerPosition.CountEnemiesInRange(1500) > 0)
                return;

            Obj_AI_Hero enemy = Activator.GetHighestAdEnemy();
            if (enemy == null || !enemy.IsValid)
                return;
            int countE = ObjectManager.Player.ServerPosition.CountEnemiesInRange(750);
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsMe && !hero.IsEnemy && hero.IsValid &&
                    hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <= 900 &&
                    countE >=
                    AutoExhaustActivator.GetMenuItem(
                        "SAssembliesSActivatorsAutoExhaustMinEnemies").GetValue<Slider>().Value)
                {
                    int countA =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(units => !units.IsEnemy)
                            .Count(
                                units =>
                                    Vector2.Distance(ObjectManager.Player.Position.To2D(), units.Position.To2D()) <= 750);
                    float healthA = hero.Health / hero.MaxHealth * 100;
                    float healthE = enemy.Health / enemy.MaxHealth * 100;
                    if (
                        AutoExhaustActivator.GetMenuItem(
                            "SAssembliesSActivatorsAutoExhaustAutoCast").GetValue<KeyBind>().Active &&
                        Activator.IsFleeing(enemy) && !Activator.ImFleeing(enemy) && countA > 0)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_exhaust, enemy);
                    }
                    else if (
                        AutoExhaustActivator.GetMenuItem(
                            "SAssembliesSActivatorsAutoExhaustAutoCast").GetValue<KeyBind>().Active &&
                        !Activator.IsFleeing(enemy) && healthA < 25)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_exhaust, enemy);
                    }
                    else if (!Activator.IsFleeing(enemy) &&
                                healthA <=
                                AutoExhaustActivator.GetMenuItem(
                                    "SAssembliesSActivatorsAutoExhaustAllyPercent")
                                    .GetValue<Slider>()
                                    .Value)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_exhaust, enemy);
                    }
                    else if (!Activator.ImFleeing(enemy) && countA > 0 && Activator.IsFleeing(enemy) && healthE >= 10 &&
                                healthE <=
                                AutoExhaustActivator.GetMenuItem(
                                    "SAssembliesSActivatorsAutoExhaustSelfPercent")
                                    .GetValue<Slider>()
                                    .Value)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_exhaust, enemy);
                    }
                }
                foreach (var damage in Activator.Damages)
                {
                    if (damage.Key.NetworkId != ObjectManager.Player.NetworkId)
                        continue;

                    if (Activator.CalcMaxDamage(damage.Key, hero) >= damage.Key.Health &&
                        hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <= 900)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_exhaust);
                    }
                }
            }
        }

        private void ObjAIBase_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive() || _exhaust == SpellSlot.Unknown || ObjectManager.Player.Spellbook.GetSpell(_exhaust).State != SpellState.Ready ||
                !AutoExhaustActivator.GetMenuItem(
                    "SAssembliesSActivatorsAutoExhaustUseUltSpells").GetValue<bool>())
                return;

            if (sender.IsEnemy)
            {
                if (args.SData.Name.Contains("InfernalGuardian") || //Annie
                    args.SData.Name.Contains("BrandWildfire") || //Brand
                    args.SData.Name.Contains("CaitlynAceintheHole") || //Caitlyn
                    args.SData.Name.Contains("DravenRCast") || //Draven
                    args.SData.Name.Contains("EzrealTrueshotBarrage") || //Ezreal
                    args.SData.Name.Contains("Crowstorm") || //Fiddle
                    args.SData.Name.Contains("FioraDance") || //Fiora
                    args.SData.Name.Contains("FizzMarinerDoom") || //Fizz
                    args.SData.Name.Contains("GragasR") || //Gragas
                    args.SData.Name.Contains("GravesChargeShot") || //Graves
                    args.SData.Name.Contains("JinxR") || //Jinx
                    args.SData.Name.Contains("KatarinaR") || //Katarina
                    args.SData.Name.Contains("KennenShurikenStorm") || //Kennen
                    args.SData.Name.Contains("LissandraR") || //Lissandra
                    args.SData.Name.Contains("LuxMaliceCannon") || //Lux
                    args.SData.Name.Contains("AlZaharNetherGrasp") || //Malzahar
                    args.SData.Name.Contains("MissFortuneBulletTime") || //Miss Fortune
                    args.SData.Name.Contains("OrianaDetonateCommand") || //Orianna
                    args.SData.Name.Contains("RivenFengShuiEngine") || //Riven
                    args.SData.Name.Contains("SyndraR") || //Syndra
                    args.SData.Name.Contains("TalonShadowAssault") || //Talon
                    args.SData.Name.Contains("BusterShot") || //Tristana
                    args.SData.Name.Contains("FullAutomatic") || //Twitch
                    args.SData.Name.Contains("VeigarPrimordialBurst") || //Veigar
                    args.SData.Name.Contains("VelkozR") || //Vel Koz
                    args.SData.Name.Contains("ViktorChaosStorm") || //Viktor
                    args.SData.Name.Contains("MonkeyKingSpinToWin") || //Wukong
                    args.SData.Name.Contains("XerathLocusOfPower2") || //Xerath
                    args.SData.Name.Contains("YasuoRKnockUpComboW") || //Yasuo
                    args.SData.Name.Contains("ZiggsR") || //Ziggs
                    args.SData.Name.Contains("ZyraBrambleZone")) //Zyra
                {
                    if (sender.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <= 750)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_exhaust, sender);
                    }
                }

                if (args.SData.Name.Contains("SoulShackles") || //Morgana
                    args.SData.Name.Contains("KarthusFallenOne") || //Karthus
                    args.SData.Name.Contains("VladimirHemoplague")) //Vladimir
                {
                    Utility.DelayAction.Add(2500, () =>
                    {
                        if (sender.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <= 750)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(_exhaust, sender);
                        }
                    });
                }

                if (args.SData.Name.Contains("AbsoluteZero") || //Nunu
                    args.SData.Name.Contains("ZedUlt")) //Zed
                {
                    Utility.DelayAction.Add(500, () =>
                    {
                        if (sender.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <= 750)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(_exhaust, sender);
                        }
                    });
                }
            }
        }
    }
}
