﻿#pragma checksum "C:\Users\leonb\Documents\GitHub\GeometryStreamer\GeoServer\GeoCoreUwpClient\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6B59085FDA664F67531D18BE99B03E4F"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GeoCoreUwpClient
{
    partial class MainPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // MainPage.xaml line 29
                {
                    this.scrollView = (global::Windows.UI.Xaml.Controls.ScrollViewer)(target);
                }
                break;
            case 3: // MainPage.xaml line 31
                {
                    this.debugLog = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 4: // MainPage.xaml line 23
                {
                    this.TestDataButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.TestDataButton).Click += this.TestDataOnClick;
                }
                break;
            case 5: // MainPage.xaml line 24
                {
                    this.AltTestButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.AltTestButton).Click += this.AlternativeTestDataOnClick;
                }
                break;
            case 6: // MainPage.xaml line 19
                {
                    this.ConnectButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ConnectButton).Click += this.ConnectOnClick;
                }
                break;
            case 7: // MainPage.xaml line 20
                {
                    this.DisconnectButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.DisconnectButton).Click += this.DisconnectOnClick;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

