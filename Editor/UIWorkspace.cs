using Hexa.NET.ImGui;
using SANS.Editor.Panels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SANS.Editor {
    public static class UIWorkspace {
        static private Dictionary<string, WorkspaceTab> _tabsOpened = new Dictionary<string, WorkspaceTab>();
        static private List<WorkspaceTab> _tabsToInit = new List<WorkspaceTab>();
        static private List<WorkspaceTab> _tabsToRemove = new List<WorkspaceTab>();

        ///////////////////////////////////////////////
        /// CLASSES
        ///////////////////////////////////////////////
        
        // Dialogue File
        public class DialogueFile {
            public string FilePath { get; set; }
            public string FileName => System.IO.Path.GetFileName(FilePath);

            public string Name { get; set; }
            public string Type { get; set; }

            public bool IsDirty = false;
        }

        // Tab
        public class WorkspaceTab {
            public DialogueFile CurrentDialogueFile = null;
            public BaseDocumentPanel CurrentDocumentPanel = null;

            public void Init() {
                IsOpen = true;
                _tabsOpened[CurrentDialogueFile.Name] = this;
            }

            public bool IsOpen = false;
            public bool IsDirty = false;
        }

        ///////////////////////////////////////////////
        // CALLS
        ///////////////////////////////////////////////
        public static void Draw() {
            ImGui.Begin("Workspace");

            if (ImGui.BeginTabBar("WorkspaceTabBar")) {

                // Render panels
                foreach (WorkspaceTab _tab in _tabsOpened.Values) {
                    if (ImGui.BeginTabItem(_tab.CurrentDialogueFile.Name, ref _tab.IsOpen)) {
                        _tab.CurrentDocumentPanel.Draw();
                        ImGui.EndTabItem();
                    }
                }

                // Init Tabs
                for (int i = _tabsToInit.Count - 1; i >= 0; i--) {
                    WorkspaceTab _tab = _tabsToInit[i];
                    _tab.Init();
                    _tabsToInit.Remove(_tab);
                }

                // Remove Tabs
                for (int i = _tabsToRemove.Count - 1; i >= 0; i--) {
                    WorkspaceTab _tab = _tabsToRemove[i];
                    _tabsOpened.Remove(_tab.CurrentDialogueFile.Name);
                    _tabsToRemove.Remove(_tab);
                }

                ImGui.EndTabBar();
            }
            ImGui.End();
        }

        public static void OpenFile(string _file_path) {
            if (!File.Exists(_file_path))
                throw new FileNotFoundException("File not found.", _file_path);

            DialogueFile file = null;

            string content = File.ReadAllText(_file_path);
            using (JsonDocument doc = JsonDocument.Parse(content)) {
                JsonElement root = doc.RootElement;

                string name = root.TryGetProperty("name", out var propName) ? propName.GetString() : "Nill";
                string type = root.TryGetProperty("type", out var propType) ? propType.GetString() : "Nill";

                file = new DialogueFile {
                    FilePath = _file_path,
                    Name = name,
                    Type = type,
                    IsDirty = false
                };

                AddTab(file);
            }

        }

        // Create new tab and increase on TabsToInit.
        public static void AddTab(DialogueFile _file) {
            WorkspaceTab new_tab = new WorkspaceTab();
            new_tab.CurrentDialogueFile = _file;

            switch (_file.Type) {
                case "AdvancedDialogue":
                    new_tab.CurrentDocumentPanel = new AdvancedDialoguePanel();
                    break;
            }

            _tabsToInit.Add(new_tab);
        }

        // Remove your fucking tab.
        public static void RemoveTab(WorkspaceTab _tab) {
            _tabsToRemove.Add(_tab);
        }
    }
}
