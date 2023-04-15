using ImGuiNET;
using System;
using System.IO;
using VfxEditor.Ui.Nodes;
using VfxEditor.Utils;

namespace VfxEditor.AvfxFormat {
    public abstract class UiNodeSplitView<T> : AvfxGenericSplitView<T>, IUiNodeView<T> where T : AvfxNode {
        public readonly AvfxFile File;
        public readonly NodeGroup<T> Group;

        public readonly string Id;
        public readonly string DefaultText;
        public readonly string DefaultPath;

        public UiNodeSplitView( AvfxFile file, NodeGroup<T> group, string name, bool allowNew, bool allowDelete, string defaultPath ) : base( group.Items, allowNew, allowDelete ) {
            File = file;
            Group = group;

            Id = $"##{name}";
            DefaultText = $"Select {UiUtils.GetArticle( name )} {name}";
            DefaultPath = Path.Combine( Plugin.RootLocation, "Files", defaultPath );
        }

        public abstract void OnSelect( T item );

        public abstract T Read( BinaryReader reader, int size );

        protected override void DrawControls( string id ) => IUiNodeView<T>.DrawControls( this, File, id );

        protected override bool DrawLeftItem( T item, int idx, string id ) {
            if( ImGui.Selectable( $"{item.GetText()}{Id}{idx}", Selected == item ) ) {
                Selected = item;
                OnSelect( item );
            }
            return false;
        }

        protected override void DrawSelected( string id ) => Selected.Draw( Id );

        public void ResetSelected() { Selected = null; }

        public NodeGroup<T> GetGroup() => Group;

        public string GetDefaultPath() => DefaultPath;

        public T GetSelected() => Selected;

        public bool IsAllowedNew() => AllowNew;

        public bool IsAllowedDelete() => AllowDelete;

        public void SetSelected( T selected ) {
            Selected = selected;
            OnSelect( selected );
        }
    }
}
