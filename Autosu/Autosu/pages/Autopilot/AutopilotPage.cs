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
using System.Runtime.InteropServices;

namespace Autosu {
    public partial class AutopilotPage : Form {
        public ChromiumWebBrowser browser = new();
        public static AutopilotPage? instance;

        #region Overlay Driver
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern bool SetThreadDesktop(IntPtr hDesktop);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x8;
        private const int WS_EX_TRANSPARENT = 0x20;
        #endregion

        public static bool gameHasLaunched;

        private bool _visible = true;

        public AutopilotPage() {
            InitializeComponent();
            Load += OnLoad;
            FormClosing += OnClose;
            Awake();

        }

        public bool visible {
            get => _visible;
            set {
                if (!gameHasLaunched) return;

                Invoke(() => {
                    SetOverlay(true, Autopilot.status < EAutopilotMasterState.ON);
                    Opacity = !value ? 0f : Autopilot.status >= EAutopilotMasterState.ON ? 0.7f : 1f ;
                });

                _visible = value;
            }
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

        }

        public void SetOverlay(bool isOverlay, bool handleInput) {
            if (isOverlay) {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                TopMost = true;
                StartPosition = FormStartPosition.Manual;
                BackColor = Color.LightPink;
                WindowState = FormWindowState.Maximized;
                //Opacity = 0.7f;

                // Set WS_EX_TRANSPARENT style on the overlay form
                if (handleInput) SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_TRANSPARENT);
                else SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) & ~WS_EX_TRANSPARENT);

                // Set the position of the overlay form
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width, 0);

            } else {
                TopMost = true;
                FormBorderStyle = FormBorderStyle.FixedSingle;
                // Remove WS_EX_TOPMOST and WS_EX_TRANSPARENT styles
                SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) & ~WS_EX_TOPMOST & ~WS_EX_TRANSPARENT);

                // Show the window in the taskbar
                ShowInTaskbar = true;
            }
        }

        public void SetDesktop(IntPtr desktop) {
            SetThreadDesktop(desktop);
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