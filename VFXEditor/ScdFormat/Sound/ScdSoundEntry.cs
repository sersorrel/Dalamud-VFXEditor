using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VfxEditor.Parsing;

namespace VfxEditor.ScdFormat {
    public enum SoundType {
        Invalid = 0,
        Normal = 1,
        Random,
        Stereo,
        Cycle,
        Order,
        FourChannelSurround,
        Engine,
        Dialog,
        FixedPosition = 10,
        DynamixStream,
        GroupRandom,
        GroupOrder,
        Atomosgear,
        ConditionalJump,
        Empty,
        MidiMusic = 128
    }

    [Flags]
    public enum SoundAttribute {
        Invalid = 0,
        Loop = 0x0001,
        Reverb = 0x0002,
        FixedVolume = 0x0004,
        FixedPosition = 0x0008,
        Music = 0x0020,
        BypassPLIIz = 0x0040,
        UseExternalAttr = 0x0080,
        ExistRoutingSetting = 0x0100,
        MusicSurround = 0x0200,
        BusDucking = 0x0400,
        Acceleration = 0x0800,
        DynamixEnd = 0x1000,
        ExtraDesc = 0x2000,
        DynamixPlus = 0x4000,
        Atomosgear = 0x8000,
    }

    public class ScdSoundEntry : ScdEntry, IScdSimpleUiBase {
        public readonly ParsedByte BusNumber = new( "Bus Number" );
        public readonly ParsedByte Priority = new( "Priority" );
        public readonly ParsedEnum<SoundType> Type = new( "Type", size:1 );
        public readonly ParsedFlag<SoundAttribute> Attributes = new( "Attributes" );
        public readonly ParsedFloat Volume = new( "Volume" );
        public readonly ParsedShort LocalNumber = new( "Local Number" ); // TODO: ushort
        public readonly ParsedByte UserId = new( "User Id" );
        public readonly ParsedByte PlayHistory = new( "Play History" ); // TODO: sbyte

        public readonly SoundRoutingInfo RoutingInfo = new(); // include sendInfos, soundEffectParam
        public SoundBusDucking BusDucking = new();
        public SoundAcceleration Acceleration = new();
        public SoundAtomos Atomos = new();
        public SoundExtra Extra = new();
        public SoundRandomTracks RandomTracks = new(); // Includes Cycle
        public SoundTracks Tracks = new();

        private bool RoutingEnabled => Attributes.Value.HasFlag( SoundAttribute.ExistRoutingSetting );
        private bool BusDuckingEnabled => Attributes.Value.HasFlag( SoundAttribute.BusDucking );
        private bool AccelerationEnabled => Attributes.Value.HasFlag( SoundAttribute.Acceleration );
        private bool AtomosEnabled => Attributes.Value.HasFlag( SoundAttribute.Atomosgear );
        private bool ExtraEnabled => Attributes.Value.HasFlag( SoundAttribute.ExtraDesc );
        private bool RandomTracksEnabled => Type.Value == SoundType.Random || Type.Value == SoundType.Cycle || Type.Value == SoundType.GroupRandom;

        public ScdSoundEntry( BinaryReader reader, int offset ) : base( reader, offset ) { }

        public override void Read( BinaryReader reader ) {
            var trackCount = reader.ReadByte();
            BusNumber.Read( reader );
            Priority.Read( reader );
            Type.Read( reader );
            Attributes.Read( reader );
            Volume.Read( reader );
            LocalNumber.Read( reader );
            UserId.Read( reader );
            PlayHistory.Read( reader );

            if( RoutingEnabled ) RoutingInfo.Read( reader );
            if( BusDuckingEnabled ) BusDucking.Read( reader );
            if( AccelerationEnabled ) Acceleration.Read( reader );
            if( AtomosEnabled ) Atomos.Read( reader );
            if( ExtraEnabled ) Extra.Read( reader );

            if( RandomTracksEnabled ) RandomTracks.Read( reader, Type.Value, trackCount );
            else Tracks.Read( reader, trackCount );
        }

        public override void Write( BinaryWriter writer ) {
            writer.Write( ( byte )( RandomTracksEnabled ? RandomTracks.Tracks.Count : Tracks.Tracks.Count ) );
            BusNumber.Write( writer );
            Priority.Write( writer );
            Type.Write( writer );
            Attributes.Write( writer );
            Volume.Write( writer );
            LocalNumber.Write( writer );
            UserId.Write( writer );
            PlayHistory.Write( writer );

            if( RoutingEnabled ) RoutingInfo.Write( writer );
            if( BusDuckingEnabled ) BusDucking.Write( writer );
            if( AccelerationEnabled ) Acceleration.Write( writer );
            if( AtomosEnabled ) Atomos.Write( writer );
            if( ExtraEnabled ) Extra.Write( writer );

            if( RandomTracksEnabled ) RandomTracks.Write( writer, Type.Value );
            else Tracks.Write( writer );
        }

        public void Draw( string id ) {
            BusNumber.Draw( id, CommandManager.Scd );
            Priority.Draw( id, CommandManager.Scd );
            Type.Draw( id, CommandManager.Scd );
            Volume.Draw( id, CommandManager.Scd );
            LocalNumber.Draw( id, CommandManager.Scd );
            UserId.Draw( id, CommandManager.Scd );
            PlayHistory.Draw( id, CommandManager.Scd );

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            if( ImGui.BeginTabBar( $"{id}/SoundTabs", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton ) ) {
                // Draw attribute selection in its own tab
                if( ImGui.BeginTabItem( $"Attributes{id}" ) ) {
                    Attributes.Draw( $"{id}/Attributes", CommandManager.Scd );
                    ImGui.EndTabItem();
                }

                if( RoutingEnabled && ImGui.BeginTabItem($"Routing{id}" ) ) {
                    RoutingInfo.Draw( $"{id}/Routing" );
                    ImGui.EndTabItem();
                }
                if( BusDuckingEnabled && ImGui.BeginTabItem( $"Bus Ducking{id}" ) ) {
                    BusDucking.Draw( $"{id}/BusDucking" );
                    ImGui.EndTabItem();
                }
                if( AccelerationEnabled && ImGui.BeginTabItem( $"Acceleration{id}" ) ) {
                    Acceleration.Draw( $"{id}/Acceleration" );
                    ImGui.EndTabItem();
                }
                if( AtomosEnabled && ImGui.BeginTabItem( $"Atomos{id}" ) ) {
                    Atomos.Draw( $"{id}/Atomos" );
                    ImGui.EndTabItem();
                }
                if( ExtraEnabled && ImGui.BeginTabItem( $"Extra{id}" ) ) {
                    Extra.Draw( $"{id}/Extra" );
                    ImGui.EndTabItem();
                }
                if( ImGui.BeginTabItem( $"Tracks{id}" ) ) {
                    if( RandomTracksEnabled ) RandomTracks.Draw( $"{id}/Tracks", Type.Value );
                    else Tracks.Draw( $"{id}/Tracks" );
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
        }
    }
}