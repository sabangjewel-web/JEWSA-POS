using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JewSaPOS.Domain;

namespace JewSaPOS.WinForms
{
    public sealed class MainForm : Form
    {
        private readonly User user;
        private readonly Panel content = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Bg };
        private readonly Dictionary<string, NavItem> navButtons = new Dictionary<string, NavItem>();
        private Control currentView;

        public MainForm(User u)
        {
            user = u;
            Text = "Enterprise Store - POS & Inventory";
            Font = Ui.FontBody;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 700);
            MinimumSize = new Size(1000, 600);
            WindowState = FormWindowState.Maximized;
            Ui.DarkForm(this);

            var sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = Ui.Sidebar };

            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                BackColor = Ui.Sidebar, Padding = new Padding(0, 6, 0, 0)
            };
            AddNav(nav, "Dashboard", "Dashboard");
            AddNav(nav, "Inventory", "Inventory");

            var logo = new PaintPanel { Dock = DockStyle.Top, Height = 84, BackColor = Ui.Sidebar };
            logo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var mark = new Rectangle(24, 24, 38, 38);
                using (var path = Ui.Round(new Rectangle(mark.X, mark.Y, mark.Width - 1, mark.Height - 1), 10))
                using (var b = new SolidBrush(Ui.Accent))
                    g.FillPath(b, path);
                TextRenderer.DrawText(g, "E", Ui.FontSemi11, mark, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, "Enterprise POS", Ui.FontSemi11, new Rectangle(72, 24, 160, 20), Ui.Text,
                    TextFormatFlags.Left | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, "Store management", Ui.FontSmall, new Rectangle(72, 45, 160, 16), Ui.Muted,
                    TextFormatFlags.Left | TextFormatFlags.NoPadding);
            };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 132, BackColor = Ui.Sidebar };
            var who = new PaintPanel { Dock = DockStyle.Fill, BackColor = Ui.Sidebar };
            who.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var circle = new Rectangle(24, 18, 42, 42);
                using (var b = new SolidBrush(Ui.Mix(Ui.Accent, Ui.Sidebar, 0.28f)))
                    g.FillEllipse(b, circle);
                TextRenderer.DrawText(g, Initials(user.FullName), Ui.FontSemi, circle, Ui.AccentLight,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, user.FullName, Ui.FontSemi, new Rectangle(76, 21, 150, 18), Ui.Text,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, user.RoleName, Ui.FontSmall, new Rectangle(76, 41, 150, 16), Ui.Muted,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            };
            var logout = new NavItem { Text = "Log out", Dock = DockStyle.Top };
            logout.Click += (s, e) => Close();
            bottom.Controls.Add(who);      // Fill first
            bottom.Controls.Add(logout);   // then Top

            sidebar.Controls.Add(nav);
            sidebar.Controls.Add(logo);
            sidebar.Controls.Add(bottom);

            Controls.Add(content);   // Fill first
            Controls.Add(sidebar);   // then Left

            Navigate("Dashboard");
        }

        private static string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private void AddNav(FlowLayoutPanel nav, string text, string key)
        {
            var b = new NavItem { Text = text };
            b.Click += (s, e) => Navigate(key);
            navButtons[key] = b;
            nav.Controls.Add(b);
        }

        private void Navigate(string key)
        {
            foreach (var kv in navButtons) kv.Value.Active = kv.Key == key;

            Control view;
            if (key == "Inventory") view = new InventoryView(user);
            else view = new DashboardView(user);

            if (currentView != null) { content.Controls.Remove(currentView); currentView.Dispose(); }
            currentView = view;
            view.Dock = DockStyle.Fill;
            content.Controls.Add(view);
        }
    }
}
