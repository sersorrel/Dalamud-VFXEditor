using Dalamud.Logging;
using ImGuiFileDialog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using VfxEditor.FileManager.Interfaces;
using VfxEditor.Interop.Havok;
using VfxEditor.Utils;

namespace VfxEditor {
    public enum WorkspaceState {
        None,
        Loading
    }

    public partial class Plugin {
        public static string CurrentWorkspaceLocation { get; private set; } = "";
        public static string CurrentWorkspaceName => string.IsNullOrEmpty( CurrentWorkspaceLocation ) ? "" : Path.GetFileName( CurrentWorkspaceLocation );

        private static DateTime LastAutoSave = DateTime.Now;
        public static WorkspaceState State { get; private set; } = WorkspaceState.None;
        public static readonly List<HavokData> HavokToInit = new(); // Make sure this is initialized on the main thread

        private static async void NewWorkspace() {
            await Task.Run( () => {
                State = WorkspaceState.Loading;
                Managers.ForEach( x => x?.ToDefault() );
                CurrentWorkspaceLocation = "";

                LastAutoSave = DateTime.Now;
                State = WorkspaceState.None;
            } );
        }

        private static void OpenWorkspace() {
            FileDialogManager.OpenFileDialog( "Select a Workspace File", "Workspace{.vfxworkspace,.json},.*", ( bool ok, string res ) => {
                if( !ok ) return;
                try {
                    var extension = new FileInfo( res ).Extension;
                    if( extension == ".json" ) { // OLD
                        var directory = Path.GetDirectoryName( res );
                        OpenWorkspaceAsync( directory.TrimEnd( Path.DirectorySeparatorChar ) + ".vfxworkspace", Path.GetDirectoryName( res ), false ); // Move to new format
                    }
                    else if( extension == ".vfxworkspace" ) { // NEW
                        OpenWorkspaceAsync( res );
                    }
                }
                catch( Exception e ) {
                    PluginLog.Error( e, "Could not load workspace" );
                }
            } );
        }

        private static void OpenWorkspaceAsync( string loadLocation ) => OpenWorkspaceAsync( loadLocation, Path.Combine( Path.GetDirectoryName( loadLocation ), "VFX_WORKSPACE_TEMP" ), true );

        private static async void OpenWorkspaceAsync( string loadLocation, string tempDir, bool unzip ) {
            await Task.Run( () => {
                if( unzip ) ZipFile.ExtractToDirectory( loadLocation, tempDir, true );
                var success = OpenWorkspaceFolder( tempDir );
                if( unzip ) Directory.Delete( tempDir, true );

                if( success ) {
                    CurrentWorkspaceLocation = loadLocation;
                    Configuration.AddRecentWorkspace( loadLocation );
                }
            } );
        }

        private static bool OpenWorkspaceFolder( string loadLocation ) {
            var metaPath = Path.Combine( loadLocation, "vfx_workspace.json" );
            if( !File.Exists( metaPath ) ) {
                PluginLog.Error( "vfx_workspace.json does not exist" );
                return false;
            }

            var meta = JObject.Parse( File.ReadAllText( metaPath ) );
            State = WorkspaceState.Loading;
            foreach( var manager in Managers ) {
                if( manager == null ) continue;
                manager.Dispose();
                manager.WorkspaceImport( meta, loadLocation );
            }
            LastAutoSave = DateTime.Now;
            State = WorkspaceState.None;

            return true;
        }

        private static void DrawLoadingDialog() {

        }

        // =================================

        private static void SaveWorkspace() {
            if( string.IsNullOrEmpty( CurrentWorkspaceLocation ) ) {
                SaveAsWorkspace();
            }
            else {
                ExportWorkspace();
            }
        }

        private static void SaveAsWorkspace() {
            FileDialogManager.SaveFileDialog( "Select a Save Location", ".vfxworkspace", "workspace", "vfxworkspace", ( bool ok, string res ) => {
                if( !ok ) return;
                CurrentWorkspaceLocation = res;
                ExportWorkspace();
            } );
        }

        private static async void ExportWorkspace() {
            await Task.Run( () => {
                var saveLocation = Path.Combine( Path.GetDirectoryName( CurrentWorkspaceLocation ), "VFX_WORKSPACE_TEMP" );
                Directory.CreateDirectory( saveLocation );

                var meta = new Dictionary<string, string>();
                Managers.ForEach( x => x?.WorkspaceExport( meta, saveLocation ) );

                var metaPath = Path.Combine( saveLocation, "vfx_workspace.json" );
                var metaString = JsonConvert.SerializeObject( meta );
                File.WriteAllText( metaPath, metaString );

                if( File.Exists( CurrentWorkspaceLocation ) ) File.Delete( CurrentWorkspaceLocation );
                ZipFile.CreateFromDirectory( saveLocation, CurrentWorkspaceLocation );
                Directory.Delete( saveLocation, true );

                UiUtils.OkNotification( "Saved workspace" );
            } );
        }

        public static void CleanupExport( IFileDocument document ) {
            TexToolsDialog.RemoveDocument( document );
            PenumbraDialog.RemoveDocument( document );
        }
    }
}
