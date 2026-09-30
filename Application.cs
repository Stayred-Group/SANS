using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using SANS;
using SANS.Editor;
using Stayred.Tools.SToolKit;
using System.Numerics;

namespace Stayred.Tools.SANS {
    public class SansApp : App {

        public override void OnInit() {
        }

        public override void OnUpdate() {
            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.MenuBar
                                             | ImGuiWindowFlags.NoTitleBar
                                             | ImGuiWindowFlags.NoCollapse
                                             | ImGuiWindowFlags.NoResize
                                             | ImGuiWindowFlags.NoMove
                                             | ImGuiWindowFlags.NoBringToFrontOnFocus
                                             | ImGuiWindowFlags.NoNavFocus
                                             | ImGuiWindowFlags.NoDecoration
                                             | ImGuiWindowFlags.NoBackground;

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize);

            if (ImGui.Begin(("SANS"), windowFlags)) {
                EWorkspace.UpdateWorkspace();
                ProjectManager.Update();
                ImGui.End();
            }
        }

        public override void OnShutdown() {
        }
    }

    public static class Application {
        static public String CurrentApplicationVersion = "0.0.1";

        public static void Main() {
            // Create config 
            AppConfig _config = new AppConfig();
            _config.AppName = ("SANS - " + CurrentApplicationVersion);
            _config.AppVersion = CurrentApplicationVersion;
            _config.ActiveImNode = true;

            // Create app
            var app = new SansApp();
            app.Run(_config);
        }
    }
}