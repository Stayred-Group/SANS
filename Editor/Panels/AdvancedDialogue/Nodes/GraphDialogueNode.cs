using Hexa.NET.ImGui;
using System.Text.Json;

namespace SANS.Editor.Panels.AdvancedDialogue.Nodes {
    public class GraphDialogueNode : GraphBaseNode {
        private class DialogueOption {
            public string OptionText = "";
            public int OutputID = -1;
        }

        private List<DialogueOption> OptionsList = new();
        private DialogueOption OptionToRemove = null;
        public string DialogueTextMultiline = "";

        public GraphDialogueNode() {
            NodeName = "Dialogue Node";
            AddInput("Input");
        }

        public override void DrawBody() {
            ImGui.InputTextMultiline("Dialogue Text", ref DialogueTextMultiline, 2000);

            foreach (DialogueOption option in OptionsList) {
                ImGui.InputText($"Option Text###option_{option.OutputID}", ref option.OptionText, 200);
                SetOutputLabel(option.OutputID, option.OptionText);
                ImGui.SameLine();
                if (ImGui.Button($"Delete##option_{option.OutputID}")) {
                    OptionToRemove = option;
                }
            }

            if (ImGui.Button("Create a new option")) {
                DialogueOption option = new();
                option.OutputID = AddOutput("");
                OptionsList.Add(option);
            }

            // Options To Remove
            if (OptionToRemove != null) {
                RemoveOutput(OptionToRemove.OutputID);
                OptionsList.Remove(OptionToRemove);
            }
        }

        public override Dictionary<string, object> SaveData() => new() {
        };

        public override void LoadData(Dictionary<string, JsonElement> data) {
        }
    }
}
