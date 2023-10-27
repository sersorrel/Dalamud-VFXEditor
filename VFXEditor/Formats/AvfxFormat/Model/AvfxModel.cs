using ImGuiNET;
using OtterGui.Raii;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VfxEditor.AvfxFormat.Model;
using VfxEditor.FilePicker;
using VfxEditor.Utils;
using VfxEditor.Utils.Gltf;
using static VfxEditor.DirectX.ModelPreview;

namespace VfxEditor.AvfxFormat {
    public class AvfxModel : AvfxNode {
        public const string NAME = "Modl";

        public readonly AvfxVertexes Vertexes = new();
        public readonly AvfxIndexes Indexes = new();
        public readonly AvfxEmitVertexes EmitVertexes = new();
        public readonly AvfxEmitVertexNumbers EmitVertexNumbers = new();

        public readonly List<UiEmitVertex> CombinedEmitVertexes = new();

        private readonly List<AvfxBase> Parsed;

        public readonly UiModelEmitSplitView EmitSplitDisplay;
        public readonly UiNodeGraphView NodeView;

        private int Mode = ( int )RenderMode.Color;
        private bool Refresh = false;
        private readonly UiModelUvView UvView;

        public AvfxModel() : base( NAME, AvfxNodeGroupSet.ModelColor ) {
            Parsed = new() {
                EmitVertexNumbers,
                EmitVertexes,
                Vertexes,
                Indexes
            };

            NodeView = new( this );
            UvView = new UiModelUvView();
            EmitSplitDisplay = new( this, CombinedEmitVertexes );
        }

        public override void ReadContents( BinaryReader reader, int size ) {
            ReadNested( reader, Parsed, size );
            if( EmitVertexes.EmitVertexes.Count != EmitVertexNumbers.VertexNumbers.Count ) {
                Dalamud.Error( $"Mismatched emit vertex counts {EmitVertexes.EmitVertexes.Count} {EmitVertexNumbers.VertexNumbers.Count}" );
            }
            for( var i = 0; i < Math.Min( EmitVertexes.EmitVertexes.Count, EmitVertexNumbers.VertexNumbers.Count ); i++ ) {
                CombinedEmitVertexes.Add( new UiEmitVertex( this, EmitVertexes.EmitVertexes[i], EmitVertexNumbers.VertexNumbers[i] ) );
            }
            EmitSplitDisplay.UpdateIdx();
        }

        protected override void RecurseChildrenAssigned( bool assigned ) => RecurseAssigned( Parsed, assigned );

        public override void WriteContents( BinaryWriter writer ) {
            EmitVertexes.EmitVertexes.Clear();
            EmitVertexes.EmitVertexes.AddRange( CombinedEmitVertexes.Select( x => x.Vertex ) );
            EmitVertexNumbers.VertexNumbers.Clear();
            EmitVertexNumbers.VertexNumbers.AddRange( CombinedEmitVertexes.Select( x => x.Number ) );

            if( EmitVertexNumbers.VertexNumbers.Count > 0 ) EmitVertexNumbers.Write( writer );
            if( EmitVertexes.EmitVertexes.Count > 0 ) EmitVertexes.Write( writer );
            if( Vertexes.Vertexes.Count > 0 ) Vertexes.Write( writer );
            if( Indexes.Indexes.Count > 0 ) Indexes.Write( writer );
        }

        public override void Draw() {
            using var _ = ImRaii.PushId( "Model" );
            NodeView.Draw();
            DrawRename();

            ImGui.TextDisabled( $"Vertices: {Vertexes.Vertexes.Count} Indexes: {Indexes.Indexes.Count}" );

            using( var style = ImRaii.PushStyle( ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemInnerSpacing ) ) {
                if( ImGui.Button( "Export" ) ) ImGui.OpenPopup( "ExportPopup" );

                using( var popup = ImRaii.Popup( "ExportPopup" ) ) {
                    if( popup ) {
                        if( ImGui.Selectable( ".gltf" ) ) ExportDialog();
                        if( ImGui.Selectable( ".avfx" ) ) Plugin.AvfxManager.ShowExportDialog( this );
                    }
                }

                ImGui.SameLine();
                if( ImGui.Button( "Replace" ) ) ImportDialog();

                ImGui.SameLine();
                UiUtils.WikiButton( "https://github.com/0ceal0t/Dalamud-VFXEditor/wiki/Replacing-textures-and-models#models" );
            }

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 4 );

            using var tabBar = ImRaii.TabBar( "ModelTabs" );
            if( !tabBar ) return;

            DrawModel3D();

            using( var tab = ImRaii.TabItem( "UV View" ) ) {
                if( tab ) UvView.Draw();
            }

            using( var tab = ImRaii.TabItem( "Emitter Vertices" ) ) {
                if( tab ) EmitSplitDisplay.Draw();
            }
        }

        public void OnSelect() {
            Plugin.DirectXManager.ModelPreview.LoadModel( this, RenderMode.Color );
            UvView.LoadModel( this );
        }

        private void DrawModel3D() {
            using var tabItem = ImRaii.TabItem( "3D View" );
            if( !tabItem ) return;

            using var _ = ImRaii.PushId( "3DModel" );

            if( Refresh ) {
                Plugin.DirectXManager.ModelPreview.LoadModel( this, ( RenderMode )Mode );
                UvView.LoadModel( this );
                Refresh = false;
            }

            if( ImGui.Checkbox( "Wireframe", ref Plugin.Configuration.ModelWireframe ) ) {
                Plugin.DirectXManager.ModelPreview.RefreshRasterizeState();
                Plugin.DirectXManager.ModelPreview.Draw();
                Plugin.Configuration.Save();
            }

            ImGui.SameLine();
            if( ImGui.Checkbox( "Show Edges", ref Plugin.Configuration.ModelShowEdges ) ) {
                Plugin.DirectXManager.ModelPreview.Draw();
                Plugin.Configuration.Save();
            }

            ImGui.SameLine();
            if( ImGui.Checkbox( "Show Emitter Vertices", ref Plugin.Configuration.ModelShowEmitters ) ) {
                Plugin.DirectXManager.ModelPreview.Draw();
                Plugin.Configuration.Save();
            }

            if( ImGui.RadioButton( "Color", ref Mode, ( int )RenderMode.Color ) ) {
                Plugin.DirectXManager.ModelPreview.LoadModel( this, RenderMode.Color );
            }

            ImGui.SameLine();
            if( ImGui.RadioButton( "UV 1", ref Mode, ( int )RenderMode.Uv1 ) ) {
                Plugin.DirectXManager.ModelPreview.LoadModel( this, RenderMode.Uv1 );
            }

            ImGui.SameLine();
            if( ImGui.RadioButton( "UV 2", ref Mode, ( int )RenderMode.Uv2 ) ) {
                Plugin.DirectXManager.ModelPreview.LoadModel( this, RenderMode.Uv2 );
            }

            ImGui.SameLine();
            if( ImGui.RadioButton( "Normal", ref Mode, ( int )RenderMode.Normal ) ) {
                Plugin.DirectXManager.ModelPreview.LoadModel( this, RenderMode.Normal );
            }

            Plugin.DirectXManager.ModelPreview.DrawInline();
        }

        private void ImportDialog() {
            FilePickerManager.OpenFileDialog( "Select a File", ".gltf,.*", ( bool ok, string res ) => {
                if( !ok ) return;
                try {
                    if( GltfModel.ImportModel( res, out var newVertexes, out var newIndexes ) ) {
                        CommandManager.Avfx.Add( new AvfxModelImportCommand( this, newIndexes, newVertexes ) );
                    }
                }
                catch( Exception e ) {
                    Dalamud.Error( e, "Could not import data" );
                }
            } );
        }

        private void ExportDialog() {
            FilePickerManager.SaveFileDialog( "Select a Save Location", ".gltf", "model", "gltf", ( bool ok, string res ) => {
                if( !ok ) return;
                GltfModel.ExportModel( this, res );
            } );
        }

        public void RefreshModelPreview() { Refresh = true; }

        public override string GetDefaultText() => $"Model {GetIdx()}";

        public override string GetWorkspaceId() => $"Mdl{GetIdx()}";
    }
}
