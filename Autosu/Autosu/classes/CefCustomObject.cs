using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.Classes
{
    public class Downstream {
        public ChromiumWebBrowser browser = null;
        public Form form = null;

        public Downstream(ChromiumWebBrowser browser, Form form) {
            this.browser = browser;
            this.form = form;
        }

        public virtual void BrowserReady() { }
        public virtual void OpenDev() {
            browser.ShowDevTools();
        }

    }
}
