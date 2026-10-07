using Hexa.NET.ImGui;
using System.Text.Json;

namespace SANS.Editor.Panels.AdvancedDialogue.Nodes {
    public class GraphStartNode : GraphBaseNode {
        public string StartID = "";
        public int OutID { get; }

        public GraphStartNode() {
            NodeName = "Start Node";
            OutID = AddOutput("Out");
        }

        public override void DrawBody() {
            ImGui.SetNextItemWidth(130.0f);
            ImGui.InputText("Start ID", ref StartID, 50, ImGuiInputTextFlags.EnterReturnsTrue);
        }

        public override Dictionary<string, object> SaveData() => new() {
            ["start_id"] = StartID,
        };

        public override void LoadData(Dictionary<string, JsonElement> data) {
            StartID = data["start_id"].GetString();
        }
    }
}
