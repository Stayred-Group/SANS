using Hexa.NET.ImGui;
using System;
using System.IO;
using System.Windows.Forms;

namespace SANS {
    public static class FileManager {

        public static class FileDialog {
            public static string SelectFolder(string initial_path = "") {
                string selected_path = string.Empty;

                Thread thread = new Thread(() => {
                    using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                        dialog.Description = "Select the project folder.";
                        dialog.UseDescriptionForTitle = true;
                        if (!string.IsNullOrEmpty(selected_path) && Directory.Exists(selected_path)) {
                            dialog.InitialDirectory = selected_path;
                        }

                        if (dialog.ShowDialog() == DialogResult.OK) {
                            selected_path = dialog.SelectedPath;
                        }
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                return selected_path;
            }

            public static string SelectProjectFile(string initial_path = "") {
                string selected_file_path = "";

                Thread thread = new Thread(() => {
                    using (OpenFileDialog dialog = new OpenFileDialog()) {
                        dialog.Title = "Select project file";
                        dialog.Filter = "Json Files (*.json)|*.json";
                        dialog.DefaultExt = "json";
                        dialog.Multiselect = false;

                        if (!string.IsNullOrEmpty(initial_path) && Directory.Exists(initial_path)) {
                            dialog.InitialDirectory = initial_path;
                        }

                        if (dialog.ShowDialog() == DialogResult.OK) {
                            selected_file_path = dialog.FileName; // Caminho completo do arquivo selecionado
                        }
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                return selected_file_path;
            }
        }

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
}
