using Hexa.NET.ImGui;
using SANS.Editor;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SANS {
    public static class ProjectManager {
        private static Project CurrentLoadedProject = null;
        private static string m_CurrentUserProjectName = "";
        private static string m_CurrentUserProjectFolder = "";

        public class Project {
            public String SProjectName { get; set; }
            public String SProjectFolder { get; set; }
            public String SProjectFullPath { get; set; }

            public void Save() {
                var json_config = new JsonSerializerOptions {
                    WriteIndented = true
                };

                string data = JsonSerializer.Serialize(this, json_config);
                File.WriteAllText(SProjectFullPath, data);
            }
        }

        public static void Update() { }

        // <---- Create Project Painel ---->
        public static void ProjectCreatorPainel() {
            ImGui.Begin("Project Creator");

            ImGui.InputText("Project Name", ref m_CurrentUserProjectName, 100);
            ImGui.InputText("Project Folder", ref m_CurrentUserProjectFolder, 100);
            ImGui.SameLine();
            if (ImGui.Button("Search")) {
                m_CurrentUserProjectFolder = FileManager.FileDialog.SelectFolder();
            }

            if (ImGui.Button("Create Project") && m_CurrentUserProjectFolder != "") {
                if (!Directory.Exists(m_CurrentUserProjectFolder)) {
                    Directory.CreateDirectory(m_CurrentUserProjectFolder);
                }

                string fullPath = Path.Combine(m_CurrentUserProjectFolder, (m_CurrentUserProjectName + ".json"));

                CurrentLoadedProject = new Project();
                CurrentLoadedProject.SProjectName = m_CurrentUserProjectName;
                CurrentLoadedProject.SProjectFolder = m_CurrentUserProjectFolder;
                CurrentLoadedProject.SProjectFullPath = fullPath;

                CurrentLoadedProject.Save();
                EWorkspace.m_EnableProjectCreatorPainel = false;
            }

            if (ImGui.Button("Cancel")) {
                EWorkspace.m_EnableProjectCreatorPainel = false;
            }

            ImGui.End();
        }

        // <---- Load Project Painel ---->
        public static void ProjectLoadPainel() {

        }

        public static void CreateProject() { }
        public static void DeleteProject() { }

        public static void SaveProject(Project _project) { }
        public static void LoadProject() { }
    }
}
