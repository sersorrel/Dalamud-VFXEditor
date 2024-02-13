using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VfxEditor.FileManager;
using VfxEditor.Formats.MdlFormat.Bone;
using VfxEditor.Formats.MdlFormat.Box;
using VfxEditor.Formats.MdlFormat.Element;
using VfxEditor.Formats.MdlFormat.Lod;
using VfxEditor.Formats.MdlFormat.Mesh.Shape;
using VfxEditor.Formats.MdlFormat.Utils;
using VfxEditor.Formats.MdlFormat.Vertex;
using VfxEditor.Parsing;
using VfxEditor.Ui.Components;
using VfxEditor.Ui.Components.SplitViews;
using VfxEditor.Utils;

namespace VfxEditor.Formats.MdlFormat {
    // https://github.com/Caraxi/SimpleHeels/blob/d345d7406958e70fb3c6823cee21872ffa65621b/Files/MdlFile.cs#L291
    // https://github.com/NotAdam/Lumina/blob/40dab50183eb7ddc28344378baccc2d63ae71d35/src/Lumina/Data/Files/MdlFile.cs#L7

    [Flags]
    public enum ModelFlags1 : int {
        Dust_Occlusion = 0x80,
        Snow_Occlusion = 0x40,
        Rain_Occlusion = 0x20,
        Unknown_1 = 0x10,
        Lighting_Reflection = 0x08,
        Waving_Animation_Disabled = 0x04,
        Light_Shadow_Disabled = 0x02,
        Shadow_Disabled = 0x01,
    }

    [Flags]
    public enum ModelFlags2 : int {
        Unknown_2 = 0x80,
        Background_UV_Scroll = 0x40,
        Force_NonResident = 0x20,
        Extra_LoD = 0x10,
        Shadow_Mask = 0x08,
        Force_LoD_Range = 0x04,
        Edge_Geometry = 0x02,
        Unknown_3 = 0x01
    }

    public class MdlFile : FileManagerFile {
        private readonly uint Version;

        private readonly ParsedByteBool IndexBufferStreaming = new( "Index Buffer Streaming" );
        private readonly ParsedByteBool EdgeGeometry = new( "Edge Geometry" );
        private readonly ParsedFloat Radius = new( "Radius" );
        private readonly ParsedFlag<ModelFlags1> Flags1 = new( "Flags 1", size: 1 );
        private readonly ParsedFlag<ModelFlags2> Flags2 = new( "Flags 2", size: 1 );
        private readonly ParsedFloat ModelClipOutDistance = new( "Model Clip Out Distance" );
        private readonly ParsedFloat ShadowClipOutDistance = new( "Shadow Clip Out Distance" );
        private readonly ParsedShort Unknown4 = new( "Unknown 4" );
        private readonly ParsedByte Unknown5 = new( "Unknown 5" );
        private readonly ParsedByte BgChangeMaterialIndex = new( "Background Change Material Index" );
        private readonly ParsedByte BgCrestChangeMaterialIndex = new( "Background Crest Change Material Index" );
        private readonly ParsedByte Unknown6 = new( "Unknown 6" );
        private readonly ParsedShort Unknown7 = new( "Unknown 7" );
        private readonly ParsedShort Unknown8 = new( "Unknown 8" );
        private readonly ParsedShort Unknown9 = new( "Unknown 9" );

        public readonly List<MdlEid> Eids = [];
        private readonly UiSplitView<MdlEid> EidView;

        public readonly List<MdlLod> AllLods = [];
        public readonly List<MdlLod> UsedLods = [];
        private readonly UiDropdown<MdlLod> LodView;

        private bool ExtraLodEnabled => Flags2.Value.HasFlag( ModelFlags2.Extra_LoD );
        public readonly List<MdlExtraLod> ExtraLods = [];
        private readonly UiDropdown<MdlExtraLod> ExtraLodView;

        public readonly List<MdlBoneTable> BoneTables = [];
        private readonly UiSplitView<MdlBoneTable> BoneTableView;

        public readonly List<MdlShape> Shapes = []; // TODO

        private readonly byte[] Padding;

        // TODO
        private readonly MdlBoundingBox UnknownBoundingBox;
        private readonly MdlBoundingBox ModelBoundingBox;
        private readonly MdlBoundingBox WaterBoundingBox;
        private readonly MdlBoundingBox VerticalFogBoundingBox;
        private readonly List<MdlBoneBoundingBox> BoneBoundingBoxes = [];

        // TODO
        //public const uint FileHeaderSize = 0x44;
        //public unsafe uint StackSize => ( uint )( VertexDeclarations.Length * NumVertices * sizeof( MdlStructs.VertexElement ) );
        // var runtimeSize = (uint)(totalSize - StackSize - FileHeaderSize);
        // var stackSize = vertexDeclarations.Length * 136;

        public MdlFile( BinaryReader reader, bool verify ) : base() {
            var data = new MdlFileData();

            Version = reader.ReadUInt32();
            reader.ReadUInt32(); // stack size
            reader.ReadUInt32(); // runtime size
            var vertexDeclarationCount = reader.ReadUInt16();
            var _materialCount = reader.ReadUInt16();

            var a = new List<uint>();
            var b = new List<uint>();

            // Order of the data is: V1, I1, V2, I2, V3, I3
            for( var i = 0; i < 3; i++ ) data.VertexBufferOffsets.Add( reader.ReadUInt32() );
            for( var i = 0; i < 3; i++ ) data.IndexBufferOffsets.Add( reader.ReadUInt32() );
            for( var i = 0; i < 3; i++ ) a.Add( reader.ReadUInt32() ); // vertex buffer sizes
            for( var i = 0; i < 3; i++ ) b.Add( reader.ReadUInt32() ); // index buffer sizes

            Dalamud.Log( $"V: {data.VertexBufferOffsets[0]:X4}/{a[0]:X4} {data.VertexBufferOffsets[1]:X4}/{a[1]:X4} {data.VertexBufferOffsets[2]:X4}/{a[2]:X4}" );
            Dalamud.Log( $"I: {data.IndexBufferOffsets[0]:X4}/{b[0]:X4} {data.IndexBufferOffsets[1]:X4}/{b[1]:X4} {data.IndexBufferOffsets[2]:X4}/{b[2]:X4}" );

            var _lodCount = reader.ReadByte(); // sometimes != 3, such as with bg/ffxiv/zon_z1/chr/z1c4/bgplate/0001.mdl
            IndexBufferStreaming.Read( reader );
            EdgeGeometry.Read( reader );
            reader.ReadByte(); // padding

            // ===== VERTEX DECLARATION ======

            var vertexFormats = new List<MdlVertexDeclaration>();
            for( var i = 0; i < vertexDeclarationCount; i++ ) vertexFormats.Add( new( reader ) );

            // ====== STRINGS ======

            var stringCount = reader.ReadUInt16();
            reader.ReadUInt16(); // padding
            var stringSize = reader.ReadUInt32();
            var stringStartPos = reader.BaseStream.Position;
            var stringEndPos = reader.BaseStream.Position + stringSize;

            for( var i = 0; i < stringCount; i++ ) {
                var pos = reader.BaseStream.Position - stringStartPos;
                var value = FileUtils.ReadString( reader );
                Dalamud.Log( $"string: {pos} {value}" );
                data.OffsetToString[( uint )pos] = value;
            }

            reader.BaseStream.Position = stringEndPos;

            // ====== MODEL HEADER =======

            Radius.Read( reader );
            var meshCount = reader.ReadUInt16();
            var attributeCount = reader.ReadUInt16();
            var submeshCount = reader.ReadUInt16();
            var materialCount = reader.ReadUInt16();
            var boneCount = reader.ReadUInt16();
            var boneTableCount = reader.ReadUInt16();
            var shapeCount = reader.ReadUInt16();
            var shapeMeshCount = reader.ReadUInt16();
            var shapeValueCount = reader.ReadUInt16();
            var lodCount = reader.ReadByte();
            Flags1.Read( reader );
            var elementIdCount = reader.ReadUInt16();
            var terrainShadowMeshCount = reader.ReadByte();
            Flags2.Read( reader );
            ModelClipOutDistance.Read( reader );
            ShadowClipOutDistance.Read( reader );
            Unknown4.Read( reader );
            var terrainShadowSubmeshCount = reader.ReadUInt16();
            Unknown5.Read( reader );
            BgChangeMaterialIndex.Read( reader );
            BgCrestChangeMaterialIndex.Read( reader );
            Unknown6.Read( reader );
            Unknown7.Read( reader );
            Unknown8.Read( reader );
            Unknown9.Read( reader );
            reader.ReadBytes( 6 ); // padding

            if( meshCount != vertexDeclarationCount || materialCount != _materialCount || _lodCount != lodCount ) {
                Dalamud.Error( $"Mesh:{meshCount}/{vertexDeclarationCount} Material:{materialCount}/{_materialCount} LoD:{lodCount}/{_lodCount}" );
            }

            // ====== DATA ========

            for( var i = 0; i < elementIdCount; i++ ) Eids.Add( new( data.OffsetToString, reader ) );

            // ====== LOD ========

            for( var i = 0; i < 3; i++ ) {
                var lod = new MdlLod( this, reader );
                AllLods.Add( lod );
                if( i < _lodCount ) UsedLods.Add( lod );
            }

            if( ExtraLodEnabled ) {
                Dalamud.Error( "Extra LoD" );
                for( var i = 0; i < 3; i++ ) ExtraLods.Add( new( this, reader ) );
            }

            // ===== MESHES ========

            for( var i = 0; i < meshCount; i++ ) data.Meshes.Add( new( this, vertexFormats[i], reader ) );
            for( var i = 0; i < attributeCount; i++ ) data.AttributeStrings.Add( data.OffsetToString[reader.ReadUInt32()] );
            for( var i = 0; i < terrainShadowMeshCount; i++ ) data.TerrainShadowMeshes.Add( new( this, reader ) );
            for( var i = 0; i < submeshCount; i++ ) data.SubMeshes.Add( new( reader ) );
            for( var i = 0; i < terrainShadowSubmeshCount; i++ ) data.TerrainShadowSubmeshes.Add( new( reader ) );
            for( var i = 0; i < materialCount; i++ ) data.MaterialStrings.Add( data.OffsetToString[reader.ReadUInt32()] );
            for( var i = 0; i < boneCount; i++ ) data.BoneStrings.Add( data.OffsetToString[reader.ReadUInt32()] );
            for( var i = 0; i < boneTableCount; i++ ) BoneTables.Add( new( reader, data.BoneStrings ) );

            // // ======== SHAPES ============

            for( var i = 0; i < shapeCount; i++ ) data.Shapes.Add( new( reader, data.OffsetToString ) );
            for( var i = 0; i < shapeMeshCount; i++ ) data.ShapesMeshes.Add( new( reader ) );
            for( var i = 0; i < shapeValueCount; i++ ) data.ShapeValues.Add( new( reader ) );
            var submeshBoneMapSize = reader.ReadUInt32();
            for( var i = 0; i < submeshBoneMapSize / 2; i++ ) data.SubmeshBoneMap.Add( reader.ReadUInt16() );

            Shapes = data.Shapes; // TODO: ????

            Padding = reader.ReadBytes( reader.ReadByte() );

            UnknownBoundingBox = new( reader );
            ModelBoundingBox = new( reader );
            WaterBoundingBox = new( reader );
            VerticalFogBoundingBox = new( reader );
            for( var i = 0; i < data.BoneStrings.Count; i++ ) BoneBoundingBoxes.Add( new( data.BoneStrings[i], reader ) );

            // ===== POPULATE =======

            foreach( var shape in data.Shapes ) shape.Populate( data );
            for( var i = 0; i < AllLods.Count; i++ ) AllLods[i].Populate( data, reader, i );
            for( var i = 0; i < ExtraLods.Count; i++ ) ExtraLods[i].Populate( data, reader, i ); // TODO: should this use vertexOffsets[i]?

            // ====== VIEWS ============

            EidView = new( "Bind Point", Eids, false );
            LodView = new( "Level of Detail", UsedLods );
            ExtraLodView = new( "Level of Detail", ExtraLods );
            BoneTableView = new( "Bone Table", BoneTables, false );

            if( verify ) Verified = FileUtils.Verify( reader, ToBytes(), null );
        }

        public override void Draw() {
            using var tabBar = ImRaii.TabBar( "Tabs", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton );
            if( !tabBar ) return;

            using( var tab = ImRaii.TabItem( "Parameters" ) ) {
                if( tab ) DrawParameters();
            }

            using( var tab = ImRaii.TabItem( "Bind Points" ) ) {
                if( tab ) EidView.Draw();
            }

            using( var tab = ImRaii.TabItem( "Bone Tables" ) ) {
                if( tab ) BoneTableView.Draw();
            }

            using( var tab = ImRaii.TabItem( "Levels of Detail" ) ) {
                if( tab ) LodView.Draw();
            }

            if( ExtraLodEnabled ) {
                using var tab = ImRaii.TabItem( "Extra LoD" );
                if( tab ) ExtraLodView.Draw();
            }
        }

        private void DrawParameters() {
            using var child = ImRaii.Child( "Child" );

            IndexBufferStreaming.Draw();
            EdgeGeometry.Draw();
            Radius.Draw();
            Flags1.Draw();
            Flags2.Draw(); // Not gonna handle if ExtraLoD is checked :/
            ModelClipOutDistance.Draw();
            ShadowClipOutDistance.Draw();
            Unknown4.Draw();
            Unknown5.Draw();
            BgChangeMaterialIndex.Draw();
            BgCrestChangeMaterialIndex.Draw();
            Unknown6.Draw();
            Unknown7.Draw();
            Unknown8.Draw();
            Unknown9.Draw();
        }

        public override void Write( BinaryWriter writer ) {
            var data = new MdlWriteData( this );

            writer.Write( Version );
            writer.Write( data.Meshes.Count * 136 ); // stack size
            var runtimePlaceholder = writer.BaseStream.Position;
            writer.Write( 0 ); // placeholder: runtime size
            writer.Write( ( ushort )data.Meshes.Count ); // vertex declaration count
            writer.Write( ( ushort )data.MaterialStrings.Count );

            // placeholders
            var placeholders = writer.BaseStream.Position;
            for( var i = 0; i < 12; i++ ) writer.Write( 0 ); // 3 x vertex offsets, 3 x index offsets, 3 x vertex size, 3 x index size

            writer.Write( ( byte )AllLods.Count );
            IndexBufferStreaming.Write( writer );
            EdgeGeometry.Write( writer );
            writer.Write( ( byte )0 ); // padding

            foreach( var mesh in data.Meshes ) mesh.Format.Write( writer );

            writer.Write( ( ushort )data.AllStrings.Count );
            writer.Write( ( ushort )0 ); // padding

            var stringPadding = ( uint )FileUtils.NumberToPad( writer.BaseStream.Position + data.TotalStringLength, 4 );
            writer.Write( data.TotalStringLength + stringPadding );
            foreach( var item in data.AllStrings ) FileUtils.WriteString( writer, item, true );
            FileUtils.Pad( writer, stringPadding );

            Radius.Write( writer );
            writer.Write( ( ushort )data.Meshes.Count );
            writer.Write( ( ushort )data.AttributeStrings.Count );
            writer.Write( ( ushort )data.SubMeshes.Count );
            writer.Write( ( ushort )data.MaterialStrings.Count );
            writer.Write( ( ushort )data.BoneStrings.Count );
            writer.Write( ( ushort )BoneTables.Count );
            writer.Write( ( ushort )data.Shapes.Count );
            writer.Write( ( ushort )data.ShapesMeshes.Count );
            writer.Write( ( ushort )data.ShapeValues.Count );
            writer.Write( ( byte )UsedLods.Count );
            Flags1.Write( writer );
            writer.Write( ( ushort )Eids.Count );
            writer.Write( ( byte )data.TerrainShadowMeshes.Count );
            Flags2.Write( writer );
            ModelClipOutDistance.Write( writer );
            ShadowClipOutDistance.Write( writer );
            Unknown4.Write( writer );
            writer.Write( ( ushort )data.TerrainShadowSubmeshes.Count );
            Unknown5.Write( writer );
            BgChangeMaterialIndex.Write( writer );
            BgCrestChangeMaterialIndex.Write( writer );
            Unknown6.Write( writer );
            Unknown7.Write( writer );
            Unknown8.Write( writer );
            Unknown9.Write( writer );
            FileUtils.Pad( writer, 6 ); // Padding

            foreach( var item in Eids ) item.Write( writer, data );
            foreach( var item in AllLods ) item.Write( writer, data );
            foreach( var item in ExtraLods ) item.Write( writer, data );

            foreach( var item in data.Meshes ) item.Write( writer, data );
            foreach( var item in data.AttributeStrings ) writer.Write( data.StringToOffset[item] );
            foreach( var item in data.TerrainShadowMeshes ) item.Write( writer, data );
            foreach( var item in data.SubMeshes ) item.Write( writer, data );
            foreach( var item in data.TerrainShadowSubmeshes ) item.Write( writer );
            foreach( var item in data.MaterialStrings ) writer.Write( data.StringToOffset[item] );
            foreach( var item in data.BoneStrings ) writer.Write( data.StringToOffset[item] );
            foreach( var item in BoneTables ) item.Write( writer, data );

            foreach( var item in data.Shapes ) item.Write( writer, data );
            foreach( var item in data.ShapesMeshes ) item.Write( writer, data );
            foreach( var item in data.ShapeValues ) item.Write( writer );

            var boneMapPadding = ( uint )FileUtils.NumberToPad( writer.BaseStream.Position + ( data.SubmeshBoneMap.Count * 2 ), 4 );
            writer.Write( ( uint )data.SubmeshBoneMap.Count * 2 + boneMapPadding );
            foreach( var item in data.SubmeshBoneMap ) writer.Write( item );
            FileUtils.Pad( writer, boneMapPadding );

            writer.Write( ( byte )Padding.Length );
            writer.Write( Padding );

            UnknownBoundingBox.Write( writer );
            ModelBoundingBox.Write( writer );
            WaterBoundingBox.Write( writer );
            VerticalFogBoundingBox.Write( writer );

            foreach( var bone in data.BoneStrings ) {
                if( BoneBoundingBoxes.FindFirst( x => x.Name.Value.Equals( bone ), out var box ) ) {
                    box.Write( writer );
                    continue;
                }
                FileUtils.Pad( writer, 32 ); // Couldn't find one, just skip it
            }

            // ================

            var vertexSizes = data.VertexData.Select( x => x.Length ).ToList();
            var indexSizes = data.IndexData.Select( x => x.Length ).ToList();
            var vertexOffsets = new List<uint>();
            var indexOffsets = new List<uint>();

            for( var i = 0; i < 3; i++ ) {
                vertexOffsets.Add( vertexSizes[i] == 0 ? 0 : ( uint )writer.BaseStream.Position );
                writer.Write( data.VertexData[i].ToArray() );

                indexOffsets.Add( indexSizes[i] == 0 ? 0 : ( uint )writer.BaseStream.Position );
                writer.Write( data.IndexData[i].ToArray() );
            }

            Dalamud.Log( $">>>V: {vertexOffsets[0]:X4}/{vertexSizes[0]:X4} {vertexOffsets[1]:X4}/{vertexSizes[1]:X4} {vertexOffsets[2]:X4}/{vertexSizes[2]:X4}" );
            Dalamud.Log( $">>>I: {indexOffsets[0]:X4}/{indexSizes[0]:X4} {indexOffsets[1]:X4}/{indexSizes[1]:X4} {indexOffsets[2]:X4}/{indexSizes[2]:X4}" );

            // ===============

            data.Dispose();
        }
    }
}
