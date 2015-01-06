using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using Newtonsoft.Json;
using SharpDX;
using SharpDX.Direct3D9;

namespace SAwareness.Miscs
{
    internal class AutoLevler
    {
        public static Menu.MenuItemSettings AutoLevlerMisc = new Menu.MenuItemSettings(typeof(AutoLevler));

        private int[] _priority = {0, 0, 0, 0};
        private int[] _sequence;
        private static int _useMode;
        private static List<SequenceLevler> sLevler = new List<SequenceLevler>();
        private int lastGameUpdateTime = 0;

        public AutoLevler()
        {
            //LoadLevelFile();
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceLoadChoice").ValueChanged += ChangeBuild_OnValueChanged;
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild").ValueChanged += ShowBuild_OnValueChanged;
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild").ValueChanged += NewBuild_OnValueChanged;
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceDeleteBuild").ValueChanged += DeleteBuild_OnValueChanged;

            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        ~AutoLevler()
        {
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceLoadChoice").ValueChanged -= ChangeBuild_OnValueChanged;
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild").ValueChanged -= ShowBuild_OnValueChanged;
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild").ValueChanged -= NewBuild_OnValueChanged;
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceDeleteBuild").ValueChanged -= DeleteBuild_OnValueChanged;

            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnWndProc -= Game_OnWndProc;
            sLevler = null;
        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && AutoLevlerMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            LoadLevelFile();
            Menu.MenuItemSettings tempSettings;
            AutoLevlerMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_AUTOLEVLER_MAIN"), "SAwarenessMiscsAutoLevler"));
            tempSettings = AutoLevlerMisc.AddMenuItemSettings(Language.GetString("MISCS_AUTOLEVLER_PRIORITY_MAIN"), "SAwarenessMiscsAutoLevlerPriority");
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(
                    new MenuItem("SAwarenessMiscsAutoLevlerPrioritySliderQ", "Q").SetValue(new Slider(0, 3, 0))));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(
                    new MenuItem("SAwarenessMiscsAutoLevlerPrioritySliderW", "W").SetValue(new Slider(0, 3, 0))));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(
                    new MenuItem("SAwarenessMiscsAutoLevlerPrioritySliderE", "E").SetValue(new Slider(0, 3, 0))));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(
                    new MenuItem("SAwarenessMiscsAutoLevlerPrioritySliderR", "R").SetValue(new Slider(0, 3, 0))));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(
                    new MenuItem("SAwarenessMiscsAutoLevlerPriorityFirstSpells", Language.GetString("MISCS_AUTOLEVLER_PRIORITY_MODE")).SetValue(new StringList(new[]
                    {
                        "Q W E", 
                        "Q E W", 
                        "W Q E", 
                        "W E Q", 
                        "E Q W", 
                        "E W Q"
                    }))));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLevlerPriorityFirstSpellsActive", Language.GetString("MISCS_AUTOLEVLER_PRIORITY_MODE_ACTIVE")).SetValue(false)));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLevlerPriorityActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false).DontSave()));
            tempSettings = AutoLevlerMisc.AddMenuItemSettings(Language.GetString("MISCS_AUTOLEVLER_SEQUENCE_MAIN"), "SAwarenessMiscsAutoLevlerSequence");
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLevlerSequenceLoadChoice", Language.GetString("MISCS_AUTOLEVLER_SEQUENCE_BUILD_CHOICE"))
                        .SetValue(GetBuildNames())
                            .DontSave()));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild", Language.GetString("MISCS_AUTOLEVLER_SEQUENCE_BUILD_LOAD")).SetValue(false)
                        .DontSave()));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild", Language.GetString("MISCS_AUTOLEVLER_SEQUENCE_CREATE_CHOICE")).SetValue(false)
                        .DontSave()));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLevlerSequenceDeleteBuild", Language.GetString("MISCS_AUTOLEVLER_SEQUENCE_DELETE_CHOICE")).SetValue(false)
                        .DontSave()));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(
                    new MenuItem("SAwarenessMiscsAutoLevlerSequenceActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false).DontSave()));
            AutoLevlerMisc.MenuItems.Add(
                AutoLevlerMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLevlerSMode", Language.GetString("GLOBAL_MODE")).SetValue(new StringList(new[]
                {
                    Language.GetString("MISCS_AUTOLEVLER_MODE_PRIORITY"), 
                    Language.GetString("MISCS_AUTOLEVLER_MODE_SEQUENCE"), 
                    Language.GetString("MISCS_AUTOLEVLER_MODE_R")
                }))));
            AutoLevlerMisc.MenuItems.Add(
                AutoLevlerMisc.Menu.AddItem(new MenuItem("SAwarenessMiscsAutoLevlerActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoLevlerMisc;
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            WriteLevelFile();
        }

        void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceLoadChoice").ValueChanged -= ChangeBuild_OnValueChanged;
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild").ValueChanged -= ShowBuild_OnValueChanged;
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild").ValueChanged -= NewBuild_OnValueChanged;
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceDeleteBuild").ValueChanged -= DeleteBuild_OnValueChanged;

            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnWndProc -= Game_OnWndProc;
            WriteLevelFile();
            sLevler = null;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (!IsActive() &&
                            (AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild").GetValue<bool>() ||
                            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild").GetValue<bool>()))
                return;
            HandleInput((WindowsMessages)args.Msg, Utils.GetCursorPos(), args.WParam);
        }

        private void HandleInput(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            HandleMainFrameClick(message, cursorPos, key);
            HandleSaveClick(message, cursorPos, key);
            HandleCancelClick(message, cursorPos, key);
        }

        private void HandleMainFrameClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_LBUTTONUP || !SequenceLevlerGUI.MainFrame.Sprite.Visible)
            {
                return;
            }
            if (Common.IsInside(cursorPos, SequenceLevlerGUI.MainFrame.Sprite.Position, SequenceLevlerGUI.MainFrame.Bitmap.Width, SequenceLevlerGUI.MainFrame.Bitmap.Height))
            {
                for (int i = 0; i < 4; i++)
                {
                    var row = SequenceLevlerGUI.SkillBlock[i];
                    for (int j = 0; j < 18; j++)
                    {
                        var column = row[j];
                        if (Common.IsInside(cursorPos, SequenceLevlerGUI.MainFrame.Sprite.Position + column, SequenceLevlerGUI.SkillBlockSize.Width,
                            SequenceLevlerGUI.SkillBlockSize.Height))
                        {
                            SequenceLevlerGUI.CurrentLevler.Sequence[j] = GetSpellSlot(i);
                        }
                    }
                }
            }
        }

        private void HandleSaveClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_LBUTTONUP || !SequenceLevlerGUI.Save.Sprite.Visible)
            {
                return;
            }
            if (Common.IsInside(cursorPos, SequenceLevlerGUI.Save.Sprite.Position, SequenceLevlerGUI.Save.Bitmap.Width, SequenceLevlerGUI.Save.Bitmap.Height))
            {
                SaveSequence(SequenceLevlerGUI.CurrentLevler.New);
                ResetMenuEntries();
            }
        }

        private void HandleCancelClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_LBUTTONUP || !SequenceLevlerGUI.Cancel.Sprite.Visible)
            {
                return;
            }
            if (Common.IsInside(cursorPos, SequenceLevlerGUI.Cancel.Sprite.Position, SequenceLevlerGUI.Cancel.Bitmap.Width, SequenceLevlerGUI.Cancel.Bitmap.Height))
            {
                ResetMenuEntries();
            }
        }

        private void ResetMenuEntries()
        {
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence")
                .GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild")
                .SetValue(false);
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence")
                .GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild")
                .SetValue(false);
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence")
                .GetMenuItem("SAwarenessMiscsAutoLevlerSequenceDeleteBuild")
                .SetValue(false);
        }

        private void ChangeBuild_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            StringList list = onValueChangeEventArgs.GetNewValue<StringList>();
            SequenceLevler curLevler = null;
            foreach (SequenceLevler levler in sLevler.ToArray())
            {
                if (levler.Name.Contains(list.SList[list.SelectedIndex]))
                {
                    curLevler = levler;
                }
            }
            if (curLevler != null)
            {
                SequenceLevlerGUI.CurrentLevler = new SequenceLevler(curLevler.Name, curLevler.Sequence);
            }
            else
            {
                SequenceLevlerGUI.CurrentLevler = new SequenceLevler();
            }
        }

        private void ShowBuild_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (onValueChangeEventArgs.GetNewValue<bool>())
            {
                StringList list =
                    AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence")
                        .GetMenuItem("SAwarenessMiscsAutoLevlerSequenceLoadChoice")
                        .GetValue<StringList>();
                SequenceLevler curLevler = null;
                foreach (SequenceLevler levler in sLevler.ToArray())
                {
                    if (list.SList[list.SelectedIndex].Equals(""))
                        continue;
                    if (levler.Name.Contains(list.SList[list.SelectedIndex]))
                    {
                        curLevler = levler;
                        break;
                    }
                }
                if (curLevler != null)
                {
                    SequenceLevlerGUI.CurrentLevler = new SequenceLevler(curLevler.Name, curLevler.Sequence);
                }
                else
                {
                    SequenceLevlerGUI.CurrentLevler = new SequenceLevler();
                }
                //gui.CurrentLevler = curLevler ?? new SequenceLevler();
            }
        }

        private void NewBuild_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (onValueChangeEventArgs.GetNewValue<bool>())
            {
                SequenceLevlerGUI.CurrentLevler = new SequenceLevler();
                SequenceLevlerGUI.CurrentLevler.Name = GetFreeSequenceName();
                SequenceLevlerGUI.CurrentLevler.ChampionName = ObjectManager.Player.ChampionName;
            }
        }

        private void DeleteBuild_OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (onValueChangeEventArgs.GetNewValue<bool>())
            {
                DeleteSequence();
                SequenceLevlerGUI.CurrentLevler = new SequenceLevler();
                onValueChangeEventArgs.Process = false;
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || lastGameUpdateTime + new Random().Next(500, 1000) > Environment.TickCount)
                return;

            lastGameUpdateTime = Environment.TickCount;

            var stringList = AutoLevlerMisc.GetMenuItem("SAwarenessMiscsAutoLevlerSMode").GetValue<StringList>();
            if (stringList.SelectedIndex == 0)
            {
                _useMode = 0;
                _priority = new[]
                {
                    AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerPriority")
                        .GetMenuItem("SAwarenessMiscsAutoLevlerPrioritySliderQ").GetValue<Slider>().Value,
                    AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerPriority")
                        .GetMenuItem("SAwarenessMiscsAutoLevlerPrioritySliderW").GetValue<Slider>().Value,
                    AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerPriority")
                        .GetMenuItem("SAwarenessMiscsAutoLevlerPrioritySliderE").GetValue<Slider>().Value,
                    AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerPriority")
                        .GetMenuItem("SAwarenessMiscsAutoLevlerPrioritySliderR").GetValue<Slider>().Value
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
                if (_useMode == 0 && AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerPriority")
                    .GetMenuItem("SAwarenessMiscsAutoLevlerPriorityActive").GetValue<bool>())
                {
                    if (AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerPriority")
                        .GetMenuItem("SAwarenessMiscsAutoLevlerPriorityFirstSpellsActive").GetValue<bool>())
                    {
                        player.Spellbook.LevelSpell(GetCurrentSpell());
                        return;
                    }
                    SpellSlot[] spellSlots = GetSortedPriotitySlots();
                    for (int slotId = 0; slotId <= 3; slotId++)
                    {
                        int spellLevel = player.Spellbook.GetSpell(spellSlots[slotId]).Level;
                        player.Spellbook.LevelSpell(spellSlots[slotId]);
                        if (player.Spellbook.GetSpell(spellSlots[slotId]).Level != spellLevel)
                            break;
                    }
                }
                else if (_useMode == 1)
                {
                    if (AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence")
                        .GetMenuItem("SAwarenessMiscsAutoLevlerSequenceActive").GetValue<bool>())
                    {
                        SpellSlot spellSlot = SequenceLevlerGUI.CurrentLevler.Sequence[player.Level - 1];
                        player.Spellbook.LevelSpell(spellSlot);
                    }
                }
                else
                {
                    if (AutoLevlerMisc.GetMenuItem("SAwarenessMiscsAutoLevlerSMode").GetValue<StringList>().SelectedIndex == 2)
                    {
                        if (ObjectManager.Player.Level == 6 ||
                            ObjectManager.Player.Level == 11 ||
                            ObjectManager.Player.Level == 16)
                        {
                            player.Spellbook.LevelSpell(SpellSlot.R);
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

        private static void SaveSequence(bool newEntry)
        {
            StringList list = AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceLoadChoice").GetValue<StringList>();
            //SpellSlot[] dummy = new SpellSlot[18];
            //String name = ObjectManager.Player.ChampionName;
            //foreach (SequenceLevler levler in sLevler)
            //{
            //    if (levler.Name.Contains(ObjectManager.Player.ChampionName))
            //    {
            //        name = levler.Name;
            //    }
            //}
            //int value = Convert.ToInt32(name[name.Length - 1]);
            //name = name.Remove(name.Length - 1);
            //name += value.ToString();
            if (SequenceLevlerGUI.CurrentLevler.New)
            {
                SequenceLevlerGUI.CurrentLevler.New = false;
                sLevler.Add(SequenceLevlerGUI.CurrentLevler);
                List<String> temp = list.SList.ToList();
                if (temp.Count == 1)
                {
                    if (temp[0].Equals(""))
                    {
                        temp.RemoveAt(0);
                    }
                    else
                    {
                        list.SelectedIndex += 1;
                    }
                }
                else
                {
                    list.SelectedIndex += 1;
                }
                temp.Add(SequenceLevlerGUI.CurrentLevler.Name);
                list.SList = temp.ToArray();
            }
            else
            {
                foreach (var levler in sLevler.ToArray())
                {
                    if (levler.Name.Equals(SequenceLevlerGUI.CurrentLevler.Name))
                    {
                        sLevler[list.SelectedIndex] = SequenceLevlerGUI.CurrentLevler;
                    }
                }
            }
            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceLoadChoice").SetValue<StringList>(list);
        }

        private static void WriteLevelFile()
        {
            string loc = Path.Combine(new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LeagueSharp", "Assemblies", "Config",
                "SAwareness", "autolevel.conf"
            });
            try
            {
                String output = JsonConvert.SerializeObject(sLevler);
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LeagueSharp",
                        "Config", "SAwareness"));
                if (output.Contains("[]"))
                {
                    throw new Exception("[], your latest changes are not getting saved!");
                }
                else
                {
                    File.WriteAllText(loc, output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't save autolevel.conf. Ex; {0}", ex);
            }
        }

        private static void LoadLevelFile()
        {
            string loc = Path.Combine(new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LeagueSharp", "Config",
                "SAwareness", "autolevel.conf"
            });
            try
            {
                sLevler = JsonConvert.DeserializeObject<List<SequenceLevler>>(File.ReadAllText(loc));
            }
            catch (Exception)
            {
                //Console.WriteLine("Couldn't load autolevel.conf.");
            }
        }

        public static StringList GetBuildNames()
        {
            StringList list = new StringList();
            if (sLevler == null)
            {
                sLevler = new List<SequenceLevler>();
            }
            if (sLevler.Count == 0)
            {
                list.SList = new[] { "" };
            }
            else
            {
                List<String> elements = new List<string>();
                foreach (SequenceLevler levler in sLevler)
                {
                    if (levler.ChampionName.Contains(ObjectManager.Player.ChampionName))
                    {
                        elements.Add(levler.Name);
                    }
                }
                if (elements.Count == 0)
                {
                    list.SList = new[] { "" };
                }
                else
                {
                    list = new StringList(elements.ToArray());
                }
            }
            return list;
        }

        private void DeleteSequence()
        {
            StringList list = AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceLoadChoice").GetValue<StringList>();
            foreach (SequenceLevler levler in sLevler.ToArray())
            {
                if (levler.Name.Contains(list.SList[list.SelectedIndex]))
                {
                    sLevler.Remove(levler);
                    List<String> temp = list.SList.ToList();
                    temp.RemoveAt(list.SelectedIndex);
                    if (temp.Count == 0)
                    {
                        temp.Add("");
                    }
                    if (list.SelectedIndex > 0)
                    {
                        list.SelectedIndex -= 1;
                    }
                    list.SList = temp.ToArray();
                    AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceLoadChoice").SetValue<StringList>(list);
                    break;
                }
            }
        }

        private static SpellSlot GetSpellSlot(int id)
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

        private static int GetSpellSlotId(SpellSlot spellSlot)
        {
            int id = -1;
            switch (spellSlot)
            {
                case SpellSlot.Q:
                    id = 0;
                    break;

                case SpellSlot.W:
                    id = 1;
                    break;

                case SpellSlot.E:
                    id = 2;
                    break;

                case SpellSlot.R:
                    id = 3;
                    break;
            }
            return id;
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
            switch (AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerPriority")
                .GetMenuItem("SAwarenessMiscsAutoLevlerPriorityFirstSpells").GetValue<StringList>().SelectedIndex)
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

        private String GetFreeSequenceName()
        {
            List<int> endings = new List<int>();
            List<SequenceLevler> sequences = new List<SequenceLevler>();
            for (int i = 0; i < sLevler.Count; i++)
            {
                if (sLevler[i].ChampionName.Contains(ObjectManager.Player.ChampionName))
                {
                    String ending = sLevler[i].Name.Substring(ObjectManager.Player.ChampionName.Length);
                    try
                    {
                        endings.Add(Convert.ToInt32(ending));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            for (int i = 0; i < 10000; i++)
            {
                if (!endings.Contains(i))
                {
                    return ObjectManager.Player.ChampionName + i;
                }
            }
            return ObjectManager.Player.ChampionName + 0;
        }

        [Serializable]
        private class SequenceLevler
        {
            public String Name;
            public String ChampionName;
            public SpellSlot[] Sequence = new SpellSlot[18];
            public bool New = true;

            public SequenceLevler(String name, SpellSlot[] sequence)
            {
                Name = name;
                Sequence = sequence;
                New = false;
                ChampionName = ObjectManager.Player.ChampionName;
            }

            public SequenceLevler()
            {
                // TODO: Complete member initialization
            }
        }

        private class SequenceLevlerGUI
        {
            public static SpriteHelper.SpriteInfo MainFrame;
            public static SpriteHelper.SpriteInfo Save;
            public static SpriteHelper.SpriteInfo Cancel;
            public static SpriteHelper.SpriteInfo[] Skill = new SpriteHelper.SpriteInfo[18];
            public static Render.Text[] Text = new Render.Text[4];
            public static SequenceLevler CurrentLevler = new SequenceLevler();
            public static Vector2 SkillStart = new Vector2(225, 45);
            public static Vector2 SkillIncrement = new Vector2(32.5f, 33); //35,35
            public static Vector2[][] SkillBlock;
            public static Size SkillBlockSize = new Size(28, 28); //30,30

            static SequenceLevlerGUI()
            {
                Vector2[][] list = new Vector2[4][];
                for (int j = 0; j < 4; j++)
                {
                    list[j] = new Vector2[18];
                    for (int i = 0; i < 18; i++)
                    {
                        list[j][i] = new Vector2(215 + ((i * SkillBlockSize.Width) + (i * 5)), 35 + ((j * SkillBlockSize.Height) + (j * 5)));
                    }
                }
                SkillBlock = list;

                MainFrame = new SpriteHelper.SpriteInfo();
                SpriteHelper.LoadTexture("SkillOrderGui", ref MainFrame, SpriteHelper.TextureType.Default);
                MainFrame.Sprite.PositionUpdate = delegate
                {
                    return new Vector2(Drawing.Width / 2 - MainFrame.Bitmap.Width / 2, Drawing.Height / 2 - MainFrame.Bitmap.Height / 2);
                };
                MainFrame.Sprite.VisibleCondition = delegate
                {
                    return Misc.Miscs.GetActive() && AutoLevlerMisc.GetActive() &&
                        (AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild").GetValue<bool>() ||
                        AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild").GetValue<bool>());
                };
                MainFrame.Sprite.Add(1);

                Save = new SpriteHelper.SpriteInfo();
                SpriteHelper.LoadTexture("SkillOrderGuiSave", ref Save, SpriteHelper.TextureType.Default);
                Save.Sprite.PositionUpdate = delegate
                {
                    return new Vector2(MainFrame.Sprite.Position.X, MainFrame.Sprite.Position.Y + MainFrame.Sprite.Height - Save.Sprite.Height);
                };
                Save.Sprite.VisibleCondition = delegate
                {
                    return Misc.Miscs.GetActive() && AutoLevlerMisc.GetActive() &&
                        (AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild").GetValue<bool>() ||
                        AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild").GetValue<bool>());
                };
                Save.Sprite.Add(1);

                Cancel = new SpriteHelper.SpriteInfo();
                SpriteHelper.LoadTexture("SkillOrderGuiCancel", ref Cancel, SpriteHelper.TextureType.Default);
                Cancel.Sprite.PositionUpdate = delegate
                {
                    return new Vector2(MainFrame.Sprite.Position.X + MainFrame.Sprite.Width - Cancel.Sprite.Width, MainFrame.Sprite.Position.Y + MainFrame.Sprite.Height - Cancel.Sprite.Height);
                };
                Cancel.Sprite.VisibleCondition = delegate
                {
                    return Misc.Miscs.GetActive() && AutoLevlerMisc.GetActive() &&
                        (AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild").GetValue<bool>() ||
                        AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild").GetValue<bool>());
                };
                Cancel.Sprite.Add(1);

                for (int index = 0; index <= 3; index++)
                {
                    int i = 0 + index;
                    Text[i] = new Render.Text(0, 0, "", 20, SharpDX.Color.LawnGreen);
                    Text[i].TextUpdate = delegate
                    {
                        return ObjectManager.Player.Spellbook.GetSpell(GetSpellSlot(i)).Name;
                    };
                    Text[i].PositionUpdate = delegate
                    {
                        return new Vector2(MainFrame.Sprite.Position.X + 30, MainFrame.Sprite.Position.Y + 40 + (i * 33));
                    };
                    Text[i].VisibleCondition = sender =>
                    {
                        return Misc.Miscs.GetActive() && AutoLevlerMisc.GetActive() &&
                        (AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild").GetValue<bool>() ||
                        AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild").GetValue<bool>());
                    };
                    Text[i].OutLined = true;
                    Text[i].Centered = false;
                    Text[i].Add(2);
                }

                for (int index = 0; index < 18; index++)
                {
                    int i = 0 + index;
                    Skill[i] = new SpriteHelper.SpriteInfo();
                    SpriteHelper.LoadTexture("SkillPoint", ref Skill[i], SpriteHelper.TextureType.Default);
                    Skill[i].Sprite.PositionUpdate = delegate
                    {
                        return GetSpellSlotPosition(GetSpellSlotId(CurrentLevler.Sequence[i]), i);
                    };
                    Skill[i].Sprite.VisibleCondition = delegate
                    {
                        return Misc.Miscs.GetActive() && AutoLevlerMisc.GetActive() &&
                            (AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceNewBuild").GetValue<bool>() ||
                            AutoLevlerMisc.GetMenuSettings("SAwarenessMiscsAutoLevlerSequence").GetMenuItem("SAwarenessMiscsAutoLevlerSequenceShowBuild").GetValue<bool>());
                    };
                    Skill[i].Sprite.Add(3);
                }
            }

            public SequenceLevlerGUI()
            {
                
            }

            private static Vector2 GetSpellSlotPosition(int row, int column)
            {
                return new Vector2(MainFrame.Sprite.X + SkillStart.X + (SkillIncrement.X * column), MainFrame.Sprite.Y + SkillStart.Y + (SkillIncrement.Y * row));
            }
        }
    }
}
