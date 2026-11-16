using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JewSaPOS.WinForms
{
    internal static class Ui
    {
        // ---- Palette (change these to re-theme the whole app) ----
        public static readonly Color Bg = Color.FromArgb(15, 18, 28);
        public static readonly Color Surface = Color.FromArgb(22, 27, 40);
        public static readonly Color Surface2 = Color.FromArgb(30, 36, 54);
        public static readonly Color Border = Color.FromArgb(42, 50, 72);
        public static readonly Color Text = Color.FromArgb(235, 239, 247);
        public static readonly Color Muted = Color.FromArgb(138, 149, 173);
        public static readonly Color Accent = Color.FromArgb(99, 102, 241);
        public static readonly Color AccentLight = Color.FromArgb(165, 180, 252);
        public static readonly Color AccentDark = Accent;
        public static readonly Color Success = Color.FromArgb(52, 211, 153);
        public static readonly Color Warning = Color.FromArgb(251, 191, 36);
        public static readonly Color Blue = Color.FromArgb(96, 165, 250);
        public static readonly Color Danger = Color.FromArgb(239, 98, 108);
        public static readonly Color Neutral = Color.FromArgb(47, 56, 82);
        public static readonly Color Sidebar = Color.FromArgb(11, 14, 23);
        public static readonly Color SidebarSel = Color.FromArgb(27, 33, 56);
        public static readonly Color RowSelected = Color.FromArgb(36, 42, 74);

        // ---- Fonts ----
        public static readonly Font FontBody = new Font("Segoe UI", 9.5f);
        public static readonly Font FontSmall = new Font("Segoe UI", 8.5f);
        public static readonly Font FontSemi = new Font("Segoe UI Semibold", 9.5f);
        public static readonly Font FontSemi11 = new Font("Segoe UI Semibold", 11f);
        public static readonly Font FontH1 = new Font("Segoe UI Semibold", 20f);
        public static readonly Font FontBig = new Font("Segoe UI Semibold", 22f);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        public static string Money(decimal v) { return "\u20B1" + v.ToString("N2"); }

        public static Color Mix(Color a, Color b, float t)
        {
            return Color.FromArgb(
                (int)(a.R * t + b.R * (1 - t)),
                (int)(a.G * t + b.G * (1 - t)),
                (int)(a.B * t + b.B * (1 - t)));
        }

        public static Color Shade(Color c, float amt)
        {
            if (amt >= 0)
                return Color.FromArgb(c.A, (int)(c.R + (255 - c.R) * amt), (int)(c.G + (255 - c.G) * amt), (int)(c.B + (255 - c.B) * amt));
            return Color.FromArgb(c.A, (int)(c.R * (1 + amt)), (int)(c.G * (1 + amt)), (int)(c.B * (1 + amt)));
        }

        public static GraphicsPath Round(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void DarkForm(Form f)
        {
            f.BackColor = Bg;
            f.ForeColor = Text;
            f.HandleCreated += (s, e) =>
            {
                try
                {
                    int v = 1;
                    if (DwmSetWindowAttribute(f.Handle, 20, ref v, sizeof(int)) != 0)
                        DwmSetWindowAttribute(f.Handle, 19, ref v, sizeof(int));
                }
                catch { }
            };
        }

        public static void Placeholder(TextBox tb, string text)
        {
            Action set = () => { try { SendMessage(tb.Handle, 0x1501, (IntPtr)1, text); } catch { } };
            if (tb.IsHandleCreated) set(); else tb.HandleCreated += (s, e) => set();
        }

        public static void StyleInput(params Control[] controls)
        {
            foreach (var c in controls)
            {
                c.BackColor = Surface2;
                c.ForeColor = Text;
                var tb = c as TextBox; if (tb != null) tb.BorderStyle = BorderStyle.FixedSingle;
                var nud = c as NumericUpDown; if (nud != null) nud.BorderStyle = BorderStyle.FixedSingle;
                var cb = c as ComboBox; if (cb != null) cb.FlatStyle = FlatStyle.Flat;
            }
        }

        public static Card Field(TextBox tb, int width, int height)
        {
            var card = new Card { Radius = 8, BackColor = Surface2, EdgeColor = Border, Size = new Size(width, height), Padding = new Padding(0) };
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = Surface2;
            tb.ForeColor = Text;
            tb.Width = width - 24;
            tb.Location = new Point(12, Math.Max(2, (height - tb.PreferredHeight) / 2));
            card.Controls.Add(tb);
            tb.Enter += (s, e) => { card.EdgeColor = Accent; card.Invalidate(); };
            tb.Leave += (s, e) => { card.EdgeColor = Border; card.Invalidate(); };
            card.Click += (s, e) => tb.Focus();
            return card;
        }

        public static Label Lbl(string text, bool muted)
        {
            return new Label
            {
                Text = text, AutoSize = true, ForeColor = muted ? Muted : Text,
                Margin = new Padding(3, 10, 6, 0), BackColor = Color.Transparent
            };
        }

        public static void StyleGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.AutoGenerateColumns = false;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.RowHeadersVisible = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.BackgroundColor = Surface;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Border;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Surface;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Surface;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Muted;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5f);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 38;
            g.RowTemplate.Height = 42;
            g.DefaultCellStyle.BackColor = Surface;
            g.DefaultCellStyle.ForeColor = Text;
            g.DefaultCellStyle.Font = FontBody;
            g.DefaultCellStyle.SelectionBackColor = RowSelected;
            g.DefaultCellStyle.SelectionForeColor = Text;
            g.DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
        }

        public static void Col(DataGridView g, string header, string prop, float weight, string format, bool right)
        {
            var c = new DataGridViewTextBoxColumn { HeaderText = header, DataPropertyName = prop, FillWeight = weight };
            if (format != null) c.DefaultCellStyle.Format = format;
            if (right)
            {
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            g.Columns.Add(c);
        }

        public static Button Btn(string text, Color back)
        {
            var b = new PillButton { Text = text, Fill = back, Margin = new Padding(4, 2, 4, 2) };
            int w = TextRenderer.MeasureText(text, b.Font).Width + 36;
            b.Size = new Size(Math.Max(w, 80), 36);
            return b;
        }

        public static void DrawPill(Graphics g, Rectangle cell, string text, Color fg, Color bg)
        {
            Size ts = TextRenderer.MeasureText(g, text, FontSmall, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            int w = ts.Width + 22, h = 22;
            var r = new Rectangle(cell.X + 10, cell.Y + (cell.Height - h) / 2, w, h);
            var old = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Round(new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1), 10))
            using (var b = new SolidBrush(bg)) g.FillPath(b, path);
            g.SmoothingMode = old;
            TextRenderer.DrawText(g, text, FontSmall, r, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        public static void Error(string msg) { MessageBox.Show(msg, "Enterprise POS", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        public static void Info(string msg) { MessageBox.Show(msg, "Enterprise POS", MessageBoxButtons.OK, MessageBoxIcon.Information); }
    }

    internal class PaintPanel : Panel
    {
        public PaintPanel() { DoubleBuffered = true; ResizeRedraw = true; }
    }

    internal class Card : Panel
    {
        public int Radius = 12;
        public Color EdgeColor = Ui.Border;

        public Card()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Ui.Surface;
            Padding = new Padding(16);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : Ui.Bg);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Ui.Round(new Rectangle(0, 0, Width - 1, Height - 1), Radius))
            using (var b = new SolidBrush(BackColor))
            using (var pen = new Pen(EdgeColor))
            {
                g.FillPath(b, path);
                g.DrawPath(pen, path);
            }
        }
    }

    internal sealed class PillButton : Button
    {
        private bool hover, down;
        public Color Fill { get; set; }

        public PillButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = Ui.FontSemi;
            ForeColor = Color.White;
            Fill = Ui.Accent;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : Ui.Bg);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color c = Enabled ? Fill : Ui.Neutral;
            if (Enabled && down) c = Ui.Shade(c, -0.15f);
            else if (Enabled && hover) c = Ui.Shade(c, 0.12f);
            using (var path = Ui.Round(new Rectangle(0, 0, Width - 1, Height - 1), 8))
            using (var b = new SolidBrush(c))
                g.FillPath(b, path);
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, Enabled ? ForeColor : Ui.Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }

    internal sealed class NavItem : Button
    {
        private bool hover;
        private bool active;
        public bool Active { get { return active; } set { active = value; Invalidate(); } }

        public NavItem()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = Ui.FontSemi;
            Margin = new Padding(0);
            Size = new Size(240, 46);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Ui.Sidebar);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(12, 3, Width - 24, Height - 7);
            if (active || hover)
            {
                using (var path = Ui.Round(r, 8))
                using (var b = new SolidBrush(active ? Ui.SidebarSel : Color.FromArgb(19, 24, 40)))
                    g.FillPath(b, path);
            }
            if (active)
            {
                using (var path = Ui.Round(new Rectangle(r.X + 6, r.Y + 11, 3, r.Height - 22), 1))
                using (var b = new SolidBrush(Ui.Accent))
                    g.FillPath(b, path);
            }
            TextRenderer.DrawText(g, Text, Font, new Rectangle(r.X + 22, r.Y, r.Width - 22, r.Height),
                active ? Color.White : Ui.Muted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }
}
