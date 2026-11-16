using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JewSaPOS.Domain;
using JewSaPOS.Services;

namespace JewSaPOS.WinForms
{
    public sealed class ProductEditorForm : Form
    {
        private readonly InventoryService svc;
        private readonly Product existing;
        private readonly TextBox txtSku = new TextBox { MaxLength = 30 };
        private readonly TextBox txtBarcode = new TextBox { MaxLength = 50 };
        private readonly TextBox txtName = new TextBox { MaxLength = 150 };
        private readonly ComboBox cboCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown nudPrice = new NumericUpDown { DecimalPlaces = 2, Maximum = 9999999, ThousandsSeparator = true };
        private readonly NumericUpDown nudCost = new NumericUpDown { DecimalPlaces = 2, Maximum = 9999999, ThousandsSeparator = true };
        private readonly NumericUpDown nudStock = new NumericUpDown { Maximum = 1000000 };
        private readonly NumericUpDown nudReorder = new NumericUpDown { Maximum = 1000000, Value = 5 };
        private readonly CheckBox chkActive = new CheckBox { Text = "Active", Checked = true, AutoSize = true };

        public ProductEditorForm(InventoryService service, List<Category> categories, Product product)
        {
            svc = service;
            existing = product;
            Text = existing == null ? "Add product" : "Edit product";
            ClientSize = new Size(480, 540);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            Font = Ui.FontBody;
            Ui.DarkForm(this);

            cboCategory.DataSource = categories;
            cboCategory.DisplayMember = "Name";
            cboCategory.ValueMember = "CategoryId";
            Ui.StyleInput(txtSku, txtBarcode, txtName, cboCategory, nudPrice, nudCost, nudStock, nudReorder);

            var t = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(28, 8, 28, 16), ColumnCount = 2 };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            int row = 0;
            AddRow(t, "SKU *", txtSku, row++);
            AddRow(t, "Barcode", txtBarcode, row++);
            AddRow(t, "Product name *", txtName, row++);
            AddRow(t, "Category *", cboCategory, row++);
            AddRow(t, "Selling price", nudPrice, row++);
            AddRow(t, "Cost", nudCost, row++);
            AddRow(t, existing == null ? "Initial stock" : "Stock (use Adjust)", nudStock, row++);
            AddRow(t, "Reorder level", nudReorder, row++);
            t.Controls.Add(chkActive, 1, row++);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0, 14, 0, 0) };
            var save = Ui.Btn("Save", Ui.Accent); save.Click += Save;
            var cancel = Ui.Btn("Cancel", Ui.Neutral); cancel.DialogResult = DialogResult.Cancel;
            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);
            t.Controls.Add(buttons, 0, row);
            t.SetColumnSpan(buttons, 2);
            AcceptButton = save;
            CancelButton = cancel;

            var title = new Label
            {
                Text = existing == null ? "Add product" : "Edit product", Dock = DockStyle.Top, Height = 64,
                Font = Ui.FontSemi11, ForeColor = Ui.Text, TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(28, 0, 0, 6)
            };
            Controls.Add(t);       // Fill first
            Controls.Add(title);   // then Top

            if (existing != null)
            {
                txtSku.Text = existing.Sku;
                txtBarcode.Text = existing.Barcode;
                txtName.Text = existing.Name;
                cboCategory.SelectedValue = existing.CategoryId;
                nudPrice.Value = existing.Price;
                nudCost.Value = existing.Cost;
                nudStock.Value = existing.StockQty;
                nudStock.Enabled = false;
                nudReorder.Value = existing.ReorderLevel;
                chkActive.Checked = existing.IsActive;
            }
            else chkActive.Visible = false;
        }

        private static void AddRow(TableLayoutPanel t, string caption, Control c, int row)
        {
            t.Controls.Add(new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = Ui.Muted, Margin = new Padding(0, 10, 0, 10) }, 0, row);
            c.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            t.Controls.Add(c, 1, row);
        }

        private void Save(object sender, EventArgs e)
        {
            try
            {
                var p = new Product
                {
                    ProductId = existing == null ? 0 : existing.ProductId,
                    Sku = txtSku.Text,
                    Barcode = txtBarcode.Text,
                    Name = txtName.Text,
                    CategoryId = cboCategory.SelectedValue == null ? 0 : (int)cboCategory.SelectedValue,
                    Price = nudPrice.Value,
                    Cost = nudCost.Value,
                    StockQty = (int)nudStock.Value,
                    ReorderLevel = (int)nudReorder.Value,
                    IsActive = existing == null || chkActive.Checked
                };
                if (existing == null) svc.Add(p); else svc.Update(p);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (PosException ex) { Ui.Error(ex.Message); }
        }
    }
}
