using CefSharp;
using CefSharp.WinForms;
using Autosu.Classes;
using Autosu.Utils;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using Autosu.Pages.SongSelect;
using Autosu.classes;
using Autosu.Pages.Bot;
using Autosu.classes.autopilot;
using Indieteur.GlobalHooks;

namespace Autosu {
    public partial class AutopilotPage : Form {
        public ChromiumWebBrowser browser = new();
        public static AutopilotPage? instance;


        private static GlobalKeyHook _globalKeyHook = new();

        public AutopilotPage() {
            InitializeComponent();
            Load += OnLoad;
            FormClosing += OnClose;
            Awake();

        }

        private void OnLoad(object sender, EventArgs e) {

        }

        void Awake() {
            instance = this;

            string uri = $"file://{CommonUtil.ParsePath("content/autopilot/index.html")}";
            browser = new(uri) {
                Dock = DockStyle.Top
            };

            Controls.Add(browser);
            browser.Dock = DockStyle.Fill;

            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            browser.JavascriptObjectRepository.Register("upstream", new AutopilotDownsream(browser, this), options: BindingOptions.DefaultBinder);

            browser.MenuHandler = new Std.MenuHandler();
            browser.KeyboardHandler = new Std.KeyboardHandler();

            WindowState = FormWindowState.Maximized;

            // autopilot key notify
            _globalKeyHook.OnKeyDown += (object sender, GlobalKeyEventArgs e) => {
                if (e.KeyCode == VirtualKeycodes.Enter) Autopilot.Arm();
            };
        }

        public void ReturnToMenu() {
            Invoke((MethodInvoker) delegate {
                SongSelectPage.instance.Show();
                Close();
            });
        }

        private void OnClose(object sender, FormClosingEventArgs args) {
            // do not forget to dispose autopilot
            instance = null;
            Autopilot.Dispose();
            SongSelectPage.instance.Show();
        }
    }

}