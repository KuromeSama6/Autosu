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

namespace Autosu {
    public partial class SongSelectPage : Form {
        public ChromiumWebBrowser browser = new();
        public static SongSelectPage instance;

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