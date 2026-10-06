using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace SANS.Editor.Components {
    public static class FileTree {
        static private string selectedPath = "";
        static private string targetContextPath = "";

        // File Creator
        static private bool _closeFileCreatorPopUp = false;
        static private string _fileCreatorNewName = "";
        static private string _fileCreatorType = "";

        static private bool _closeFolderCreatorPopUp = false;

        /// ///////////////////////////////////////////
        // Draw FileTree
        /// ///////////////////////////////////////////
        static public void DrawFileTree(string _initial_path) {
            ImGui.Begin("File Tree");

            /// ///////////////////////////////////////////
            // Draw folders
            /// ///////////////////////////////////////////
            foreach (var dir in Directory.GetDirectories(_initial_path).OrderBy(d => d)) {
                var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth;
                if (dir == selectedPath) flags |= ImGuiTreeNodeFlags.Selected;

                bool open = ImGui.TreeNodeEx(dir, flags, Path.GetFileName(dir));

                if (ImGui.IsItemClicked()) {
                    selectedPath = dir;
                }

                if (ImGui.BeginPopupContextItem($"ContextMenu_{dir}")) {
                    selectedPath = dir;
                    targetContextPath = dir;
                    DrawContextMenu();
                    ImGui.EndPopup();
                }

                if (open) {
                    DrawFileTree(dir); ImGui.TreePop();
                }
            }

            /// ///////////////////////////////////////////
            // Draw files
            /// ///////////////////////////////////////////
            foreach (var file in Directory.GetFiles(_initial_path).OrderBy(f => f)) {
                var flags = ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth;
                if (file == selectedPath) flags |= ImGuiTreeNodeFlags.Selected;

                string fileName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file).TrimStart('.').ToLower();

                ImGui.TreeNodeEx(file, flags, (fileName + "." + extension));

                //if (ImGui.IsItemClicked()) { 
                //    selectedPath = file;
                //}

                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && extension == "sans") {
                    selectedPath = file;
                    UIWorkspace.OpenFile(selectedPath);
                }
            }

            ImGui.End();
        }

        /// ///////////////////////////////////////////
        // Draw Context Menu
        /// ///////////////////////////////////////////
        static private void DrawContextMenu() {
            if (ImGui.BeginMenu("New File")) {
                if (ImGui.MenuItem("Advanced Dialogue")) {
                    _fileCreatorType = "AdvancedDialogue";
                    _fileCreatorNewName = "new_advanced_dialogue.sans";
                    ImGui.OpenPopup("Create New File");
                }

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("New Folder")) {
            }

            if (ImGui.MenuItem("Rename")) {

            }
        }

        /// ///////////////////////////////////////////
        // Draw Creators Popup
        /// ///////////////////////////////////////////
        static public void DrawCreatorsPopup() {
            if (ImGui.BeginPopupModal("Create New File", ref _closeFileCreatorPopUp, ImGuiWindowFlags.AlwaysAutoResize)) {
                ImGui.InputText("File Name: ", ref _fileCreatorNewName, 100);
                Console.WriteLine("Aoba");
                //ImGui.CloseCurrentPopup();

                ImGui.EndPopup();
            }
        }
    }
}
