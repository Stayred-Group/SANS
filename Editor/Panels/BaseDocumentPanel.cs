using System;
using System.Collections.Generic;
using System.Text;

namespace SANS.Editor.Panels {
    public abstract class BaseDocumentPanel {
        public virtual void Draw() { }
        public virtual void Save() { }
        public virtual void load() { }
    }
}
