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
using System.Numerics;

namespace Autosu {
    public partial class DebugOverlay : Form {
        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80 /* WS_EX_TOOLWINDOW */;
                cp.ExStyle |= 0x8 /* WS_EX_TOPMOST */;
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            //empty implementation
        }

        public DebugOverlay() {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            // Set the form's size and position
            this.FormBorderStyle = FormBorderStyle.None;
            // Set the form's transparency color and make it semi-transparent
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.TopMost = true;
            this.ShowInTaskbar = false;

        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            // Set the outline color and width
            Pen pen = new Pen(Color.Red, 2);

            // Calculate the box dimensions based on screen size
            int boxHeight = (int) (Screen.PrimaryScreen.Bounds.Height * 0.8);
            int boxWidth = (int) (boxHeight * 4 / 3);
            int x = (Screen.PrimaryScreen.Bounds.Width - boxWidth) / 2;
            int y = (Screen.PrimaryScreen.Bounds.Height - boxHeight) / 2;

            // Draw the rectangle with the calculated dimensions and make the inside transparent
            Rectangle rect = new Rectangle(x, y, boxWidth, boxHeight);
            e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            e.Graphics.FillRectangle(new SolidBrush(Color.Transparent), rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);

            // Draw two green circles at a specific position
            SolidBrush brush = new SolidBrush(Color.Green);
            int circleRadius = 30;

            Vector2 pos1 = APUtil.OsuPixelToScreen(new Vector2(0, 0));
            Vector2 pos2 = APUtil.OsuPixelToScreen(new Vector2(511, 383));

            e.Graphics.FillEllipse(brush, pos1.X - circleRadius, pos1.Y - circleRadius, 2 * circleRadius, 2 * circleRadius);
            e.Graphics.FillEllipse(brush, pos2.X - circleRadius, pos2.Y - circleRadius, 2 * circleRadius, 2 * circleRadius);

            // Dispose of the pen and brush objects
            pen.Dispose();
            brush.Dispose();

            // Dispose of the pen object
            pen.Dispose();
        }
    }

}