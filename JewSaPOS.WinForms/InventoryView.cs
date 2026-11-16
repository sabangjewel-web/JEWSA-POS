using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JewSaPOS.Domain;
using JewSaPOS.Services;

namespace JewSaPOS.WinForms
{
    public sealed class InventoryView : UserControl
    {
        private readonly InventoryService svc;
        private readonly bool canModify;

        private readonly TextBox txtSearch = new TextBox { Font = new Font("Segoe UI", 10f) };
        private readonly CheckBox chkLow = new CheckBox { Text = "Low stock only", AutoSize = true, ForeColor = Ui.Muted, Margin = new Padding(12, 11, 4, 0) };
        private readonly CheckBox chkInactive = new CheckBox { Text = "Show inactive", AutoSize = true, ForeColor = Ui.Muted, Margin = new Padding(8, 11, 12, 0) };
        private readonly DataGridView grid = new DataGridView();
        private readonly DataGridView history = new DataGridView();
        private readonly Label lblAdjust = new Label { AutoSize = true, ForeColor = Ui.Text, Font = Ui.FontSemi, Margin = new Padding(3, 11, 20, 0), Text = "Select a product" };
        private readonly NumericUpDown nudChange = new NumericUpDown { Minimum = -100000, Maximum = 100000, Width = 80 };
        private readonly ComboBox cboReason = new ComboBox { Width = 200 };
        private Button btnApply;

        public InventoryView(User user)
        {
            svc = new InventoryService(user);
            canModify = svc.CanModify;
            BackColor = Ui.Bg;
            ForeColor = Ui.Text;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5,
                Padding = new Padding(32, 24, 32, 24), BackColor = Ui.Bg
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Bg };
            header.Controls.Add(new Label
            {
                Text = "Inventory", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft,
                Font = Ui.FontH1, ForeColor = Ui.Text
            });
            header.Controls.Add(new Label
            {
                Text = "Manage products and stock levels", Dock = DockStyle.Bottom, Height = 26,
                ForeColor = Ui.Muted, TextAlign = ContentAlignment.MiddleLeft
            });
            root.Controls.Add(header, 0, 0);

            var bar = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Bg };
            var left = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, BackColor = Ui.Bg, Padding = new Padding(0, 6, 0, 0) };
            var searchField = Ui.Field(txtSearch, 280, 38);
            searchField.Margin = new Padding(0, 0, 8, 0);
            Ui.Placeholder(txtSearch, "Search name, SKU, or barcode");
            var btnSearch = Ui.Btn("Search", Ui.Neutral);
            btnSearch.Click += (s, e) => LoadRows(null);
            Ui.StyleInput(nudChange, cboReason);
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadRows(null); } };
            chkLow.CheckedChanged += (s, e) => LoadRows(null);
            chkInactive.CheckedChanged += (s, e) => LoadRows(null);
            left.Controls.Add(searchField);
            left.Controls.Add(btnSearch);
            left.Controls.Add(chkLow);
            left.Controls.Add(chkInactive);
            bar.Controls.Add(left);   // Fill first
            if (canModify)
            {
                var right = new FlowLayoutPanel
                {
                    Dock = DockStyle.Right, AutoSize = true, WrapContents = false, BackColor = Ui.Bg,
                    Padding = new Padding(0, 6, 0, 0)
                };
                var add = Ui.Btn("Add product", Ui.Accent); add.Click += (s, e) => AddProduct();
                var edit = Ui.Btn("Edit", Ui.Neutral); edit.Click += (s, e) => EditProduct();
                var del = Ui.Btn("Delete", Ui.Danger); del.Click += (s, e) => DeleteProduct();
                right.Controls.Add(add); right.Controls.Add(edit); right.Controls.Add(del);
                bar.Controls.Add(right);
            }
            root.Controls.Add(bar, 0, 1);

            var gridCard = new Card { Dock = DockStyle.Fill, Padding = new Padding(8), Margin = new Padding(0, 0, 0, 14) };
            Ui.StyleGrid(grid);
            Ui.Col(grid, "SKU", "Sku", 10, null, false);
            Ui.Col(grid, "BARCODE", "Barcode", 14, null, false);
            Ui.Col(grid, "PRODUCT", "Name", 26, null, false);
            Ui.Col(grid, "CATEGORY", "CategoryName", 14, null, false);
            Ui.Col(grid, "PRICE", "Price", 9, "N2", true);
            Ui.Col(grid, "COST", "Cost", 9, "N2", true);
            Ui.Col(grid, "IN STOCK", "StockQty", 8, null, true);
            Ui.Col(grid, "REORDER", "ReorderLevel", 8, null, true);
            Ui.Col(grid, "STATUS", "Status", 12, null, false);
            grid.SelectionChanged += (s, e) => OnSelectionChanged();
            grid.CellFormatting += GridCellFormatting;
            grid.CellPainting += GridCellPainting;
            grid.CellDoubleClick += (s, e) => { if (canModify && e.RowIndex >= 0) EditProduct(); };
            gridCard.Controls.Add(grid);
            root.Controls.Add(gridCard, 0, 2);

            var histCard = new Card { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 8, 8), Margin = new Padding(0, 0, 0, 14) };
            Ui.StyleGrid(history);
            Ui.Col(history, "WHEN", "CreatedAt", 22, "g", false);
            Ui.Col(history, "CHANGE", "ChangeQty", 10, "+#;-#;0", true);
            Ui.Col(history, "REASON", "Reason", 50, null, false);
            Ui.Col(history, "BY", "UserName", 18, null, false);
            history.RowTemplate.Height = 36;
            history.CellFormatting += HistoryCellFormatting;
            histCard.Controls.Add(history);   // Fill first
            histCard.Controls.Add(new Label
            {
                Text = "Stock history", Dock = DockStyle.Top, Height = 44, ForeColor = Ui.Text, Font = Ui.FontSemi11,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0)
            });
            root.Controls.Add(histCard, 0, 3);

            var adjCard = new Card { Dock = DockStyle.Fill, Padding = new Padding(14, 10, 14, 6) };
            var adj = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, BackColor = Ui.Surface };
            cboReason.Items.AddRange(new object[] { "Restock / delivery", "Damaged", "Expired", "Stock count correction", "Returned to supplier" });
            btnApply = Ui.Btn("Apply adjustment", Ui.Accent);
            btnApply.Click += (s, e) => ApplyAdjustment();
            adj.Controls.Add(lblAdjust);
            adj.Controls.Add(Ui.Lbl("Change (+/-)", true));
            nudChange.Margin = new Padding(3, 8, 16, 0);
            adj.Controls.Add(nudChange);
            adj.Controls.Add(Ui.Lbl("Reason", true));
            cboReason.Margin = new Padding(3, 8, 16, 0);
            adj.Controls.Add(cboReason);
            adj.Controls.Add(btnApply);
            adjCard.Controls.Add(adj);
            adjCard.Enabled = canModify;
            root.Controls.Add(adjCard, 0, 4);

            Controls.Add(root);
            Load += (s, e) => LoadRows(null);
        }

        private Product Selected()
        {
            return grid.CurrentRow == null ? null : grid.CurrentRow.DataBoundItem as Product;
        }

        private void LoadRows(int? selectId)
        {
            try
            {
                grid.DataSource = null;
                var items = svc.Search(txtSearch.Text, chkLow.Checked, chkInactive.Checked);
                grid.DataSource = items;
                if (selectId.HasValue)
                {
                    foreach (DataGridViewRow r in grid.Rows)
                    {
                        var p = r.DataBoundItem as Product;
                        if (p != null && p.ProductId == selectId.Value) { grid.CurrentCell = r.Cells[0]; break; }
                    }
                }
                OnSelectionChanged();
            }
            catch (PosException ex) { Ui.Error(ex.Message); }
        }

        private void OnSelectionChanged()
        {
            var p = Selected();
            if (p == null)
            {
                lblAdjust.Text = "Select a product";
                history.DataSource = null;
                return;
            }
            lblAdjust.Text = p.Name + "  (stock " + p.StockQty + ")";
            try { history.DataSource = svc.History(p.ProductId); }
            catch (PosException) { history.DataSource = null; }
        }

        private void GridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            var p = grid.Rows[e.RowIndex].DataBoundItem as Product;
            if (p == null) return;
            if (!p.IsActive) e.CellStyle.ForeColor = Ui.Muted;
            else if (p.IsLowStock && e.ColumnIndex == 6)
            {
                e.CellStyle.ForeColor = Ui.Danger;
                e.CellStyle.Font = Ui.FontSemi;
            }
        }

        private void GridCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 8) return;
            var p = grid.Rows[e.RowIndex].DataBoundItem as Product;
            if (p == null) return;
            e.PaintBackground(e.ClipBounds, true);
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Border);
            string text; Color fg;
            if (!p.IsActive) { text = "Inactive"; fg = Ui.Muted; }
            else if (p.IsLowStock) { text = "Low stock"; fg = Ui.Danger; }
            else { text = "In stock"; fg = Ui.Success; }
            Ui.DrawPill(e.Graphics, e.CellBounds, text, fg, Ui.Mix(fg, Ui.Surface, 0.18f));
            e.Handled = true;
        }

        private void HistoryCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1 || !(e.Value is int)) return;
            int v = (int)e.Value;
            e.CellStyle.ForeColor = v > 0 ? Ui.Success : (v < 0 ? Ui.Danger : Ui.Text);
            e.CellStyle.Font = Ui.FontSemi;
        }

        private void AddProduct()
        {
            try
            {
                using (var f = new ProductEditorForm(svc, svc.Categories(), null))
                    if (f.ShowDialog(this) == DialogResult.OK) LoadRows(null);
            }
            catch (PosException ex) { Ui.Error(ex.Message); }
        }

        private void EditProduct()
        {
            var p = Selected();
            if (p == null) { Ui.Error("Select a product first."); return; }
            try
            {
                using (var f = new ProductEditorForm(svc, svc.Categories(), p))
                    if (f.ShowDialog(this) == DialogResult.OK) LoadRows(p.ProductId);
            }
            catch (PosException ex) { Ui.Error(ex.Message); }
        }

        private void DeleteProduct()
        {
            var p = Selected();
            if (p == null) { Ui.Error("Select a product first."); return; }
            var answer = MessageBox.Show(
                "Delete \"" + p.Name + "\"?\n\nThe product is hidden from lists but kept so past sales stay intact.",
                "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
            try { svc.Delete(p.ProductId); LoadRows(null); }
            catch (PosException ex) { Ui.Error(ex.Message); }
        }

        private void ApplyAdjustment()
        {
            var p = Selected();
            if (p == null) { Ui.Error("Select a product first."); return; }
            try
            {
                svc.Adjust(p.ProductId, (int)nudChange.Value, cboReason.Text);
                nudChange.Value = 0;
                cboReason.Text = "";
                LoadRows(p.ProductId);
            }
            catch (PosException ex) { Ui.Error(ex.Message); }
        }
    }
}
