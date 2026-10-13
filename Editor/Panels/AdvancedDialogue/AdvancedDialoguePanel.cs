using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using SANS.Editor.Panels.AdvancedDialogue.Nodes;
using SANS.Systems;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text.Json;


namespace SANS.Editor.Panels.AdvancedDialogue {
    public class AdvancedDialoguePanel : BaseDocumentPanel {
        private readonly List<GraphBaseNode> _nodes = new();
        private readonly List<GraphLink> _links = new();
        private readonly Dictionary<int, GraphBaseNode> _nodesID = new();

        private readonly Dictionary<string, System.Func<GraphBaseNode>> _factories = new() {
            ["Start Node"] = () => new GraphStartNode(),
            ["Dialogue Node"] = () => new GraphDialogueNode(),
        };

        string m_NodeGraphCanvaTitle = "";
        string m_InspectorTitle = "";

        public override void Init() {
            m_NodeGraphCanvaTitle = $"Node Graph###NodeGraph_{m_DocumentFilePath}";
            m_InspectorTitle = $"Inspector###Inspector_{m_DocumentFilePath}";

            var io = ImNodes.GetIO();

            //AddNode(new GraphStartNode(), new Vector2(-10, 0));
            //AddNode(new GraphDialogueNode(), new Vector2(15, -12));

            ImGuiP.DockBuilderRemoveNode(m_DockspaceID);
            ImGuiP.DockBuilderAddNode(m_DockspaceID);

            Vector2 mainViewportSize = ImGui.GetMainViewport().Size;
            if (mainViewportSize.X <= 0 || mainViewportSize.Y <= 0) {
                mainViewportSize = new Vector2(1280, 720);
            }

            ImGuiP.DockBuilderSetNodeSize(m_DockspaceID, mainViewportSize);

            uint mainNode = 0;
            uint RightNode = 0;
            unsafe {
                ImGuiP.DockBuilderSplitNode(m_DockspaceID, ImGuiDir.Right, 0.30f, &RightNode, &mainNode);
            }

            ImGuiP.DockBuilderDockWindow(m_NodeGraphCanvaTitle, mainNode);
            ImGuiP.DockBuilderDockWindow(m_InspectorTitle, RightNode);

            ImGuiP.DockBuilderFinish(m_DockspaceID);
        }

        public override void Draw() {
            // <-------------------->
            // Node Graph
            // <-------------------->
            if (ImGui.Begin(m_NodeGraphCanvaTitle)) {
                ImGui.Columns(2, "NodeGraphContent", true);

                // Node Selector Content
                var selected = GetSelectedNodes();
                if (selected.Count == 1) {
                    GraphBaseNode node = selected[0];
                    ImGui.Text(node.NodeName + " - " + node.NodeID);
                    node.DrawBody();
                }

                ImGui.NextColumn();

                ImNodes.BeginNodeEditor();

                foreach (var node in _nodes) node.Render();
                foreach (var link in _links) ImNodes.Link(link.Id, link.FromId, link.ToId);

                ImNodes.MiniMap(.1f);
                ImNodes.EndNodeEditor();

                ImGui.End();
            }

            // <-------------------->
            // Inspector
            // <-------------------->
            if (ImGui.Begin(m_InspectorTitle)) {
                ImGui.End();
            }

            int from = 0, to = 0;
            if (ImNodes.IsLinkCreated(ref from, ref to)) {
                _links.Add(new GraphLink(from, to));
            }
        }

        public override void Shutdown() {
        }

        /// Save Content
        public override string Save() {
            // Save nodes
            var nodes = new Dictionary<int, object>();
            foreach (GraphBaseNode node in _nodes) {
                var pos = ImNodes.GetNodeGridSpacePos(node.NodeID);
                nodes[node.NodeID] = new Dictionary<string, object> {
                    ["type"] = node.NodeName,
                    ["offset"] = new[] { pos.X, pos.Y },
                    ["data"] = node.SaveData()
                };
            }

            // Save connections
            var connections = new List<object>();
            foreach (GraphLink link in _links) {
                var from = _nodesID[link.FromId - 1];
                var to = _nodesID[link.ToId - 1];

                connections.Add(new Dictionary<string, object> {
                    ["from_node"] = from.NodeID,
                    ["from_port"] = from.Outputs.ToList().FindIndex(o => o.Id == link.FromId),
                    ["to_node"] = to.NodeID,
                    ["to_port"] = to.Inputs.ToList().FindIndex(i => i.Id == link.ToId)
                });
            }

            var data = new Dictionary<string, object> {
                ["nodes"] = nodes,
                ["connections"] = connections
            };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"[SANS::INFO] {m_DocumentFilePath} saved!");
            return json;
        }

        /// Load Dialogue File
        public override void Load(UIWorkspace.DialogueFile _file) {
            Clear();

            if (string.IsNullOrWhiteSpace(_file.Content)) { 
                return; 
            }

            using JsonDocument doc = JsonDocument.Parse(_file.Content);
            JsonElement root = doc.RootElement;
            var loaded = new Dictionary<int, GraphBaseNode>(); // Saved ID -> the new node created in this run (local, discarded after Load)

            // Load nodes
            foreach (JsonProperty entry in root.GetProperty("nodes").EnumerateObject()) {
                JsonElement n = entry.Value;
                string type = n.GetProperty("type").GetString();

                if (!_factories.TryGetValue(type, out var create)) { 
                    continue;
                }

                GraphBaseNode node = create(); // gets a fresh NodeId, that's fine
                node.LoadData(n.GetProperty("data").Deserialize<Dictionary<string, JsonElement>>()); // Load node data
                float[] offset = n.GetProperty("offset").Deserialize<float[]>(); // Get node offset
                AddNode(node, new Vector2(offset[0], offset[1]));
                loaded[int.Parse(entry.Name)] = node;
            }

            // Set connections
            foreach (JsonElement c in root.GetProperty("connections").EnumerateArray()) {
                if (!loaded.TryGetValue(c.GetProperty("from_node").GetInt32(), out var from)) continue;
                if (!loaded.TryGetValue(c.GetProperty("to_node").GetInt32(), out var to)) continue;

                int fromPort = c.GetProperty("from_port").GetInt32();
                int toPort = c.GetProperty("to_port").GetInt32();
                if (fromPort >= from.Outputs.Count || toPort >= to.Inputs.Count) continue;

                _links.Add(new GraphLink(from.Outputs[fromPort].Id, to.Inputs[toPort].Id));
            }
        }

        /// Clear Data
        private void Clear() {
            _nodes.Clear();
            _links.Clear();
            _nodesID.Clear();
        }

        ///////////////////////////////////////////////
        /// CALLS
        ///////////////////////////////////////////////

        public T AddNode<T>(T node, Vector2 pos) where T : GraphBaseNode {
            _nodes.Add(node);
            _nodesID[node.NodeID] = node;

            node.Init();
            ImNodes.SetNodeGridSpacePos(node.NodeID, pos);
            return node;
        }

        public List<GraphBaseNode> GetSelectedNodes() {
            var result = new List<GraphBaseNode>();
            int count = ImNodes.NumSelectedNodes();
            if (count <= 0) return result;

            int[] ids = new int[count];
            ImNodes.GetSelectedNodes(ref ids[0]);

            foreach (int id in ids)
                if (_nodesID.TryGetValue(id, out var node))
                    result.Add(node);

            return result;
        }
    }
}