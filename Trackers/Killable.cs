using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness.Trackers
{
    class Killable
    {
        public static Menu.MenuItemSettings KillableTracker = new Menu.MenuItemSettings(typeof(Killable));

        Dictionary<Obj_AI_Hero, InternalKillable> _enemies = new Dictionary<Obj_AI_Hero, InternalKillable>();
        private int lastGameUpdateTime = 0;

        public Killable() //TODO: Add more option for e.g. most damage first, add ignite spell
        {
            int index = 0;
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                int i = 0 + index;
                if (enemy.IsEnemy)
                {
                    Combo nCombo = CalculateKillable(enemy, null);
                    InternalKillable killable = new InternalKillable(null, null);
                    Render.Text text = new Render.Text(new Vector2(0, 0), "", 28, SharpDX.Color.OrangeRed);
                    text.Centered = true;
                    text.OutLined = true;
                    text.VisibleCondition = sender =>
                    {
                        return (killable.Combo != null ? killable.Combo.Killable : false) && enemy.IsVisible && !enemy.IsDead &&
                            Tracker.Trackers.GetActive() && KillableTracker.GetActive();
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
                        String killText = "Killable " + enemy.ChampionName + ": ";
                        if (combo.Spells != null && combo.Spells.Count > 0)
                            combo.Spells.ForEach(x => killText += x.Name + "/");
                        if (combo.Items != null && combo.Items.Count > 0)
                            combo.Items.ForEach(x => killText += x.Name + "/");
                        if (killText.Contains("/"))
                            killText = killText.Remove(killText.LastIndexOf("/"));
                        return killText;
                    };
                    text.Add();
                    killable = new InternalKillable(nCombo, text);
                    _enemies.Add(enemy, killable);
                }
                index++;
            }
            ThreadHelper.GetInstance().Called += Game_OnGameUpdate;
            //Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~Killable()
        {
            ThreadHelper.GetInstance().Called -= Game_OnGameUpdate;
            //Game.OnGameUpdate -= Game_OnGameUpdate;
            _enemies = null;
        }

        public bool IsActive()
        {
            return Tracker.Trackers.GetActive() && KillableTracker.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            KillableTracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("TRACKERS_KILLABLE_MAIN"), "SAwarenessTrackersKillable"));
            KillableTracker.MenuItems.Add(
                KillableTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersKillableSpeech", Language.GetString("GLOBAL_VOICE")).SetValue(false)));
            KillableTracker.MenuItems.Add(
                KillableTracker.Menu.AddItem(new MenuItem("SAwarenessTrackersKillableActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return KillableTracker;
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

            var ignite = new LeagueSharp.Common.Spell(Activator.GetIgniteSlot(), 1000);

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
                    return new Combo(null, tempItemList, true);
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
                        return new Combo(tempSpellList, tempItemList, true);
                    }
                    return new Combo(null, null, false);
                }
            }

            if (Activator.GetIgniteSlot() != SpellSlot.Unknown && enemy.Health > enoughDmg)
            {
                enoughDmg += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                tempSpellList.Add(new Spell("Ignite", ignite.Slot));
            }
            if (enemy.Health < enoughDmg)
            {
                Speak(killable, enemy);
                return new Combo(tempSpellList, tempItemList, true);
            }
            if (killable != null)
            {
                killable.Spoken = false;
            }
            return new Combo();
        }

        private void Speak(InternalKillable killable, Obj_AI_Hero hero)
        {
            if (killable != null)
            {
                if (KillableTracker.GetMenuItem("SAwarenessTrackersKillableSpeech").GetValue<bool>() && !killable.Spoken && hero.IsVisible && !hero.IsDead)
                {
                    Speech.Speak("Killable " + hero.ChampionName);
                    killable.Spoken = true;
                }
            }
        }

        void Game_OnGameUpdate(object sender, EventArgs args)
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

            public Combo(List<Spell> spells, List<Item> items, bool killable)
            {
                Spells = spells;
                Items = items;
                Killable = killable;
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
