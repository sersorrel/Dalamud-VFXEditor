using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using Lumina.Data.Files;

using VFXSelect.Select.Sheets;

namespace VFXSelect {
    public abstract class SelectTab {
        public abstract void Draw();
    }

    public abstract class SelectTab<T, S> : SelectTab {
        protected abstract bool CheckMatch( T item, string searchInput );
        protected abstract string UniqueRowTitle( T item );
        protected abstract void DrawSelected( S loadedItem );
        protected virtual void DrawExtra() { }
        protected virtual void OnSelect() { }

        protected readonly string Id;
        protected readonly string Name;
        protected readonly string ParentId;
        protected readonly SheetLoader<T, S> Loader;

        protected string SearchInput = "";
        protected T Selected = default;
        protected S Loaded = default;

        protected List<T> Searched;

        public SelectTab( string parentId, string tabId, SheetLoader<T, S> loader ) {
            Loader = loader;
            Name = tabId;
            ParentId = parentId;
            Id = "##Select/" + tabId + "/" + parentId;
        }

        protected void LoadIcon( ushort iconId, ref ImGuiScene.TextureWrap texWrap ) {
            texWrap?.Dispose();
            texWrap = null;
            if( iconId > 0 ) {
                TexFile tex;
                try {
                    tex = SheetManager.DataManager.GetIcon( iconId );
                }
                catch( Exception ) {
                    tex = SheetManager.DataManager.GetIcon( 0 );
                }
                texWrap = SheetManager.PluginInterface.UiBuilder.LoadImageRaw( BGRA_to_RGBA( tex.ImageData ), tex.Header.Width, tex.Header.Height, 4 );
            }
        }

        protected static byte[] BGRA_to_RGBA( byte[] data ) {
            var ret = new byte[data.Length];
            for( var i = 0; i < data.Length / 4; i++ ) {
                var idx = i * 4;
                ret[idx + 0] = data[idx + 2];
                ret[idx + 1] = data[idx + 1];
                ret[idx + 2] = data[idx + 0];
                ret[idx + 3] = data[idx + 3];
            }
            return ret;
        }

        public override void Draw() {
            var ret = ImGui.BeginTabItem( Name + "##Select/" + ParentId );
            if( !ret )
                return;
            Loader.Load();
            if( !Loader.Loaded ) {
                ImGui.EndTabItem();
                return;
            }
            //
            if( Searched == null ) { Searched = new List<T>(); Searched.AddRange( Loader.Items ); }
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            var ResetScroll = false;
            DrawExtra();
            if( ImGui.InputText( "Search" + Id, ref SearchInput, 255 ) ) {
                Searched = Loader.Items.Where( x => CheckMatch( x, SearchInput ) ).ToList();
                ResetScroll = true;
            }
            ImGui.Columns( 2, Id + "Columns", true );
            ImGui.BeginChild( Id + "Tree" );
            DisplayVisible( Searched.Count, out var preItems, out var showItems, out var postItems, out var itemHeight );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + preItems * itemHeight );
            if( ResetScroll ) { ImGui.SetScrollHereY(); };
            var idx = 0;
            foreach( var item in Searched ) {
                if( idx < preItems || idx > ( preItems + showItems ) ) { idx++; continue; }
                if( ImGui.Selectable( UniqueRowTitle( item ), EqualityComparer<T>.Default.Equals( Selected, item ) ) ) {
                    if( !EqualityComparer<T>.Default.Equals( Selected, item ) ) {
                        Task.Run( async () => {
                            var result = Loader.SelectItem( item, out Loaded );
                        } );
                        Selected = item;
                        OnSelect();
                    }
                }
                idx++;
            }
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + postItems * itemHeight );
            ImGui.EndChild();
            ImGui.NextColumn();
            // ========================
            if( Selected == null ) {
                ImGui.Text( "Select an item..." );
            }
            else {
                if( Loaded != null ) {
                    ImGui.BeginChild( Id + "Selected" );

                    DrawSelected( Loaded );

                    ImGui.EndChild();
                }
                else {
                    ImGui.Text( "No data found" );
                }
            }
            ImGui.Columns( 1 );
            //
            ImGui.EndTabItem();
        }

        public static bool Matches( string item, string query ) {
            return item.ToLower().Contains( query.ToLower() );
        }

        public static void DisplayPath( string path ) {
            ImGui.PushStyleColor( ImGuiCol.Text, new Vector4( 0.8f, 0.8f, 0.8f, 1 ) );
            ImGui.TextWrapped( path );
            ImGui.PopStyleColor();
        }

        public static void Copy( string copyPath, string id = "" ) {
            ImGui.PushStyleColor( ImGuiCol.Button, new Vector4( 0.15f, 0.15f, 0.15f, 1 ) );
            if( ImGui.Button( "Copy##" + id ) ) {
                ImGui.SetClipboardText( copyPath );
            }
            ImGui.PopStyleColor();
        }

        public static void DisplayVisible( int count, out int preItems, out int showItems, out int postItems, out float itemHeight ) {
            var childHeight = ImGui.GetWindowSize().Y - ImGui.GetCursorPosY();
            var scrollY = ImGui.GetScrollY();
            var style = ImGui.GetStyle();
            itemHeight = ImGui.GetTextLineHeight() + style.ItemSpacing.Y;
            preItems = ( int )Math.Floor( scrollY / itemHeight );
            showItems = ( int )Math.Ceiling( childHeight / itemHeight );
            postItems = count - showItems - preItems;

        }
    }
}