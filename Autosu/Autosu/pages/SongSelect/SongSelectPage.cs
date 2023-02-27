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
using Indieteur.GlobalHooks;
using Autosu.classes.autopilot;
using Newtonsoft.Json;
using System.Linq;
using Autosu.utils;
using WindowsInput;
using WindowsInput.Native;
using System.Diagnostics;
using System.Numerics;

namespace Autosu {
    public partial class SongSelectPage : Form {
        public ChromiumWebBrowser browser = new();
        public static SongSelectPage instance;
        public static GlobalKeyHook globalKeyHook = new();
        public static GlobalMouseHook globalMouseHook = new();

        public SongSelectPage() {
            InitializeComponent();
            Load += OnLoad;
            FormClosing += OnClose;
            Awake();
        }

        private void OnLoad(object sender, EventArgs e) {

        }

        void Awake() {
            instance = this;

            string uri = $"file://{CommonUtil.ParsePath("content/songSelect/index.html")}";
            browser = new(uri) {
                Dock = DockStyle.Top
            };

            Controls.Add(browser);
            browser.Dock = DockStyle.Fill;

            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            browser.JavascriptObjectRepository.Register("upstream", new SongSelectDownstream(browser, this), options: BindingOptions.DefaultBinder);

            browser.MenuHandler = new Std.MenuHandler();
            browser.KeyboardHandler = new Std.KeyboardHandler();
            
            //WindowState = FormWindowState.Maximized;
            globalKeyHook.OnKeyDown += MainKeyboardDown;
            globalMouseHook.OnButtonDown += MainMouseDown;
            Vector2 startPos = new(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2);
            this.Icon = new Icon(CommonUtil.ParsePath("resources/common/logo.ico"));
        }

        private void MainKeyboardDown(object sender, GlobalKeyEventArgs e) {
            if (new List<VirtualKeycodes>(globalKeyHook.KeysBeingPressed).Contains(VirtualKeycodes.LeftShift) && e.KeyCode == VirtualKeycodes.End) Environment.Exit(0);
            
            // regular
            if (new List<VirtualKeycodes>(globalKeyHook.KeysBeingPressed).Contains(VirtualKeycodes.LeftShift) && e.KeyCode == VirtualKeycodes.RightArrow) Autopilot.i.TryRegularCalib(true);
            if (new List<VirtualKeycodes>(globalKeyHook.KeysBeingPressed).Contains(VirtualKeycodes.LeftShift) && e.KeyCode == VirtualKeycodes.LeftArrow) Autopilot.i.TryRegularCalib(false);

            if (e.KeyCode == VirtualKeycodes.C) Autopilot.i.TryN1Calib();
            if (e.KeyCode == VirtualKeycodes.A && Autopilot.i.navTarget != null) Debug.WriteLine($"Mark placed at: #{Autopilot.i.navTarget.time}");

            if (AutopilotPage.instance != null) {
                switch (e.KeyCode) {
                    case VirtualKeycodes.Home: AutopilotPage.instance.visible = !AutopilotPage.instance.visible; break;
                    case VirtualKeycodes.End:
                        if (Autopilot.i.status > EAutopilotMasterState.OFF) Autopilot.i.Disengage(!(Autopilot.i.status > EAutopilotMasterState.ARM));
                        break;
                }
            }
        }

        private void MainMouseDown(object sender, GlobalMouseEventArgs e) {
            if (AutopilotPage.instance != null) {
                switch (e.Button) {
                    case GHMouseButtons.Left: Autopilot.i.Arm(); break;
                }
            }
        }

        public void SwitchPage<T>() where T: Form, new(){
            Invoke((MethodInvoker) delegate {
                T newPage = new();
                newPage.Show();
                Hide();
            });
        }

        private void OnClose(object sender, FormClosingEventArgs args) {
            if (Application.OpenForms.Count == 1) Environment.Exit(0);
        }
    }

}