using Hexa.NET.ImGui;

namespace SANS.Editor {
    public static class EWorkspace {
        public static bool m_EnableProjectCreatorPainel = false;

        public static void UpdateWorkspace() {
            EMenuBar.Update();

            if (m_EnableProjectCreatorPainel) { ProjectManager.ProjectCreatorPainel(); }
        }
    }
}
