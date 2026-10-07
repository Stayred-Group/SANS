using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace SANS.Editor.Panels {
    public class AdvancedDialoguePanel : BaseDocumentPanel {
        public override void Draw() {
            ImGui.Text("Test Advanced Dialogue");
        }

        public override void Save() { 
        }

        public override void load() {
        }
    }
}
