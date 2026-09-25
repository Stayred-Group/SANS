using System;
using System.IO;
using System.Windows.Forms;

namespace SANS {
    public static class FileManager {

        public static class FileDialog {
            public static string SelectFolder(string initial_path = "") {
                string selected_path = string.Empty;

                Thread thread = new Thread(() => {
                    using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                        dialog.Description = "Select the project folder.";
                        dialog.UseDescriptionForTitle = true;
                        if (!string.IsNullOrEmpty(selected_path) && Directory.Exists(selected_path)) {
                            dialog.InitialDirectory = selected_path;
                        }

                        if (dialog.ShowDialog() == DialogResult.OK) {
                            selected_path = dialog.SelectedPath;
                        }
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                return selected_path;
            }
        }
    }
}
