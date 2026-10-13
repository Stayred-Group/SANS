using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using static SANS.Editor.UIWorkspace;

namespace SANS.Systems {
    public static class DialogueFileSerializer {
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        public static void SaveToDisk(DialogueFile file) {
            var root = new JsonObject {
                ["name"] = file.Name,
                ["type"] = file.Type,
                ["graph"] = JsonNode.Parse(file.Content ?? "{}")
            };

            File.WriteAllText(file.FilePath, root.ToJsonString(Options));
            file.IsDirty = false;
        }

        public static DialogueFile LoadFile(string path) {
            var root = JsonNode.Parse(File.ReadAllText(path));

            return new DialogueFile {
                FilePath = path,
                Name = root["name"]?.GetValue<string>(),
                Type = root["type"]?.GetValue<string>(),
                Content = root["graph"]?.ToJsonString(),
                IsDirty = false
            };
        }
    }
}
