using Autosu.classes;
using CefSharp.WinForms;
using CefSharp;
using System.Reflection;
using Autosu.Hooks;
using Indieteur.GlobalHooks;

namespace Autosu {
    internal static class App {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        /// 

        [STAThread]
        static void Main() {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            CefSettings settings = new();

            Cef.Initialize(settings);

            Application.Run(new SongSelectPage());
        }
    }
}