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
        private static string m_CurrentUserProjectFileSelected = "";

        public class Project {
            public String SProjectName { get; set; }
            public String SProjectVersion { get; set; }
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

        public static void Update() {
        }

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
                CurrentLoadedProject.SProjectVersion = "0.0.1";
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

        // <---- Create Project ---->
        public static void CreateOrLoadProject(String _filepath) {
            if (!File.Exists(_filepath)) {
                throw new FileNotFoundException("The specified project file does not exist.", _filepath);
            }
            string content = File.ReadAllText(_filepath);
            CurrentLoadedProject = JsonSerializer.Deserialize<Project>(content);
        }

        // <---- Save Project ---->
        public static void SaveCurrentProject() {
            if (CurrentLoadedProject != null) { 
                CurrentLoadedProject.Save();
                Console.WriteLine("[SANS::INFO] Current project saved");
            }
        }

        // <---- Get CurrentLoadedProject ---->
        public static Project GetCurrentLoadedProject() { return CurrentLoadedProject; }
    }
}
