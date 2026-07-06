using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PrivacyDots
{
    // Borderless per-pixel-alpha layered window. Click-through (WS_EX_TRANSPARENT),
    // never activated, hidden from Alt-Tab, and periodically re-asserted as topmost
    // so other "always on top" windows cannot bury it.
    public class OverlayForm : Form
    {
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;
        const int WS_EX_TOOLWINDOW = 0x80;
        const int WS_EX_NOACTIVATE = 0x8000000;
        const int WS_EX_TOPMOST = 0x8;

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOMOVE = 0x2;
        const uint SWP_NOSIZE = 0x1;
        const uint SWP_NOACTIVATE = 0x10;

        const int ULW_ALPHA = 2;
        const byte AC_SRC_OVER = 0;
        const byte AC_SRC_ALPHA = 1;

        [StructLayout(LayoutKind.Sequential)]
        struct POINT { public int x; public int y; }

        [StructLayout(LayoutKind.Sequential)]
        struct SIZE { public int cx; public int cy; }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
            IntPtr hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        public static readonly Color CameraColor = Color.FromArgb(52, 199, 89);   // green
        public static readonly Color MicColor = Color.FromArgb(255, 149, 0);      // orange

        bool _visibleNow;

        public OverlayForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "PrivacyDotsOverlay";
            // start off-screen and tiny; UpdateLayeredWindow controls real bounds
            Bounds = new Rectangle(-32000, -32000, 1, 1);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST;
                return cp;
            }
        }

        public void UpdateDots(bool cameraOn, bool micOn, AppSettings settings)
        {
            if (!cameraOn && !micOn)
            {
                if (_visibleNow)
                {
                    Hide();
                    _visibleNow = false;
                }
                return;
            }

            int d = settings.DotSize;
            int gap = Math.Max(4, d / 2);
            int pad = Math.Max(3, d / 3);          // room for the soft shadow
            int count = (cameraOn && micOn) ? 2 : 1;
            int w = count * d + (count - 1) * gap + pad * 2;
            int h = d + pad * 2;

            using (Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    int x = pad;
                    if (cameraOn)
                    {
                        DrawDot(g, x, pad, d, CameraColor);
                        x += d + gap;
                    }
                    if (micOn)
                    {
                        DrawDot(g, x, pad, d, MicColor);
                    }
                }

                Point pos = ComputePosition(w, h, settings);
                PushBitmap(bmp, pos);
            }

            if (!_visibleNow)
            {
                Show();
                _visibleNow = true;
            }
            ReassertTopMost();
        }

        static void DrawDot(Graphics g, float x, float y, float d, Color color)
        {
            // soft dark halo so the dot stays visible over any background
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
            {
                g.FillEllipse(shadow, x - 1.5f, y - 1f, d + 3f, d + 3.5f);
            }
            using (SolidBrush fill = new SolidBrush(color))
            {
                g.FillEllipse(fill, x, y, d, d);
            }
            // small specular highlight for a "light" look
            using (SolidBrush highlight = new SolidBrush(Color.FromArgb(95, 255, 255, 255)))
            {
                g.FillEllipse(highlight, x + d * 0.22f, y + d * 0.14f, d * 0.42f, d * 0.32f);
            }
        }

        static Point ComputePosition(int w, int h, AppSettings s)
        {
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            int x, y;
            switch (s.Position)
            {
                case DotPosition.TopLeft:
                    x = area.Left + s.Margin;
                    y = area.Top + s.Margin;
                    break;
                case DotPosition.TopCenter:
                    x = area.Left + (area.Width - w) / 2;
                    y = area.Top + s.Margin;
                    break;
                case DotPosition.BottomLeft:
                    x = area.Left + s.Margin;
                    y = area.Bottom - h - s.Margin;
                    break;
                case DotPosition.BottomRight:
                    x = area.Right - w - s.Margin;
                    y = area.Bottom - h - s.Margin;
                    break;
                case DotPosition.TopRight:
                default:
                    x = area.Right - w - s.Margin;
                    y = area.Top + s.Margin;
                    break;
            }
            return new Point(x, y);
        }

        void PushBitmap(Bitmap bmp, Point position)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;
            try
            {
                hBitmap = bmp.GetHbitmap(Color.FromArgb(0));
                oldBitmap = SelectObject(memDc, hBitmap);

                SIZE size = new SIZE();
                size.cx = bmp.Width;
                size.cy = bmp.Height;
                POINT src = new POINT();
                POINT dst = new POINT();
                dst.x = position.X;
                dst.y = position.Y;
                BLENDFUNCTION blend = new BLENDFUNCTION();
                blend.BlendOp = AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = 255;
                blend.AlphaFormat = AC_SRC_ALPHA;

                UpdateLayeredWindow(Handle, screenDc, ref dst, ref size, memDc, ref src, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                if (oldBitmap != IntPtr.Zero) SelectObject(memDc, oldBitmap);
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                DeleteDC(memDc);
                ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        public void ReassertTopMost()
        {
            if (IsHandleCreated)
            {
                SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }
    }
}
