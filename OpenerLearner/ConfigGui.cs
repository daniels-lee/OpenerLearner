using Dalamud.Game.Gui.Toast;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenerHelper
{
    class ConfigGui : IDisposable
    {
        internal bool open = false;

        OpenerHelper p;

        private (string[] s, int i) CurrentlySelected = (new string[] { "Opener", "Rotation" }, 0);
        internal ConfigGui(OpenerHelper p)
        {
            this.p = p;
            Svc.PluginInterface.UiBuilder.Draw += Draw;
        }

        public void Dispose()
        {
            Svc.PluginInterface.UiBuilder.Draw -= Draw;
        }

        internal void Draw()
        {
            if (!open) return;
            if (ImGui.Begin("OpenerHelper configuration", ref open, ImGuiWindowFlags.MenuBar))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu(CurrentlySelected.s[CurrentlySelected.i]))
                    {
                        if (ImGui.MenuItem("Opener"))
                        {
                            CurrentlySelected.i = 0;
                            
                        }
                        if (ImGui.MenuItem("Rotation"))
                        {
                            CurrentlySelected.i = 1;
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenuBar();
                }
                if (CurrentlySelected.i == 0)
                {
                    foreach (var e in p.cfg.openerDic.Keys.ToArray())
                    {
                        if (ImGui.CollapsingHeader(Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.ClassJob>().GetRow(e).Name))
                        {
                            var text = string.Join(",", p.cfg.openerDic[e]);
                            ImGui.InputText("##opener" + e, ref text, 10000);
                            try
                            {
                                p.cfg.openerDic[e] = text.Split(',').Select(a => uint.Parse(a.Trim())).ToArray();
                            }
                            catch (Exception) { }
                        }
                    }
                }
                if (CurrentlySelected.i == 1)
                {
                    foreach (var e in p.cfg.rotationDic.Keys.ToArray())
                    {
                        if (ImGui.CollapsingHeader(Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.ClassJob>().GetRow(e).Name))
                        {
                            var text = string.Join(",", p.cfg.rotationDic[e]);
                            ImGui.InputText("##rotation" + e, ref text, 10000);
                            try
                            {
                                p.cfg.rotationDic[e] = text.Split(',').Select(a => uint.Parse(a.Trim())).ToArray();
                            }
                            catch (Exception) { }
                        }
                    }
                }


            }
            ImGui.End();
            if (!open)
            {
                if (CurrentlySelected.i == 0)
                {
                    p.currentSkills = p.cfg.openerDic[(byte)Svc.ClientState.LocalPlayer?.ClassJob.Id];
                }
                if (CurrentlySelected.i == 1)
                {
                    p.currentSkills = p.cfg.rotationDic[(byte)Svc.ClientState.LocalPlayer?.ClassJob.Id];
                }

                if (p.currentSkills.Length > 0)
                {
                    p.nextSkill = p.currentSkills[0];
                }
                p.cfg.Save();
                Svc.Toasts.ShowQuest("Configuration saved", new QuestToastOptions() { DisplayCheckmark = true, PlaySound = true });
            }
        }
    }
}
