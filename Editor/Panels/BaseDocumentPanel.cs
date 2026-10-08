using System;
using System.Collections.Generic;
using System.Text;

namespace SANS.Editor.Panels {
    public abstract class BaseDocumentPanel {
        public uint m_DockspaceID = 0;

        public virtual void Init() { }
        public virtual void Shutdown() { }

        public virtual void Draw() { }
        public virtual void Save() { }
        public virtual void load() { }
    }
}
