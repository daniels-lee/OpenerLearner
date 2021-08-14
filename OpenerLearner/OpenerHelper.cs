﻿using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Game.Internal.Network;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenerHelper
{
    unsafe class OpenerHelper : IDalamudPlugin
    {
        internal DalamudPluginInterface pi;
        public string Name => "OpenerHelper";
        public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
        private string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

        private Drawer drawer;
        internal Configuration cfg;
        internal ConfigGui configGui;

        public Dictionary<uint, Lumina.Excel.GeneratedSheets.Action> ActionsDic { get; private set; }

        private const ushort FFXIVIpcSkillHandler = 373;
        private bool inCombat;
        private uint acId;
        internal uint[] currentSkills = new uint[] { 7524, 7507, 7524, 7505 };
        private uint[] openerSkills = new uint[] {7524,7507,7524,7505 };
        private uint[] rotationSkills = new uint[] { }; 
        private uint nextSkill;
        internal uint currentSkill = 0;


        public void Dispose()
        {
            pi.Framework.OnUpdateEvent -= Tick; 
            pi.Framework.Network.OnNetworkMessage -= NetworkMessageReceived;
            drawer.Dispose();
            cfg.Save();
            configGui.Dispose();
            pi.CommandManager.RemoveHandler("/opener");
            pi.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;
            pi.Framework.OnUpdateEvent += Tick;
            pi.Framework.Network.OnNetworkMessage += NetworkMessageReceived;
            drawer = new Drawer(this);
            ActionsDic = pi.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().ToDictionary(row => (uint)row.RowId, row => row);
            cfg = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            cfg.Initialize(pi);
            foreach (var e in pi.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.ClassJob>().Where(c => c.JobIndex > 0))
            {
                if (!cfg.openerDic.ContainsKey((byte)e.RowId))
                {
                    cfg.openerDic.Add((byte)e.RowId, new uint[] { });
                }
            }
            configGui = new ConfigGui(this);
            pi.UiBuilder.OnOpenConfigUi += delegate { configGui.open = true; };
            pi.CommandManager.AddHandler("/opener", new CommandInfo(delegate { configGui.open = true; }));

            nextSkill = currentSkills[0];
            
        }

        private void NetworkMessageReceived(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (opCode == FFXIVIpcSkillHandler && direction == NetworkMessageDirection.ZoneUp)
            {
                acId = *(uint*)(dataPtr + 0x4);
                if (currentSkills[currentSkill] == acId)
                {
                    currentSkill++;
                    nextSkill = currentSkills[currentSkill];
                }
                else
                {
                    pi.Framework.Gui.Chat.Print("Opener failed");
                }
                if(currentSkill==currentSkills.Length)
                {
                    pi.Framework.Gui.Chat.Print("Opener success");
                }
            }
        }

        private void Tick(Framework framework)
        {
            if((pi.ClientState.Condition[ConditionFlag.InCombat] != inCombat) && pi.ClientState.Condition[ConditionFlag.InCombat] == false)
            {
                pi.Framework.Gui.Chat.Print("Went out of combat");
                currentSkill = 0;
                nextSkill = currentSkills[currentSkill];
            }

            inCombat = pi.ClientState.Condition[ConditionFlag.InCombat];
        }
    }

    public enum ClassJob : byte
    {
        Adventurer = 0,
        Gladiator = 1,
        Pugilist = 2,
        Marauder = 3,
        Lancer = 4,
        Archer = 5,
        Conjurer = 6,
        Thaumaturge = 7,
        Carpenter = 8,
        Blacksmith = 9,
        Armorer = 10,
        Goldsmith = 11,
        Leatherworker = 12,
        Weaver = 13,
        Alchemist = 14,
        Culinarian = 15,
        Miner = 16,
        Botanist = 17,
        Fisher = 18,
        Paladin = 19,
        Monk = 20,
        Warrior = 21,
        Dragoon = 22,
        Bard = 23,
        WhiteMage = 24,
        BlackMage = 25,
        Arcanist = 26,
        Summoner = 27,
        Scholar = 28,
        Rogue = 29,
        Ninja = 30,
        Machinist = 31,
        DarkKnight = 32,
        Astrologian = 33,
        Samurai = 34,
        RedMage = 35,
        BlueMage = 36,
        Gunbreaker = 37,
        Dancer = 38
    }
}