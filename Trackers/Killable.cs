﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAssemblies.Trackers
{
    class Killable
    {
        public static Menu.MenuItemSettings KillableTracker = new Menu.MenuItemSettings(typeof(Killable));

        Dictionary<Obj_AI_Hero, InternalKillable> _enemies = new Dictionary<Obj_AI_Hero, InternalKillable>();
        private int lastGameUpdateTime = 0;

        public Killable() //TODO: Add more option for e.g. most damage first, add ignite spell
        {
            GameUpdate a = null;
            a = delegate(EventArgs args)
            {
                Init();
                Game.OnUpdate -= a;
            };
            Game.OnUpdate += a;
            //ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
            Game.OnUpdate += Game_OnGameUpdate;
        }

        ~Killable()
        {
            //ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            Game.OnUpdate -= Game_OnGameUpdate;
            _enemies = null;
        }

        public bool IsActive()
        {  
#if TRACKERS
            return Tracker.Trackers.GetActive() && KillableTracker.GetActive();
#else
            return KillableTracker.GetActive();
#endif
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            KillableTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_KILLABLE_MAIN"), "SAssembliesTrackersKillable"));
            KillableTracker.MenuItems.Add(
                KillableTracker.Menu.AddItem(new MenuItem("SAssembliesTrackersKillableSpeech", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            KillableTracker.MenuItems.Add(KillableTracker.CreateActiveMenuItem("SAssembliesTrackersKillableActive", () => new Killable()));
            return KillableTracker;
        }

        private void Init()
        {
            int index = 0;
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                int i = 0 + index;
                if (enemy.IsEnemy)
                {
                    Combo nCombo = CalculateKillable(enemy, null);
                    InternalKillable killable = new InternalKillable(null, null);
                    Vector2 barOffset = new Vector2(10f, 29f);
                    int barWidth = 104;
                    Render.Text text = new Render.Text(new Vector2(0, 0), "", 28, SharpDX.Color.OrangeRed);
                    text.Centered = true;
                    text.OutLined = true;
                    text.VisibleCondition = delegate
                    {
                        return (killable.Combo != null ? killable.Combo.Killable : false) && enemy.IsVisible && !enemy.IsDead &&
                            IsActive();
                    };
                    text.PositionUpdate = delegate
                    {
                        return new Vector2(Drawing.Width / 2, Drawing.Height * 0.80f - (17 * i));
                    };
                    text.TextUpdate = delegate
                    {
                        if (killable.Combo == null)
                            return "";
                        Combo combo = killable.Combo;
                        String killText = "Killable " + enemy.ChampionName + ": AA / ";
                        if (combo.Spells != null && combo.Spells.Count > 0)
                            combo.Spells.ForEach(x => killText += x.Name + "/");
                        if (combo.Items != null && combo.Items.Count > 0)
                            combo.Items.ForEach(x => killText += x.Name + "/");
                        if (killText.Contains("/"))
                            killText = killText.Remove(killText.LastIndexOf("/"));
                        return killText;
                    };
                    text.Add();

                    Render.Rectangle rect = new Render.Rectangle(0, 0, 1, 10, SharpDX.Color.Gray);
                    rect.VisibleCondition = delegate
                    {
                        return enemy.IsVisible && !enemy.IsDead &&
                            enemy.IsHPBarRendered && enemy.Position.IsOnScreen() && IsActive();
                    };
                    rect.PositionUpdate = delegate
                    {
                        if (killable.Combo == null)
                            return new Vector2(0, 0);

                        double damagePercentage = ((enemy.Health - killable.Combo.Damage) > 0 ? (enemy.Health - killable.Combo.Damage) : 0) /
                                               enemy.MaxHealth;

                        Vector2 startPos =
                            new Vector2((float)(enemy.HPBarPosition.X + barOffset.X + damagePercentage * barWidth),
                                (float)(enemy.HPBarPosition.Y + barOffset.Y) - 10);
                        rect.Width = (int)((enemy.HPBarPosition.X + barOffset.X + (enemy.Health / enemy.MaxHealth) * barWidth) + 1 -
                                        startPos.X);
                        //rect.Height = 5;
                        return startPos;
                    };
                    rect.Add();

                    killable = new InternalKillable(nCombo, text);
                    _enemies.Add(enemy, killable);
                }
                index++;
            }
        }

        private void CalculateKillable()
        {
            foreach (var enemy in _enemies.ToArray())
            {
                _enemies[enemy.Key].Combo = (CalculateKillable(enemy.Key, enemy.Value));
            }
        }

        private Combo CalculateKillable(Obj_AI_Hero enemy, InternalKillable killable)
        {
            var creationItemList = new Dictionary<Item, Damage.DamageItems>();
            var creationSpellList = new List<LeagueSharp.Common.Spell>();
            var tempSpellList = new List<Spell>();
            var tempItemList = new List<Item>();

            var ignite = new LeagueSharp.Common.Spell(SummonerSpells.GetIgniteSlot(), 1000);

            var q = new LeagueSharp.Common.Spell(SpellSlot.Q, 1000);
            var w = new LeagueSharp.Common.Spell(SpellSlot.W, 1000);
            var e = new LeagueSharp.Common.Spell(SpellSlot.E, 1000);
            var r = new LeagueSharp.Common.Spell(SpellSlot.R, 1000);
            creationSpellList.Add(q);
            creationSpellList.Add(w);
            creationSpellList.Add(e);
            creationSpellList.Add(r);

            var dfg = new Item(3128, 1000, "Dfg");//Items.Deathfire_Grasp;
            var bilgewater = new Item(3144, 1000, "Bilgewater");//Items.Bilgewater_Cutlass;//
            var hextechgun = new Item(3146, 1000, "Hextech");//Items.Hextech_Gunblade;//
            var blackfire = new Item(3188, 1000, "Blackfire");//Items.Blackfire_Torch;//
            var botrk = new Item(3153, 1000, "Botrk");//Items.Blade_of_the_Ruined_King;//
            creationItemList.Add(dfg, Damage.DamageItems.Dfg);
            creationItemList.Add(bilgewater, Damage.DamageItems.Bilgewater);
            creationItemList.Add(hextechgun, Damage.DamageItems.Hexgun);
            creationItemList.Add(blackfire, Damage.DamageItems.BlackFireTorch);
            creationItemList.Add(botrk, Damage.DamageItems.Botrk);

            double enoughDmg = 0;
            double enoughMana = 0;

            enoughDmg += ObjectManager.Player.GetAutoAttackDamage(enemy, true);
            if (enemy.Health < enoughDmg)
            {
                Speak(killable, enemy);
                return new Combo(tempSpellList, tempItemList, true, enoughDmg);
            }

            foreach (var item in creationItemList)
            {
                if (item.Key.IsReady())
                {
                    enoughDmg += ObjectManager.Player.GetItemDamage(enemy, item.Value);
                    tempItemList.Add(item.Key);
                }
                if (enemy.Health < enoughDmg)
                {
                    Speak(killable, enemy);
                    return new Combo(null, tempItemList, true, enoughDmg);
                }
            }

            foreach (LeagueSharp.Common.Spell spell in creationSpellList)
            {
                if (spell.IsReady())
                {
                    double spellDamage = spell.GetDamage(enemy, 0);
                    if (spellDamage > 0)
                    {
                        enoughDmg += spellDamage;
                        enoughMana += spell.Instance.ManaCost;
                        tempSpellList.Add(new Spell(spell.Slot.ToString(), spell.Slot));
                    }
                }
                if (enemy.Health < enoughDmg)
                {
                    if (ObjectManager.Player.Mana >= enoughMana)
                    {
                        Speak(killable, enemy);
                        return new Combo(tempSpellList, tempItemList, true, enoughDmg);
                    }
                    return new Combo(null, null, false, 0);
                }
            }

            if (SummonerSpells.GetIgniteSlot() != SpellSlot.Unknown && enemy.Health > enoughDmg)
            {
                enoughDmg += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                tempSpellList.Add(new Spell("Ignite", ignite.Slot));
            }

            if (enemy.Health < enoughDmg)
            {
                Speak(killable, enemy);
                return new Combo(tempSpellList, tempItemList, true, enoughDmg);
            }
            if (killable != null)
            {
                killable.Spoken = false;
            }
            return new Combo(tempSpellList, tempItemList, false, enoughDmg);
        }

        private void Speak(InternalKillable killable, Obj_AI_Hero hero)
        {
            if (killable != null)
            {
                if (KillableTracker.GetMenuItem("SAssembliesTrackersKillableSpeech").GetValue<bool>() && !killable.Spoken && hero.IsVisible && !hero.IsDead)
                {
                    Speech.Speak("Killable " + hero.ChampionName);
                    killable.Spoken = true;
                }
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;
            CalculateKillable();
        }

        public class InternalKillable
        {
            public Combo Combo;
            public Render.Text Text;
            public bool Spoken = false;

            public InternalKillable(Combo combo, Render.Text text)
            {
                Combo = combo;
                Text = text;
            }
        }

        public class Combo
        {
            public List<Item> Items = new List<Item>();
            public bool Killable = false;
            public List<Spell> Spells = new List<Spell>();
            public double Damage = 0;

            public Combo(List<Spell> spells, List<Item> items, bool killable, double damage)
            {
                Spells = spells;
                Items = items;
                Killable = killable;
                Damage = damage;
            }

            public Combo()
            {
            }
        }

        public class Item : Items.Item
        {
            public String Name;

            public Item(int id, float range, String name)
                : base(id, range)
            {
                Name = name;
            }
        }

        public class Spell
        {
            public String Name;
            public SpellSlot SpellSlot;

            public Spell(String name, SpellSlot spellSlot)
            {
                Name = name;
                SpellSlot = spellSlot;
            }
        }
    }
}
