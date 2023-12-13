using System.IO;

namespace VfxEditor.Formats.SgbFormat.Layers.Objects.Data {
    public class LightInstanceObject : SgbObject {
        public LightInstanceObject( LayerEntryType type ) : base( type ) { }

        public LightInstanceObject( LayerEntryType type, BinaryReader reader ) : this( type ) {
            Read( reader );
        }

        protected override void DrawBody() {

        }

        protected override void ReadBody( BinaryReader reader, long startPos ) {

        }
    }
}