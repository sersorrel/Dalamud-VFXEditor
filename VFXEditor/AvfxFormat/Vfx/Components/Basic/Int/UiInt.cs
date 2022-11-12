using ImGuiNET;
using VfxEditor.AVFXLib;
using VfxEditor.Data;

namespace VfxEditor.AvfxFormat.Vfx {
    public class UiInt : IUiBase {
        public readonly string Name;
        public readonly AVFXInt Literal;

        public UiInt( string name, AVFXInt literal ) {
            Name = name;
            Literal = literal;
        }

        public void DrawInline( string id ) {
            // Copy/Paste
            if( CopyManager.IsCopying ) {
                CopyManager.Assigned[Name] = Literal.IsAssigned();
                CopyManager.Ints[Name] = Literal.GetValue();
            }
            if( CopyManager.IsPasting ) {
                if( CopyManager.Assigned.TryGetValue( Name, out var a ) ) CopyManager.PasteCommand.Add( new UiAssignableCommand( Literal, a ) );
                if( CopyManager.Ints.TryGetValue( Name, out var l ) ) CopyManager.PasteCommand.Add( new UiIntCommand( Literal, l ) );
            }

            // Unassigned
            if( IUiBase.DrawAddButton( Literal, Name, id ) ) return;

            var value = Literal.GetValue();
            if( ImGui.InputInt( Name + id, ref value ) ) {
                CommandManager.Avfx.Add( new UiIntCommand( Literal, value ) );
            }

            IUiBase.DrawRemoveContextMenu( Literal, Name, id );
        }
    }
}