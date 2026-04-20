using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VRAMVacuum
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly string[] _targetProcesses = new[]
        {
            "chrome", "msedge", "discord", "spotify", "slack", "brave", "steamwebhelper"
        };

        private Label _statusLabel;
        private Button _vacuumButton;

        private const string Base64Icon = 
            "AAABAAEAEBAAAAEAIABoBAAAFgAAACgAAAAQAAAAIAAAAAEAIAAAAAAAAAQAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA" +
            "/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8A" +
            "AAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA" +
            "/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8A" +
            "AAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA" +
            "/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8A" +
            "AAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA" +
            "/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAA" +
            "AAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAA" +
            "AP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/" +
            "AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAA" +
            "AP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/" +
            "AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAA" +
            "AP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/" +
            "AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/AAAA/wAA" +
            "AP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAAAP8AAAD/" +
            "AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/AAAA/wAA" +
            "AP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAD/" +
            "AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8A" +
            "AA==";

        public MainForm()
        {
            InitializeComponent();
            
            _trayIcon = new NotifyIcon
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true,
                Text = "VRAM Vacuum"
            };

            LoadIcon();

            _trayIcon.ContextMenuStrip.Items.Add("Open Dashboard", null, (s, e) => ShowForm());
            _trayIcon.ContextMenuStrip.Items.Add("Vacuum VRAM", null, (s, e) => VacuumVram());
            _trayIcon.ContextMenuStrip.Items.Add("-");
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => ExitApp());

            _trayIcon.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) ShowForm(); };
        }

        private void InitializeComponent()
        {
            this.Text = "VRAM Vacuum";
            this.Size = new Size(320, 240);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.ForeColor = Color.Orange;
            this.StartPosition = FormStartPosition.CenterScreen;

            var titleLabel = new Label
            {
                Text = "VRAM VACUUM",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                ForeColor = Color.Orange
            };

            _statusLabel = new Label
            {
                Text = "Ready to reclaim GPU memory.",
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.Gray
            };

            _vacuumButton = new Button
            {
                Text = "VACUUM",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Width = 200,
                Height = 50,
                Cursor = Cursors.Hand
            };
            _vacuumButton.FlatAppearance.BorderSize = 0;
            _vacuumButton.Location = new Point((this.ClientSize.Width - _vacuumButton.Width) / 2, 110);
            _vacuumButton.Click += (s, e) => VacuumVram();

            var footerLabel = new Label
            {
                Text = "Minimal. Ruthless. Instant.",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 30,
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            this.Controls.Add(_vacuumButton);
            this.Controls.Add(_statusLabel);
            this.Controls.Add(titleLabel);
            this.Controls.Add(footerLabel);
        }

        private void LoadIcon()
        {
            try
            {
                using var ms = new MemoryStream(Convert.FromBase64String(Base64Icon));
                var icon = new Icon(ms);
                this.Icon = icon;
                _trayIcon.Icon = icon;
            }
            catch
            {
                using var bmp = new Bitmap(16, 16);
                using var g = Graphics.FromImage(bmp);
                g.Clear(Color.Black);
                using var pen = new Pen(Color.Orange, 2);
                g.DrawRectangle(pen, 1, 1, 13, 13);
                var hIcon = bmp.GetHicon();
                var icon = Icon.FromHandle(hIcon);
                this.Icon = icon;
                _trayIcon.Icon = icon;
            }
        }

        private void VacuumVram()
        {
            _statusLabel.Text = "Vacuuming...";
            _statusLabel.ForeColor = Color.Orange;
            _vacuumButton.Enabled = false;

            int killedCount = 0;
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                if (_targetProcesses.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        process.Kill();
                        killedCount++;
                    }
                    catch { }
                }
            }

            _statusLabel.Text = $"Terminated {killedCount} tasks.";
            _statusLabel.ForeColor = Color.LimeGreen;
            _vacuumButton.Enabled = true;

            _trayIcon.ShowBalloonTip(3000, "VRAM Vacuum", $"Vacuumed: {killedCount} background tasks terminated.", ToolTipIcon.Info);
        }

        private void ShowForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                _trayIcon.ShowBalloonTip(2000, "VRAM Vacuum", "App is still running in the system tray.", ToolTipIcon.Info);
            }
            base.OnFormClosing(e);
        }

        private void ExitApp()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.ExitThread();
            Environment.Exit(0);
        }
    }
}
