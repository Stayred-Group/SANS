using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace SANS.Editor {
    public static class FileSystem {
        static string selectedPath = "";

        static public void DrawFileSystem(string _initial_path) {
            ImGui.Begin("File System");

            foreach (var dir in Directory.GetDirectories(_initial_path).OrderBy(d => d)) {
                var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth;
                if (dir == selectedPath) flags |= ImGuiTreeNodeFlags.Selected;

                bool open = ImGui.TreeNodeEx(dir, flags, Path.GetFileName(dir));

                if (ImGui.IsItemClicked()) {
                    selectedPath = dir;
                }

                //DrawContextMenu(dir, isFolder: true);

                if (open) {
                    DrawFileSystem(dir); ImGui.TreePop();
                }
            }




            ImGui.End();
        }
    }
}
