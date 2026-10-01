using Hexa.NET.ImGui;

namespace SANS.Editor.Components {
    public static class EMenuBar {
        static public void Update() {
            ImGui.BeginMainMenuBar();

            if (ImGui.BeginMenu("Project")) {

                // <---- If exists project loaded, allow new options ---->
                if (ProjectManager.GetCurrentLoadedProject() != null) {
                    if (ImGui.MenuItem("Config Project")) {
                    }

                    if (ImGui.MenuItem("Save Project")) {
                        ProjectManager.SaveCurrentProject();
                    }

                    ImGui.Separator();
                }
                
                // <---- Create Project ---->
                if (ImGui.MenuItem("Create Project")) {
                    UIEditor.m_EnableProjectCreatorPainel = true;
                }

                // <---- Load Project ---->
                if (ImGui.MenuItem("Load Project")) {
                    ProjectManager.CreateOrLoadProject(FileManager.FileDialog.SelectProjectFile());
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("File")) {
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit")) {
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
