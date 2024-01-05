using System.Collections.Generic;
using VfxEditor.Formats.MdlFormat.Bone;
using VfxEditor.Formats.MdlFormat.Mesh;
using VfxEditor.Formats.MdlFormat.Mesh.Shape;
using VfxEditor.Formats.MdlFormat.Mesh.TerrainShadow;

namespace VfxEditor.Formats.MdlFormat.Utils {
    public class MdlReaderData {
        public readonly List<uint> VertexBufferOffsets = new();
        public readonly List<uint> IndexBufferOffsets = new();

        public readonly List<MdlMesh> Meshes = new();
        public readonly List<MdlTerrainShadowMesh> TerrainShadowMeshes = new();
        public readonly List<MdlSubMesh> SubMeshes = new();
        public readonly List<MdlTerrainShadowSubmesh> TerrainShadowSubmeshes = new();

        public readonly Dictionary<uint, string> StringOffsets = new();
        public readonly List<string> AttributeStrings = new();
        public readonly List<string> MaterialStrings = new();
        public readonly List<string> BoneStrings = new();

        public readonly List<MdlBoneTable> BoneTables = new();
        public readonly List<ushort> SubmeshBoneMap = new();

        public readonly List<MdlShape> Shapes = new();
        public readonly List<MdlShapeMesh> ShapesMeshes = new();
        public readonly List<MdlShapeValue> ShapeValues = new();
    }
}