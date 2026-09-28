using Hexa.NET.ImGui;

namespace SANS.Editor {
    public static class EMenuBar {
        static public void Update() {
            ImGui.BeginMainMenuBar();

            if (ImGui.BeginMenu("Project")) {
                // <---- Create Project ---->
                if (ImGui.MenuItem("Create Project")) {
                    EWorkspace.m_EnableProjectCreatorPainel = true;
                }

                if (ImGui.MenuItem("Load Project")) {
                    EWorkspace.m_EnableProjectLoaderPainel = true;
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
