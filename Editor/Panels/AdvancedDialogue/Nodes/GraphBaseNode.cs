using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using System.Text.Json;

namespace SANS.Editor.Panels.AdvancedDialogue.Nodes {
    public static class GraphIDGenerator {
        private static int _next = 1;
        public static int Next() => _next++;
    }

    public readonly struct GraphID {
        public readonly int Id;
        public readonly string Label;
        public GraphID(int id, string label) { Id = id; Label = label; }
    }

    public class GraphLink {
        public int Id { get; } = GraphIDGenerator.Next();
        public int FromId { get; }   // output ID
        public int ToId { get; }     // input ID
        public GraphLink(int fromId, int toId) { FromId = fromId; ToId = toId; }
    }

    public abstract class GraphBaseNode {
        public int NodeID { get; } = GraphIDGenerator.Next();
        public string NodeName { get; set; }
        public string NodeUniqueID { get; set; }

        private readonly List<GraphID> _inputs = new();
        private readonly List<GraphID> _outputs = new();
        public IReadOnlyList<GraphID> Inputs => _inputs;
        public IReadOnlyList<GraphID> Outputs => _outputs;

        protected int AddInput(string label) {
            int id = GraphIDGenerator.Next();
            _inputs.Add(new GraphID(id, label));
            return id;
        }

        protected int AddOutput(string label) {
            int id = GraphIDGenerator.Next();
            _outputs.Add(new GraphID(id, label));
            return id;
        }

        public virtual void Init() { }
        public virtual void DrawBody() { }
        public virtual void Shutdown() { }

        public virtual Dictionary<string, object> SaveData() => new();
        public virtual void LoadData(Dictionary<string, JsonElement> data) { }

        public void Render() {
            ImNodes.BeginNode(NodeID);

            ImNodes.BeginNodeTitleBar();
            ImGui.TextUnformatted(NodeName);
            ImNodes.EndNodeTitleBar();

            foreach (var input in _inputs) {
                ImNodes.BeginInputAttribute(input.Id);
                ImGui.TextUnformatted(input.Label);
                ImNodes.EndInputAttribute();
            }

            DrawBody();

            foreach (var output in _outputs) {
                ImNodes.BeginOutputAttribute(output.Id);
                ImGui.TextUnformatted(output.Label);
                ImNodes.EndOutputAttribute();
            }

            ImNodes.EndNode();
        }
    }
}