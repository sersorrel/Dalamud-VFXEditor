using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using VfxEditor.Formats.KdbFormat.Nodes.Types;
using VfxEditor.Formats.KdbFormat.Nodes.Types.Source;
using VfxEditor.Ui.NodeGraphViewer;

namespace VfxEditor.Formats.KdbFormat.Nodes {
    public class KdbNodeGraphViewer : NodeGraphViewer<KdbNode, KdbSlot> {
        public KdbNodeGraphViewer() { }

        protected override void DrawUtilsBar() {
            using( var font = ImRaii.PushFont( UiBuilder.IconFont ) ) {
                if( ImGui.Button( FontAwesomeIcon.Plus.ToIconString() ) ) {
                    ImGui.OpenPopup( "NewPopup" );
                }
            }

            using( var popup = ImRaii.Popup( "NewPopup" ) ) {
                if( popup ) {
                    if( ImGui.Selectable( "EffectorEZParamLink" ) ) AddToCanvas( new KdbNodeEffectorEZParamLink(), true );
                    if( ImGui.Selectable( "EffectorEZParamLinkLinear" ) ) AddToCanvas( new KdbNodeEffectorEZParamLinkLinear(), true );
                    if( ImGui.Selectable( "SourceRotate" ) ) AddToCanvas( new KdbNodeSourceRotate(), true );
                    if( ImGui.Selectable( "TargetBendSTRoll" ) ) AddToCanvas( new KdbNodeTargetBendSTRoll(), true );
                    if( ImGui.Selectable( "TargetTranslate" ) ) AddToCanvas( new KdbNodeTargetTranslate(), true );
                }
            }

            ImGui.SameLine();
            // ===================
            base.DrawUtilsBar();
        }
    }
}
