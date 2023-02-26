using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Autosu.Utils {
    public static class CommonUtil {
        [DllImport("user32.dll")]
        static extern IntPtr GetProcessWindowStation();
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetUserObjectInformation(IntPtr hObj, int nIndex, IntPtr pvInfo, int nLength, out int lpnLengthNeeded);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr OpenDesktop(string lpszDesktop, uint dwFlags, bool fInherit, uint dwDesiredAccess);
        [DllImport("user32.dll")]
        static extern IntPtr GetThreadDesktop(int dwThreadId);

        public static string ParsePath(string path) {
            return $"{Application.StartupPath}{path.Replace("/", "\\")}";
        }

        public static void BrowserConsoleLog(ChromiumWebBrowser browser, string message) {
            browser.ExecuteScriptAsync($"console.log(`{message}`)");
        }

        public static void CallBrowserFunction(ChromiumWebBrowser browser, string name, object param) {
            browser.ExecuteScriptAsync($"{name}(JSON.parse('{JsonConvert.SerializeObject(param)}'))");
        }

        public static Task<JavascriptResponse> CallBrowserFunctionWithCallback(ChromiumWebBrowser browser, string name, object param) {
            return browser.EvaluateScriptAsync($"{name}(JSON.parse('{JsonConvert.SerializeObject(param)}'))");
        }

        public static float RandomFloatRange(float min, float max) {
            System.Random random = new System.Random();
            double val = (random.NextDouble() * (max - min) + min);
            return (float) val;
        }

        public static float Normalize(float value, float min, float max) {
            value = Math.Clamp(value, min, max);
            if (min == max) {
                // Avoid division by zero
                return 0f;
            } else {
                return (value - min) / (max - min);
            }
        }
        public static List<string> ExtractStringSection(string input, string sectionName) {
            // Split the input into lines
            string[] lines = input.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            // Find the line with the section name
            int sectionIndex = Array.IndexOf(lines, sectionName);
            if (sectionIndex == -1) {
                return null;  // section not found
            }

            // Find the next blank line after the section name
            int endIndex = Array.IndexOf(lines, "", sectionIndex + 1);
            if (endIndex == -1) {
                endIndex = lines.Length;
            }

            // Join the lines between the section name and the blank line
            //List<string> ret = new(string.Join(Environment.NewLine, lines, sectionIndex, endIndex - sectionIndex).Split(Environment.NewLine));
            List<string> ret = new();
            foreach (string line in string.Join(Environment.NewLine, lines, sectionIndex, endIndex - sectionIndex).Split(Environment.NewLine)) {
                ret.Add(line);
            }

            return ret;
        }

        public static Dictionary<string, string> SerializeStringSection(List<string> input) {
            Dictionary<string, string> ret = new();
            foreach (string line in input) {
                if (line.Split(':').Length > 1) {
                    ret.Add(line.Split(':')[0], line.Split(':')[1]);
                    
                }
            }
            return ret;
        }

        public static IntPtr GetDesktopHandle(Process proc) {
            IntPtr hWinSta = GetProcessWindowStation();
            IntPtr hDesk = IntPtr.Zero;

            int needed;
            if (GetUserObjectInformation(hWinSta, 2, IntPtr.Zero, 0, out needed) == 0) {
                IntPtr buffer = Marshal.AllocHGlobal(needed);
                if (GetUserObjectInformation(hWinSta, 2, buffer, needed, out needed) != 0) {
                    string desktop = Marshal.PtrToStringUni(buffer);
                    hDesk = OpenDesktop(desktop, 0, false, 0);
                }
                Marshal.FreeHGlobal(buffer);
            }

            if (hDesk != IntPtr.Zero) {
                // Retrieve the desktop associated with the input process
                IntPtr hProcessDesk = GetThreadDesktop(proc.Threads[0].Id);
                if (hProcessDesk != IntPtr.Zero) {
                    MessageBox.Show(string.Format("Desktop handle: {0}", hProcessDesk));
                } else {
                    MessageBox.Show("Failed to get desktop handle for process");
                }
            } else {
                MessageBox.Show("Failed to get desktop handle for window station");
            }

            return hDesk;
        }

        public static async void DelayedCall(Action call, float delay) {
            await Task.Delay((int) (delay * 1000));
            call.Invoke();
        }

    }
}
