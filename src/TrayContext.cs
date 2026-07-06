using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PrivacyDots
{
    public class TrayContext : ApplicationContext
    {
        [DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr handle);

        NotifyIcon _tray;
        OverlayForm _overlay;
        AppSettings _settings;
        Timer _pollTimer;
        Timer _topMostTimer;
        ToolStripMenuItem _startupItem;

        IntPtr _currentIconHandle = IntPtr.Zero;
        int _lastIconState = -1;

        DateTime _testUntil = DateTime.MinValue;
        bool _previewMode;                 // settings dialog open: force dots visible
        bool _lastMic, _lastCam;
        Rectangle _lastArea;
        int _lastSize, _lastMargin;
        DotPosition _lastPosition;
        bool _first = true;

        public TrayContext(string[] args)
        {
            _settings = AppSettings.Load();

            _overlay = new OverlayForm();
            _overlay.Show(); // stays invisible until dots are drawn

            _tray = new NotifyIcon();
            _tray.Text = "Privacy Dots";
            _tray.ContextMenuStrip = BuildMenu();
            _tray.DoubleClick += delegate { OpenSettings(); };
            SetTrayIcon(false, false);
            _tray.Visible = true;

            _pollTimer = new Timer();
            _pollTimer.Interval = 700;
            _pollTimer.Tick += delegate { Poll(); };
            _pollTimer.Start();

            _topMostTimer = new Timer();
            _topMostTimer.Interval = 2000;
            _topMostTimer.Tick += delegate { _overlay.ReassertTopMost(); };
            _topMostTimer.Start();

            foreach (string a in args)
            {
                if (string.Equals(a, "--test", StringComparison.OrdinalIgnoreCase))
                {
                    _testUntil = DateTime.UtcNow.AddSeconds(30);
                }
            }

            Poll();
        }

        ContextMenuStrip BuildMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Settings...");
            settingsItem.Click += delegate { OpenSettings(); };
            menu.Items.Add(settingsItem);

            ToolStripMenuItem testItem = new ToolStripMenuItem("Show test dots (5 s)");
            testItem.Click += delegate
            {
                _testUntil = DateTime.UtcNow.AddSeconds(5);
                Poll();
            };
            menu.Items.Add(testItem);

            _startupItem = new ToolStripMenuItem("Start with Windows");
            _startupItem.Checked = AppSettings.GetRunAtStartup();
            _startupItem.Click += delegate
            {
                bool enable = !_startupItem.Checked;
                AppSettings.SetRunAtStartup(enable);
                _startupItem.Checked = AppSettings.GetRunAtStartup();
            };
            menu.Items.Add(_startupItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem aboutItem = new ToolStripMenuItem("About");
            aboutItem.Click += delegate
            {
                MessageBox.Show(
                    "Privacy Dots 1.1\n" +
                    "Developed by Hiteshwar Singh\n\n" +
                    "Shows an always-on-top indicator when your camera (green) or microphone (orange) is in use.",
                    "About Privacy Dots", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            menu.Items.Add(aboutItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += delegate { ExitThread(); };
            menu.Items.Add(exitItem);

            return menu;
        }

        SettingsForm _settingsForm;

        void OpenSettings()
        {
            if (_settingsForm != null && !_settingsForm.IsDisposed)
            {
                if (_settingsForm.WindowState == FormWindowState.Minimized)
                {
                    _settingsForm.WindowState = FormWindowState.Normal;
                }
                _settingsForm.Activate();
                _settingsForm.BringToFront();
                return;
            }

            _previewMode = true;
            Poll();
            _settingsForm = new SettingsForm(_settings, new Action(OnSettingsChanged));
            _settingsForm.FormClosed += delegate
            {
                _settingsForm = null;
                _previewMode = false;
                _startupItem.Checked = AppSettings.GetRunAtStartup();
                Poll();
            };
            _settingsForm.Show();
        }

        void OnSettingsChanged()
        {
            _first = true; // force re-render with new size/position
            Poll();
        }

        void Poll()
        {
            bool mic = UsageMonitor.IsMicrophoneInUse();
            bool cam = UsageMonitor.IsCameraInUse();

            bool forced = _previewMode || DateTime.UtcNow < _testUntil;
            bool showMic = mic || forced;
            bool showCam = cam || forced;

            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            bool changed = _first
                || showMic != _lastMic || showCam != _lastCam
                || area != _lastArea
                || _settings.DotSize != _lastSize
                || _settings.Margin != _lastMargin
                || _settings.Position != _lastPosition;

            if (changed)
            {
                _overlay.UpdateDots(showCam, showMic, _settings);
                _lastMic = showMic;
                _lastCam = showCam;
                _lastArea = area;
                _lastSize = _settings.DotSize;
                _lastMargin = _settings.Margin;
                _lastPosition = _settings.Position;
                _first = false;

                SetTrayIcon(showCam, showMic);
                _tray.Text = BuildTooltip(cam, mic);
            }
        }

        static string BuildTooltip(bool cam, bool mic)
        {
            if (cam && mic) return "Privacy Dots — camera and microphone in use";
            if (cam) return "Privacy Dots — camera in use";
            if (mic) return "Privacy Dots — microphone in use";
            return "Privacy Dots — camera and mic idle";
        }

        void SetTrayIcon(bool cam, bool mic)
        {
            int state = (cam ? 1 : 0) | (mic ? 2 : 0);
            if (state == _lastIconState) return;
            _lastIconState = state;

            using (Bitmap bmp = new Bitmap(32, 32, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    Color camColor = cam ? OverlayForm.CameraColor : Color.FromArgb(120, 120, 120);
                    Color micColor = mic ? OverlayForm.MicColor : Color.FromArgb(120, 120, 120);
                    using (SolidBrush b = new SolidBrush(camColor)) g.FillEllipse(b, 1f, 9f, 13f, 13f);
                    using (SolidBrush b = new SolidBrush(micColor)) g.FillEllipse(b, 18f, 9f, 13f, 13f);
                }

                IntPtr hIcon = bmp.GetHicon();
                Icon icon = Icon.FromHandle(hIcon);
                _tray.Icon = icon;
                if (_currentIconHandle != IntPtr.Zero)
                {
                    DestroyIcon(_currentIconHandle);
                }
                _currentIconHandle = hIcon;
            }
        }

        protected override void ExitThreadCore()
        {
            if (_pollTimer != null) _pollTimer.Stop();
            if (_topMostTimer != null) _topMostTimer.Stop();
            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
            }
            if (_currentIconHandle != IntPtr.Zero)
            {
                DestroyIcon(_currentIconHandle);
                _currentIconHandle = IntPtr.Zero;
            }
            if (_overlay != null) _overlay.Close();
            base.ExitThreadCore();
        }
    }
}
