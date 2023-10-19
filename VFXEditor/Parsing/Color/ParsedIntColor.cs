using ImGuiNET;
using System;
using System.IO;
using System.Numerics;

namespace VfxEditor.Parsing {
    public class ParsedIntColor : ParsedSimpleBase<Vector4> {
        private bool Editing = false;
        private DateTime LastEditTime = DateTime.Now;
        private Vector4 StateBeforeEdit;

        public ParsedIntColor( string name, Vector4 value ) : this( name ) {
            Value = value;
        }

        public ParsedIntColor( string name ) : base( name ) { }

        public override void Read( BinaryReader reader ) => Read( reader, 0 );

        public override void Read( BinaryReader reader, int size ) {
            Value.X = reader.ReadByte() / 255f;
            Value.Y = reader.ReadByte() / 255f;
            Value.Z = reader.ReadByte() / 255f;
            Value.W = reader.ReadByte() / 255f;
        }

        public override void Write( BinaryWriter writer ) {
            writer.Write( ( byte )( int )( Value.X * 255f ) );
            writer.Write( ( byte )( int )( Value.Y * 255f ) );
            writer.Write( ( byte )( int )( Value.Z * 255f ) );
            writer.Write( ( byte )( int )( Value.W * 255f ) );
        }

        public override void Draw( CommandManager manager ) {
            CopyPaste( manager );

            var prevValue = Value;
            if( ImGui.ColorEdit4( Name, ref Value, ImGuiColorEditFlags.Float | ImGuiColorEditFlags.NoDragDrop ) ) {
                if( !Editing ) {
                    Editing = true;
                    StateBeforeEdit = prevValue;
                }
                LastEditTime = DateTime.Now;
            }
            else if( Editing && ( DateTime.Now - LastEditTime ).TotalMilliseconds > 200 ) {
                Editing = false;
                manager.Add( new ParsedSimpleCommand<Vector4>( this, StateBeforeEdit, Value ) );
            }
        }
    }
}
