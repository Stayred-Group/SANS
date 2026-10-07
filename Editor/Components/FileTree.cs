using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SANS.Editor.Components {
    public class AssetFileData {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
    }

    public static class FileTree {
        const string Extension = ".sans";

        static readonly JsonSerializerOptions JsonOpts = new() {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        static readonly (string Label, string Type, string DefaultName)[] FileTypes = {
            ("Advanced Dialogue", "AdvancedDialogue", "new_advanced_dialogue"),
            ("Ambient Dialogue",  "AmbientDialogue",  "new_ambient_dialogue"),
            ("Bark Dialogue",     "BarkDialogue",     "new_bark_dialogue"),
        };

        /// ///////////////////////////////////////////
        // Signals TODO: (UIWorkspace can subscribe to close/remap open panels)
        /// ///////////////////////////////////////////
        static public event Action<string, string>? OnPathRenamed;
        static public event Action<string>? OnPathDeleted;

        /// ///////////////////////////////////////////
        // State
        /// ///////////////////////////////////////////
        static private string _rootPath = "";
        static private string selectedPath = "";

        // Tree Cache
        class FsNode {
            public string Path = "";
            public string Name = "";
            public bool IsDir;
            public List<FsNode> Children = new();
        }

        static private FsNode? _tree;
        static private FileSystemWatcher? _watcher;
        static private volatile bool _needsRefresh;

        // Popups
        static private string _pendingPopup = "";
        static private string _error = "";

        static private string _targetPath = "";
        static private string _targetDir = "";

        static private string _fileCreatorType = "";
        static private string _fileCreatorNewName = "";
        static private string _folderNewName = "";
        static private string _renameNewName = "";

        /// ///////////////////////////////////////////
        // Root / Refresh
        /// ///////////////////////////////////////////
        static private void SetRoot(string path) {
            _rootPath = path;
            selectedPath = path;

            _watcher?.Dispose();
            _watcher = null;

            if (Directory.Exists(path)) {
                _watcher = new FileSystemWatcher(path) {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    EnableRaisingEvents = true
                };
                // Events come from another thread: they only raise the flag
                _watcher.Created += (_, _) => _needsRefresh = true;
                _watcher.Deleted += (_, _) => _needsRefresh = true;
                _watcher.Renamed += (_, _) => _needsRefresh = true;
            }

            _needsRefresh = true;
        }

        static public void Refresh() => _needsRefresh = true;

        static private FsNode Scan(string path) {
            var node = new FsNode { Path = path, Name = Path.GetFileName(path), IsDir = true };

            try {
                foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d, StringComparer.OrdinalIgnoreCase)) {
                    if (Path.GetFileName(dir).StartsWith('.')) continue;   // ignore .git etc.
                    node.Children.Add(Scan(dir));
                }

                foreach (var file in Directory.GetFiles(path).OrderBy(f => f, StringComparer.OrdinalIgnoreCase)) {
                    // Only shows .sans (the project configuration JSON remains hidden)
                    if (!file.EndsWith(Extension, StringComparison.OrdinalIgnoreCase)) continue;
                    node.Children.Add(new FsNode { Path = file, Name = Path.GetFileName(file) });
                }
            } catch (Exception e) {
                Console.WriteLine($"[FileTree] Fail to read '{path}': {e.Message}");
            }

            return node;
        }

        /// ///////////////////////////////////////////
        // Draw FileTree
        /// ///////////////////////////////////////////
        static public void DrawFileTree(string rootPath) {
            if (rootPath != _rootPath) SetRoot(rootPath);

            if (_needsRefresh) {
                _needsRefresh = false;
                _tree = Directory.Exists(_rootPath) ? Scan(_rootPath) : null;
            }

            if (ImGui.Begin("File Tree")) {
                if (_tree != null) {
                    foreach (var child in _tree.Children) DrawNode(child);
                }

                if (ImGui.BeginPopupContextWindow("RootContextMenu",
                        ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems)) {
                    if (ImGui.IsWindowAppearing()) selectedPath = _rootPath;
                    DrawContextMenu(_rootPath);
                    ImGui.EndPopup();
                }

                DrawCreatorsPopup();
            }
            ImGui.End();
        }

        /// ///////////////////////////////////////////
        // Draw Node
        /// ///////////////////////////////////////////
        static private void DrawNode(FsNode node) {
            if (node.IsDir) {
                var flags = ImGuiTreeNodeFlags.OpenOnArrow
                          | ImGuiTreeNodeFlags.OpenOnDoubleClick
                          | ImGuiTreeNodeFlags.SpanFullWidth;
                if (node.Path == selectedPath) flags |= ImGuiTreeNodeFlags.Selected;

                bool open = ImGui.TreeNodeEx($"{node.Name}###{node.Path}", flags);

                HandleSelection(node.Path);
                DrawItemContext(node.Path);

                if (open) {
                    foreach (var child in node.Children) DrawNode(child);
                    ImGui.TreePop();
                }
            } 
            else {
                var flags = ImGuiTreeNodeFlags.Leaf
                          | ImGuiTreeNodeFlags.NoTreePushOnOpen
                          | ImGuiTreeNodeFlags.SpanFullWidth;
                if (node.Path == selectedPath) flags |= ImGuiTreeNodeFlags.Selected;

                ImGui.TreeNodeEx($"{node.Name}###{node.Path}", flags);

                HandleSelection(node.Path);

                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) {
                    selectedPath = node.Path;
                    UIWorkspace.OpenFile(node.Path);
                }

                DrawItemContext(node.Path);
            }
        }

        static private void HandleSelection(string path) {
            if ((ImGui.IsItemClicked(ImGuiMouseButton.Left) && !ImGui.IsItemToggledOpen())
                || ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                selectedPath = path;
            }
        }

        static private void DrawItemContext(string path) {
            if (ImGui.BeginPopupContextItem($"ContextMenu_{path}")) {
                DrawContextMenu(path);
                ImGui.EndPopup();
            }
        }

        /// ///////////////////////////////////////////
        // Draw Context Menu
        /// ///////////////////////////////////////////
        static private void DrawContextMenu(string path) {
            bool isRoot = path == _rootPath;
            bool isDir = Directory.Exists(path);

            if (!isDir) {
                if (ImGui.MenuItem("Open")) UIWorkspace.OpenFile(path);
                ImGui.Separator();
            } else {
                if (ImGui.BeginMenu("New File")) {
                    foreach (var t in FileTypes) {
                        if (ImGui.MenuItem(t.Label)) {
                            _fileCreatorType = t.Type;
                            _fileCreatorNewName = UniqueName(path, t.DefaultName, Extension);
                            RequestPopup(path, "Create New File");
                        }
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("New Folder")) {
                    _folderNewName = UniqueName(path, "new_folder", "");
                    RequestPopup(path, "Create New Folder");
                }

                ImGui.Separator();
            }

            // The project root cannot be renamed or deleted
            ImGui.BeginDisabled(isRoot);

            if (ImGui.MenuItem("Rename")) {
                _renameNewName = isDir ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
                RequestPopup(path, "Rename");
            }

            if (ImGui.MenuItem("Delete")) {
                RequestPopup(path, "Delete");
            }

            ImGui.EndDisabled();

            ImGui.Separator();

            if (ImGui.MenuItem("Show in Explorer")) ShowInExplorer(path);
        }

        static private void RequestPopup(string path, string popup) {
            _targetPath = path;
            _targetDir = Directory.Exists(path) ? path : (Path.GetDirectoryName(path) ?? _rootPath);
            _error = "";
            _pendingPopup = popup;
        }

        /// ///////////////////////////////////////////
        // Helpers
        /// ///////////////////////////////////////////
        // suggest "name", "name_1", "name_2"... until find a free name
        static private string UniqueName(string dir, string baseName, string ext) {
            string name = baseName;
            int i = 1;
            while (File.Exists(Path.Combine(dir, name + ext)) || Directory.Exists(Path.Combine(dir, name))) {
                name = $"{baseName}_{i++}";
            }
            return name;
        }

        static private string? ValidateName(string name, string dir, string ext, string? ignorePath = null) {
            if (string.IsNullOrWhiteSpace(name)) return "The file name cannot be empty.";
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return "The name contains invalid characters.";

            string full = Path.Combine(dir, name + ext);
            bool exists = File.Exists(full) || Directory.Exists(full);
            bool isSelf = ignorePath != null &&
                          string.Equals(Path.GetFullPath(full), Path.GetFullPath(ignorePath),
                                        StringComparison.OrdinalIgnoreCase);

            return exists && !isSelf ? "An file with this name already exists." : null;
        }

        static private void DrawError(string? validation) {
            string msg = validation ?? _error;
            if (msg != "") ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f), msg);
        }

        static private void ShowInExplorer(string path) {
            try {
                if (OperatingSystem.IsWindows()) {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                        "explorer.exe", Directory.Exists(path) ? $"\"{path}\"" : $"/select,\"{path}\"") {
                        UseShellExecute = true
                    });
                }
            } catch (Exception e) {
                Console.WriteLine($"[FileTree] {e.Message}");
            }
        }

        // The "x" bool is local and always start with true
        static private bool BeginModal(string name) {
            bool open = true;
            return ImGui.BeginPopupModal(name, ref open, ImGuiWindowFlags.AlwaysAutoResize);
        }

        /// ///////////////////////////////////////////
        // Draw Creators Popup
        /// ///////////////////////////////////////////
        static private void DrawCreatorsPopup() {
            if (!string.IsNullOrEmpty(_pendingPopup)) {
                ImGui.OpenPopup(_pendingPopup);
                _pendingPopup = "";
            }

            DrawCreateFilePopup();
            DrawCreateFolderPopup();
            DrawRenamePopup();
            DrawDeletePopup();
        }

        // ---------- New File ----------
        static private void DrawCreateFilePopup() {
            if (!BeginModal("Create New File"))
                return;

            ImGui.Text(_fileCreatorType);

            if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere();
            bool enter = ImGui.InputText("File Name", ref _fileCreatorNewName, 100,
                                         ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.SameLine();
            ImGui.TextDisabled(Extension);

            string? validation = ValidateName(_fileCreatorNewName, _targetDir, Extension);
            DrawError(validation);

            ImGui.BeginDisabled(validation != null);
            if (ImGui.Button("Create") || (enter && validation == null)) {
                try {
                    string path = Path.Combine(_targetDir, _fileCreatorNewName + Extension);

                    var data = new AssetFileData { Name = _fileCreatorNewName, Type = _fileCreatorType };
                    File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOpts));

                    selectedPath = path;
                    _needsRefresh = true;
                    UIWorkspace.OpenFile(path);
                    ImGui.CloseCurrentPopup();
                } catch (Exception e) {
                    _error = e.Message;
                }
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }

        // ---------- New Folder ----------
        static private void DrawCreateFolderPopup() {
            if (!BeginModal("Create New Folder"))
                return;

            if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere();
            bool enter = ImGui.InputText("Folder Name", ref _folderNewName, 100,
                                         ImGuiInputTextFlags.EnterReturnsTrue);

            string? validation = ValidateName(_folderNewName, _targetDir, "");
            DrawError(validation);

            ImGui.BeginDisabled(validation != null);
            if (ImGui.Button("Create") || (enter && validation == null)) {
                try {
                    string path = Path.Combine(_targetDir, _folderNewName);
                    Directory.CreateDirectory(path);

                    selectedPath = path;
                    _needsRefresh = true;
                    ImGui.CloseCurrentPopup();
                } catch (Exception e) {
                    _error = e.Message;
                }
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }

        // ---------- Rename ----------
        static private void DrawRenamePopup() {
            if (!BeginModal("Rename"))
                return;

            bool isDir = Directory.Exists(_targetPath);
            string parent = Path.GetDirectoryName(_targetPath) ?? "";
            string ext = isDir ? "" : Path.GetExtension(_targetPath); // keeps ".sans"

            if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere();
            bool enter = ImGui.InputText("New Name", ref _renameNewName, 100,
                                         ImGuiInputTextFlags.EnterReturnsTrue);
            if (!isDir) {
                ImGui.SameLine();
                ImGui.TextDisabled(ext);
            }

            string? validation = ValidateName(_renameNewName, parent, ext, ignorePath: _targetPath);
            DrawError(validation);

            ImGui.BeginDisabled(validation != null);
            if (ImGui.Button("Rename") || (enter && validation == null)) {
                try {
                    string newPath = Path.Combine(parent, _renameNewName + ext);

                    if (!string.Equals(newPath, _targetPath, StringComparison.Ordinal)) {
                        if (isDir) {
                            Directory.Move(_targetPath, newPath);
                        } else {
                            File.Move(_targetPath, newPath);
                            UpdateNameInsideFile(newPath, _renameNewName);   // JSON "name" in sync
                        }

                        selectedPath = RemapPath(selectedPath, _targetPath, newPath);

                        _needsRefresh = true;
                        OnPathRenamed?.Invoke(_targetPath, newPath);
                    }
                    ImGui.CloseCurrentPopup();
                } catch (Exception e) {
                    _error = e.Message;
                }
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }

        // Updates only the "name" field, preserving the rest of the JSON
        static private void UpdateNameInsideFile(string path, string newName) {
            try {
                var node = JsonNode.Parse(File.ReadAllText(path));
                if (node is JsonObject obj) {
                    obj["name"] = newName;
                    File.WriteAllText(path, obj.ToJsonString(JsonOpts));
                }
            } catch {
                // Invalid json
            }
        }

        static private string RemapPath(string path, string oldPath, string newPath) {
            if (string.Equals(path, oldPath, StringComparison.OrdinalIgnoreCase)) return newPath;

            string prefix = oldPath + Path.DirectorySeparatorChar;
            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(newPath, path[prefix.Length..])
                : path;
        }

        // ---------- Delete ----------
        static private void DrawDeletePopup() {
            if (!BeginModal("Delete"))
                return;

            bool isDir = Directory.Exists(_targetPath);
            ImGui.Text($"Delete \"{Path.GetFileName(_targetPath)}\"?");

            if (isDir) {
                ImGui.TextColored(new Vector4(1f, 0.8f, 0.3f, 1f),
                                  "The folder and everything inside it will be deleted.");
            }
            DrawError(null);

            if (ImGui.Button("Delete")) {
                try {
                    DeleteToTrash(_targetPath, isDir);

                    string prefix = _targetPath + Path.DirectorySeparatorChar;
                    if (selectedPath == _targetPath ||
                        selectedPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                        selectedPath = _rootPath;
                    }

                    _needsRefresh = true;
                    OnPathDeleted?.Invoke(_targetPath);
                    ImGui.CloseCurrentPopup();
                } catch (Exception e) {
                    _error = e.Message;
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }

        // Recycle Bin on Windows; on other systems, it deletes immediately
        static private void DeleteToTrash(string path, bool isDir) {
            if (OperatingSystem.IsWindows()) {
                if (isDir)
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path,
                        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                        Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                else
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path,
                        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                        Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            } else {
                if (isDir) Directory.Delete(path, true);
                else File.Delete(path);
            }
        }
    }
}