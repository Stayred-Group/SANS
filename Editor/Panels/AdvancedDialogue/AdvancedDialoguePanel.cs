using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using SANS.Editor.Panels.AdvancedDialogue.Nodes;


namespace SANS.Editor.Panels.AdvancedDialogue {
    public class AdvancedDialoguePanel : BaseDocumentPanel {
        private readonly List<GraphBaseNode> _nodes = new();
        private readonly List<GraphLink> _links = new();
        private readonly Dictionary<int, GraphBaseNode> _nodesID = new();

        string m_NodeGraphCanvaTitle = "";
        string m_InspectorTitle = "";

        public override void Init() {
            m_NodeGraphCanvaTitle = $"Node Graph###NodeGraph_{m_DocumentFilePath}";
            m_InspectorTitle = $"Inspector###Inspector_{m_DocumentFilePath}";

            var io = ImNodes.GetIO();

            AddNode(new GraphStartNode(), new Vector2(0, 0));
            AddNode(new GraphStartNode(), new Vector2(10, 0));
            AddNode(new GraphStartNode(), new Vector2(-10, 0));
            AddNode(new GraphDialogueNode(), new Vector2(15, -12));

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
                    ImGui.Text(node.NodeName);
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
                ImGui.Text("TEst test ets");
                ImGui.End();
            }

            int from = 0, to = 0;
            if (ImNodes.IsLinkCreated(ref from, ref to)) {
                _links.Add(new GraphLink(from, to));
            }
        }

        public override void Shutdown() {  
        }

        public override void Save() { 
        }

        public override void load() {
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