using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Direct3D9;

namespace SAwareness
{
    internal class AutoLevler
    {
        private int[] _priority = {0, 0, 0, 0};
        private int[] _sequence;
        private static int _useMode;
        private static List<SequenceLevler> sLevler = new List<SequenceLevler>();

        public AutoLevler()
        {
            //LoadLevelFile();
            Game.OnGameUpdate += Game_OnGameUpdate;
            AppDomain.CurrentDomain.DomainUnload += delegate { WriteLevelFile(); };
            AppDomain.CurrentDomain.ProcessExit += delegate { WriteLevelFile(); };
        }

        ~AutoLevler()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            sLevler = null;
        }

        public bool IsActive()
        {
            return Menu.Misc.GetActive() && Menu.AutoLevler.GetActive();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;

            var stringList = Menu.AutoLevler.GetMenuItem("SAwarenessAutoLevlerSMode").GetValue<StringList>();
            if (stringList.SelectedIndex == 0)
            {
                _useMode = 0;
                _priority = new[]
                {
                    Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPrioritySliderQ").GetValue<Slider>().Value,
                    Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPrioritySliderW").GetValue<Slider>().Value,
                    Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPrioritySliderE").GetValue<Slider>().Value,
                    Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPrioritySliderR").GetValue<Slider>().Value
                };
            }
            else if (stringList.SelectedIndex == 1)
            {
                _useMode = 1;
            }
            else
            {
                _useMode = 2;
            }

            Obj_AI_Hero player = ObjectManager.Player;
            SpellSlot[] spellSlotst = GetSortedPriotitySlots();
            if (player.SpellTrainingPoints > 0)
            {
                //TODO: Add level logic// try levelup spell, if fails level another up etc.
                if (_useMode == 0 && Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                    .GetMenuItem("SAwarenessAutoLevlerPriorityActive").GetValue<bool>())
                {
                    if (Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPriorityFirstSpellsActive").GetValue<bool>())
                    {
                        player.Spellbook.LevelUpSpell(GetCurrentSpell());
                        return;
                    }
                    SpellSlot[] spellSlots = GetSortedPriotitySlots();
                    for (int slotId = 0; slotId <= 3; slotId++)
                    {
                        int spellLevel = player.Spellbook.GetSpell(spellSlots[slotId]).Level;
                        player.Spellbook.LevelUpSpell(spellSlots[slotId]);
                        if (player.Spellbook.GetSpell(spellSlots[slotId]).Level != spellLevel)
                            break;
                    }
                }
                else if (_useMode == 1)
                {
                    if (Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerSequence")
                        .GetMenuItem("SAwarenessAutoLevlerSequenceActive").GetValue<bool>())
                    {
                        SpellSlot spellSlot = GetSpellSlot(_sequence[player.Level - 1]);
                        player.Spellbook.LevelUpSpell(spellSlot);
                    }
                }
                else
                {
                    if (Menu.AutoLevler.GetMenuItem("SAwarenessAutoLevlerSMode").GetValue<StringList>().SelectedIndex == 2)
                    {
                        if (ObjectManager.Player.Level == 6 ||
                            ObjectManager.Player.Level == 11 ||
                            ObjectManager.Player.Level == 16)
                        {
                            player.Spellbook.LevelUpSpell(SpellSlot.R);
                        }
                    }
                }
            }
        }

        public void SetPriorities(int priorityQ, int priorityW, int priorityE, int priorityR)
        {
            _sequence[0] = priorityQ;
            _sequence[1] = priorityW;
            _sequence[2] = priorityE;
            _sequence[3] = priorityR;
        }

        private static void SaveSequence(/*GUI Sequenz übergeben*/)
        {
            Dictionary<int, SpellSlot> dummy = new Dictionary<int, SpellSlot>();
            String name = ObjectManager.Player.ChampionName;
            foreach (SequenceLevler levler in sLevler)
            {
                if (levler.Name.Contains(ObjectManager.Player.ChampionName))
                {
                    name = levler.Name;
                }
            }
            int value = Convert.ToInt32(name[name.Length - 1]);
            name = name.Remove(name.Length - 1);
            name += value.ToString();
            sLevler.Add(new SequenceLevler(name, dummy));
        }

        private static void WriteLevelFile()
        {
            string loc = Config.LeagueSharpDirectory;
            loc = loc.Remove(loc.LastIndexOf("\\", StringComparison.Ordinal));
            loc = loc + "\\Config\\SAwareness\\autolevel.conf";
            try
            {
                StreamWriter sw = File.CreateText(loc);
                sw.Close();
                Serialize.Save(loc, sLevler);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't save autolevel.conf.");
            }
        }

        private static void LoadLevelFile()
        {
            string loc = Config.LeagueSharpDirectory;
            loc = loc.Remove(loc.LastIndexOf("\\", StringComparison.Ordinal));
            loc = loc + "\\Config\\SAwareness\\autolevel.conf";
            try
            {
            if (!File.Exists(loc))
            {
                StreamWriter sw = File.CreateText(loc);
                sw.Close();
            }
            
                StreamReader sr = File.OpenText(loc);
                ReadLevelFile(loc);
                sr.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't load autolevel.conf. Using priority mode.");
                _useMode = 0;
            }
        }

        private static void ReadLevelFile(String loc)
        {
            //sLevler = Serialize.Load<SequenceLevler>(loc);
        }

        public static StringList GetBuildNames()
        {
            StringList list = new StringList();
            list.SList = new[] {""};
            //List<String> elements = new List<string>();
            //LoadLevelFile();
            //foreach (SequenceLevler levler in sLevler)
            //{
            //    if (levler.Name.Contains(ObjectManager.Player.ChampionName))
            //    {
            //        elements.Add(levler.Name);
            //    }
            //}
            //list = new StringList(elements.ToArray());
            return list;
        }

        private void DeleteSequence()
        {
            StringList list = Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerSequence").GetMenuItem("SAwarenessAutoLevlerSequenceLoadChoice").GetValue<StringList>();
            if (Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerSequence")
                .GetMenuItem("SAwarenessAutoLevlerSequenceDeleteBuild").GetValue<bool>())
            {
                foreach (SequenceLevler levler in sLevler.ToArray())
                {
                    if (levler.Name.Contains(list.SList[list.SelectedIndex]))
                    {
                        sLevler.Remove(levler);
                    }
                }
            }
        }

        private SpellSlot GetSpellSlot(int id)
        {
            var spellSlot = SpellSlot.Unknown;
            switch (id)
            {
                case 0:
                    spellSlot = SpellSlot.Q;
                    break;

                case 1:
                    spellSlot = SpellSlot.W;
                    break;

                case 2:
                    spellSlot = SpellSlot.E;
                    break;

                case 3:
                    spellSlot = SpellSlot.R;
                    break;
            }
            return spellSlot;
        }

        private SpellSlot[] GetSortedPriotitySlots()
        {
            int[] listOld = _priority;
            var listNew = new SpellSlot[4];

            listNew = ToSpellSlot(listOld, listNew);

            //listNew = listNew.OrderByDescending(c => c).ToList();


            return listNew;
        }

        private SpellSlot[] ToSpellSlot(int[] listOld, SpellSlot[] listNew)
        {
            for (int i = 0; i <= 3; i++)
            {
                switch (listOld[i])
                {
                    case 0:
                        listNew[0] = GetSpellSlot(i);
                        break;

                    case 1:
                        listNew[1] = GetSpellSlot(i);
                        break;

                    case 2:
                        listNew[2] = GetSpellSlot(i);
                        break;

                    case 3:
                        listNew[3] = GetSpellSlot(i);
                        break;
                }
            }
            return listNew;
        }

        private SpellSlot GetCurrentSpell()
        {
            SpellSlot[] spellSlot = null;
            switch (Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                .GetMenuItem("SAwarenessAutoLevlerPriorityFirstSpells").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    spellSlot = new[] {SpellSlot.Q, SpellSlot.W, SpellSlot.E};
                    break;
                case 1:
                    spellSlot = new[] { SpellSlot.Q, SpellSlot.E, SpellSlot.W };
                    break;
                case 2:
                    spellSlot = new[] { SpellSlot.W, SpellSlot.Q, SpellSlot.E };
                    break;
                case 3:
                    spellSlot = new[] { SpellSlot.W, SpellSlot.E, SpellSlot.Q };
                    break;
                case 4:
                    spellSlot = new[] { SpellSlot.E, SpellSlot.Q, SpellSlot.W };
                    break;
                case 5:
                    spellSlot = new[] { SpellSlot.E, SpellSlot.W, SpellSlot.Q };
                    break;
            }
            return spellSlot[ObjectManager.Player.Level - 1];
        }

        private SpellSlot ConvertSpellSlot(String spell)
        {
            switch (spell)
            {
                case "Q":
                    return SpellSlot.Q;

                case "W":
                    return SpellSlot.W;

                case "E":
                    return SpellSlot.E;

                case "R":
                    return SpellSlot.R;

                default:
                    return SpellSlot.Unknown;
            }
        }

        //private List<SpellSlot> SortAlgo(List<int> listOld, List<SpellSlot> listNew)
        //{
        //    int highestPriority = -1;
        //    for (int i = 0; i < listOld.Count; i++)
        //    {
        //        int prio = _priority[i];
        //        if (highestPriority < prio)
        //        {
        //            highestPriority = prio;
        //            listNew.Add(GetSpellSlot(i));
        //            listOld.Remove(_priority[i]);
        //        }
        //    }
        //    if (listOld.Count > 1)
        //        listNew = SortAlgo(listOld, listNew);
        //    return listNew;
        //}

        [Serializable]
        private class SequenceLevler
        {
            public String Name;
            public Dictionary<int, SpellSlot> Sequence = new Dictionary<int, SpellSlot>();

            public SequenceLevler(String name, Dictionary<int, SpellSlot> sequence)
            {
                Name = name;
                Sequence = sequence;
            }
        }

        private class SequenceLevlerGUI
        {
            public Render.Sprite MainFrame;
            public Render.Sprite Save;
            public Render.Sprite[] Skill = new Render.Sprite[18];

            public SequenceLevlerGUI()
            {
                //MainFrame = new Render.Sprite();
                //texture = Texture.FromMemory(Drawing.Direct3DDevice, MyResources[name.ToLower()]);
            }
        }
    }
}
