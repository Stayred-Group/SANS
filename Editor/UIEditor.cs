using Hexa.NET.ImGui;
using SANS.Editor.Components;

namespace SANS.Editor {
    public static class UIEditor {
        public static bool m_EnableProjectCreatorPainel = false;

        public static void DrawEditor() {
            // Header
            EMenuBar.Update();

            // Content
            if (ProjectManager.GetCurrentLoadedProject() != null) {
                // <----- File Tree ----->
                FileTree.DrawFileTree(ProjectManager.GetCurrentLoadedProject().SProjectFolder);

                // <----- Workspace ----->
                UIWorkspace.Draw();
            }


            // Others and Popups
            if (m_EnableProjectCreatorPainel) { ProjectManager.ProjectCreatorPainel(); }
        }
    }
}
