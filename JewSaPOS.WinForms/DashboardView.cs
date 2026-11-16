using System;
using System.Drawing;
using System.Windows.Forms;
using JewSaPOS.Domain;
using JewSaPOS.Services;

namespace JewSaPOS.WinForms
{
    public sealed class DashboardView : UserControl
    {
        private readonly User user;
        private readonly Label lblSales, lblOrders, lblLow, lblActive;
        private readonly DataGridView grid = new DataGridView();

        public DashboardView(User u)
        {
            user = u;
            BackColor = Ui.Bg;
            ForeColor = Ui.Text;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                Padding = new Padding(32, 24, 32, 24), BackColor = Ui.Bg
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Bg };
            header.Controls.Add(new Label
            {
                Text = Greeting() + ", " + FirstName(user.FullName), Dock = DockStyle.Fill, ForeColor = Ui.Text,
                TextAlign = ContentAlignment.BottomLeft, Font = Ui.FontH1
            });
            header.Controls.Add(new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM d, yyyy"), Dock = DockStyle.Bottom, Height = 26,
                ForeColor = Ui.Muted, TextAlign = ContentAlignment.MiddleLeft
            });
            root.Controls.Add(header, 0, 0);

            var tiles = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Ui.Bg };
            for (int i = 0; i < 4; i++) tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tiles.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var t1 = Stat("Today's sales", "\u20B1", Ui.Success, out lblSales);
            var t2 = Stat("Orders today", "#", Ui.Blue, out lblOrders);
            var t3 = Stat("Low-stock items", "!", Ui.Danger, out lblLow);
            var t4 = Stat("Active products", "+", Ui.AccentLight, out lblActive);
            t1.Margin = new Padding(0, 14, 14, 6);
            t2.Margin = new Padding(0, 14, 14, 6);
            t3.Margin = new Padding(0, 14, 14, 6);
            t4.Margin = new Padding(0, 14, 0, 6);
            tiles.Controls.Add(t1, 0, 0);
            tiles.Controls.Add(t2, 1, 0);
            tiles.Controls.Add(t3, 2, 0);
            tiles.Controls.Add(t4, 3, 0);
            root.Controls.Add(tiles, 0, 1);

            var card = new Card { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 8, 8), Margin = new Padding(0, 8, 0, 0) };
            Ui.StyleGrid(grid);
            Ui.Col(grid, "SKU", "Sku", 20, null, false);
            Ui.Col(grid, "PRODUCT", "Name", 60, null, false);
            Ui.Col(grid, "IN STOCK", "StockQty", 10, null, true);
            Ui.Col(grid, "REORDER LEVEL", "ReorderLevel", 12, null, true);
            grid.Columns[0].HeaderText = "SKU";
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 2) { e.CellStyle.ForeColor = Ui.Danger; e.CellStyle.Font = Ui.FontSemi; }
            };
            card.Controls.Add(grid);   // Fill first
            card.Controls.Add(new Label
            {
                Text = "Low-stock items", Dock = DockStyle.Top, Height = 50, ForeColor = Ui.Text, Font = Ui.FontSemi11,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0)
            });
            root.Controls.Add(card, 0, 2);

            Controls.Add(root);
            Load += (s, e) => LoadData();
        }

        private static string Greeting()
        {
            int h = DateTime.Now.Hour;
            return h < 12 ? "Good morning" : (h < 18 ? "Good afternoon" : "Good evening");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DashboardView
            // 
            this.Name = "DashboardView";
            this.Size = new System.Drawing.Size(257, 195);
            this.ResumeLayout(false);

        }

        private static string FirstName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            return name.Trim().Split(' ')[0];
        }

        private static Card Stat(string caption, string glyph, Color color, out Label value)
        {
            var card = new Card { Dock = DockStyle.Fill, Padding = new Padding(0) };
            var chipColor = Ui.Mix(color, Ui.Surface, 0.2f);
            var chip = new Card { Radius = 10, Size = new Size(38, 38), Location = new Point(18, 16), BackColor = chipColor, EdgeColor = chipColor, Padding = new Padding(0) };
            chip.Controls.Add(new Label
            {
                Text = glyph, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = color, Font = Ui.FontSemi11
            });
            var cap = new Label { Text = caption, AutoSize = true, ForeColor = Ui.Muted, Location = new Point(68, 26) };
            value = new Label { Text = "-", AutoSize = true, ForeColor = Ui.Text, Location = new Point(16, 58), Font = Ui.FontBig };
            card.Controls.Add(chip);
            card.Controls.Add(cap);
            card.Controls.Add(value);
            return card;
        }

        private void LoadData()
        {
            try
            {
                var svc = new DashboardService();
                var s = svc.Stats();
                lblSales.Text = Ui.Money(s.TodaySales);
                lblOrders.Text = s.TodayOrders.ToString();
                lblLow.Text = s.LowStockCount.ToString();
                lblActive.Text = s.ActiveProducts.ToString();
                grid.DataSource = svc.LowStock();
            }
            catch (PosException ex) { Ui.Error(ex.Message); }
        }
    }
}
