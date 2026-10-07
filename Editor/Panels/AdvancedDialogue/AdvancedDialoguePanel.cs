using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using SANS.Editor.Panels.AdvancedDialogue.Nodes;


namespace SANS.Editor.Panels.AdvancedDialogue {
    public class AdvancedDialoguePanel : BaseDocumentPanel {
        private readonly List<GraphBaseNode> _nodes = new();
        private readonly List<GraphLink> _links = new();

        public override void Init() {
            var io = ImNodes.GetIO();

            AddNode(new GraphStartNode(), new Vector2(0, 0));
            AddNode(new GraphStartNode(), new Vector2(10, 0));
            AddNode(new GraphStartNode(), new Vector2(-10, 0));
            AddNode(new GraphStartNode(), new Vector2(15, -12));
        }

        public override void Draw() {
            var io = ImNodes.GetIO();

            ImNodes.BeginNodeEditor();

            foreach (var node in _nodes) node.Render();
            foreach (var link in _links) ImNodes.Link(link.Id, link.FromId, link.ToId);

            ImNodes.MiniMap(.1f);
            ImNodes.EndNodeEditor();

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
            node.Init();
            ImNodes.SetNodeGridSpacePos(node.NodeID, pos);
            return node;
        }
    }
}