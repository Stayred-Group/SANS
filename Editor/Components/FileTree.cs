using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace SANS.Editor.Components {
    public static class FileTree {
        static string selectedPath = "";

        static public void DrawFileTree(string _initial_path) {
            ImGui.Begin("File Tree");

            // Draw folders
            foreach (var dir in Directory.GetDirectories(_initial_path).OrderBy(d => d)) {
                var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth;
                if (dir == selectedPath) flags |= ImGuiTreeNodeFlags.Selected;

                bool open = ImGui.TreeNodeEx(dir, flags, Path.GetFileName(dir));

                if (ImGui.IsItemClicked()) {
                    selectedPath = dir;
                }

                //DrawContextMenu(dir, isFolder: true);

                if (open) {
                    DrawFileTree(dir); ImGui.TreePop();
                }
            }

            // Draw files
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
    }
}
