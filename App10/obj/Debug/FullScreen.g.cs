﻿

#pragma checksum "E:\App10\App10\FullScreen.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5DF8CC79F37084AC818E1B5DF6ACC454"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace App10
{
    partial class FullScreen : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 15 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).KeyDown += this.Grid_KeyDown;
                 #line default
                 #line hidden
                #line 15 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).KeyUp += this.Grid_KeyUp;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 24 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).KeyUp += this.Grid_KeyUp;
                 #line default
                 #line hidden
                #line 24 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).KeyDown += this.Grid_KeyDown;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 128 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.OnBackToWindowsClick;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 103 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.OnFindResultsListBoxSelectionChanged;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 86 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.OnGoToLineButtonClick;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 51 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.OnErrorListBoxSelectionChanged;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 30 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.BoldClick;
                 #line default
                 #line hidden
                break;
            case 8:
                #line 31 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ItalicClick;
                 #line default
                 #line hidden
                break;
            case 9:
                #line 32 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ToggleButton)(target)).Checked += this.lineNUmberButton_Checked;
                 #line default
                 #line hidden
                #line 32 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ToggleButton)(target)).Unchecked += this.toggle_uncheck;
                 #line default
                 #line hidden
                break;
            case 10:
                #line 34 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.brse_Click;
                 #line default
                 #line hidden
                break;
            case 11:
                #line 35 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.OnNewButtonClick;
                 #line default
                 #line hidden
                break;
            case 12:
                #line 36 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.OpenFileButton_Click;
                 #line default
                 #line hidden
                break;
            case 13:
                #line 37 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.OnFormatButton_Click;
                 #line default
                 #line hidden
                break;
            case 14:
                #line 148 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).KeyUp += this.Grid_KeyUp;
                 #line default
                 #line hidden
                #line 148 "..\..\FullScreen.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).KeyDown += this.Grid_KeyDown;
                 #line default
                 #line hidden
                #line 151 "..\..\FullScreen.xaml"
                ((global::ActiproSoftware.UI.Xaml.Controls.SyntaxEditor.SyntaxEditor)(target)).DocumentParseDataChanged += this.OnSyntaxEditorDocumentParseDataChanged;
                 #line default
                 #line hidden
                #line 152 "..\..\FullScreen.xaml"
                ((global::ActiproSoftware.UI.Xaml.Controls.SyntaxEditor.SyntaxEditor)(target)).UserInterfaceUpdate += this.OnSyntaxEditorUserInterfaceUpdate;
                 #line default
                 #line hidden
                #line 153 "..\..\FullScreen.xaml"
                ((global::ActiproSoftware.UI.Xaml.Controls.SyntaxEditor.SyntaxEditor)(target)).ViewSearch += this.OnEditorViewSearch;
                 #line default
                 #line hidden
                #line 154 "..\..\FullScreen.xaml"
                ((global::ActiproSoftware.UI.Xaml.Controls.SyntaxEditor.SyntaxEditor)(target)).ViewSelectionChanged += this.OnSyntaxEditorViewSelectionChanged;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


