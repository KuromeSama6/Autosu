using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Autosu.Utils {
    public static class CommonUtil {
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

    }
}
