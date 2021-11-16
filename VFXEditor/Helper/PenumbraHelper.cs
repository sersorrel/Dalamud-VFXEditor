using System;
using System.IO;

namespace VFXEditor.Helper {
    public static class PenumbraHelper {
        public static void WriteBytes( byte[] data, string modFolder, string path ) {
            var modFile = Path.Combine( modFolder, path );
            var modFileFolder = Path.GetDirectoryName( modFile );
            Directory.CreateDirectory( modFileFolder );
            File.WriteAllBytes( modFile, data );
        }

        public static void CopyFile( string localPath, string modFolder, string path ) {
            var modFile = Path.Combine( modFolder, path );
            var modFileFolder = Path.GetDirectoryName( modFile );
            Directory.CreateDirectory( modFileFolder );
            File.Copy( localPath, modFile, true );
        }
    }
}