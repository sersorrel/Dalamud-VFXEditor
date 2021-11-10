using ImGuiNET;
using System.IO;

namespace VFXEditor.Tmb {
    public class C131 : TmbItem {
        private short Time = 0;
        private int Unk_2 = 1;
        private int Unk_3 = 0;

        public static readonly string Name = "Movement Cancel (C131)";

        // Animation movement cancel
        public C131() { }
        public C131( BinaryReader reader ) {
            reader.ReadInt16(); // id
            Time = reader.ReadInt16(); // ?
            Unk_2 = reader.ReadInt32(); // 1
            Unk_3 = reader.ReadInt32(); // 0
        }

        public override int GetSize() => 0x14;
        public override int GetStringSize() => 0;
        public override int GetExtraSize() => 0;

        public override void Write( BinaryWriter entryWriter, int entryPos, BinaryWriter extraWriter, int extraPos, BinaryWriter stringWriter, int stringPos, int timelinePos ) {
            TmbFile.WriteString( entryWriter, "C131" );
            entryWriter.Write( 0x14 );
            entryWriter.Write( Id );
            entryWriter.Write( Time);
            entryWriter.Write( Unk_2);
            entryWriter.Write( Unk_3);
        }

        public override string GetName() => Name;

        public override void Draw( string id ) {
            TmbFile.ShortInput( $"Time{id}", ref Time );
            ImGui.InputInt( $"Uknown 2{id}", ref Unk_2 );
            ImGui.InputInt( $"Uknown 3{id}", ref Unk_3 );
        }
    }
}
