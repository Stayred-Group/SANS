using Hexa.NET.ImGui;
using SANS.Editor.Components;
using SANS.Editor.Panels;
using System.Text.Json;
using System.Numerics;

namespace SANS.Editor {
    public static class UIWorkspace {
        static private bool _initialized = false;
        static private Dictionary<string, WorkspaceTab> _tabsOpened = new (StringComparer.OrdinalIgnoreCase);
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
                _tabsOpened[CurrentDialogueFile.FilePath] = this;
                CurrentDocumentPanel.Init();
            }

            public void Shutdown() {
                CurrentDocumentPanel.Shutdown();
            }

            public bool IsOpen = false;
            public bool IsDirty = false;
        }

        ///////////////////////////////////////////////
        // CALLS
        ///////////////////////////////////////////////
        
        public static void Init() {
            if (_initialized) return;
            _initialized = true;

            FileTree.OnPathRenamed += HandlePathRenamed;
            FileTree.OnPathDeleted += HandlePathDeleted;
        }

        public static void Draw() {
            ImGui.Begin("Workspace");

            if (ImGui.BeginTabBar("WorkspaceTabBar")) {

                // Render panels
                foreach (WorkspaceTab _tab in _tabsOpened.Values) {
                    if (ImGui.BeginTabItem(_tab.CurrentDialogueFile.Name, ref _tab.IsOpen)) {
                        ImGui.DockSpace(_tab.CurrentDocumentPanel.m_DockspaceID, new Vector2(0.0f, 0.0f), ImGuiDockNodeFlags.None);
                        _tab.CurrentDocumentPanel.Draw();
                        ImGui.EndTabItem();
                    }
                }

                // Init Tabs
                for (int i = _tabsToInit.Count - 1; i >= 0; i--) {
                    WorkspaceTab _tab = _tabsToInit[i];
                    _tab.CurrentDocumentPanel.m_DocumentFilePath = _tab.CurrentDialogueFile.FilePath;
                    _tab.CurrentDocumentPanel.m_DockspaceID = ImGui.GetID($"DockSpace_{_tab.CurrentDialogueFile.FilePath}");
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
                    new_tab.CurrentDocumentPanel = new SANS.Editor.Panels.AdvancedDialogue.AdvancedDialoguePanel();
                    break;
            }

            _tabsToInit.Add(new_tab);
        }

        // Remove your fucking tab.
        public static void RemoveTab(WorkspaceTab _tab) {
            _tabsToRemove.Add(_tab);
        }

        ///////////////////////////////////////////////
        // HELPERS
        ///////////////////////////////////////////////

        // True if 'path' is the target itself or lives inside it (when the target is a folder)
        static private bool IsAffected(string path, string target) =>
            path.Equals(target, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(target + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

        ///////////////////////////////////////////////
        // SIGNALS
        ///////////////////////////////////////////////

        static void HandlePathRenamed(string oldPath, string newPath) {
            foreach (var (path, tab) in _tabsOpened.ToList()) {
                if (!IsAffected(path, oldPath)) continue;

                string newFilePath = path.Equals(oldPath, StringComparison.OrdinalIgnoreCase) ? newPath : Path.Combine(newPath, path[(oldPath.Length + 1)..]);
                var file = tab.CurrentDialogueFile;
                file.FilePath = newFilePath;
                file.Name = Path.GetFileNameWithoutExtension(newFilePath);

                // Remove the older one and append the new one.
                _tabsOpened.Remove(path);
                _tabsOpened[newFilePath] = tab;
            }
        }

        static void HandlePathDeleted(string deletedPath) {
            foreach (var (path, tab) in _tabsOpened.ToList()) {
                if (!IsAffected(path, deletedPath)) continue;

                _tabsOpened.Remove(path);
                if (!_tabsToRemove.Contains(tab)) _tabsToRemove.Add(tab);
            }
        }
    }
}
