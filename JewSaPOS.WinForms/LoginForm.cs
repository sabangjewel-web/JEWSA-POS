using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JewSaPOS.Domain;
using JewSaPOS.Services;

namespace JewSaPOS.WinForms
{
    public sealed class LoginForm : Form
    {
        private readonly TextBox txtUsername = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly Button btnLogin = Ui.Btn("Sign in", Ui.Accent);
        private readonly Label lblStatus = new Label();

        public LoginForm()
        {
            Text = "Enterprise POS - Login";
            ClientSize = new Size(780, 480);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = Ui.FontBody;
            Ui.DarkForm(this);

            var right = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Bg };

            right.Controls.Add(new Label
            {
                Text = "Welcome back", AutoSize = true, ForeColor = Ui.Text, Font = Ui.FontH1,
                Location = new Point(56, 66)
            });
            right.Controls.Add(new Label
            {
                Text = "Sign in to your account to continue", AutoSize = true, ForeColor = Ui.Muted,
                Location = new Point(58, 108)
            });

            right.Controls.Add(new Label { Text = "Username", AutoSize = true, ForeColor = Ui.Muted, Location = new Point(58, 156) });
            txtUsername.Font = new Font("Segoe UI", 11f);
            var userField = Ui.Field(txtUsername, 368, 44);
            userField.Location = new Point(56, 178);
            right.Controls.Add(userField);
            Ui.Placeholder(txtUsername, "Enter your username");

            right.Controls.Add(new Label { Text = "Password", AutoSize = true, ForeColor = Ui.Muted, Location = new Point(58, 242) });
            txtPassword.Font = new Font("Segoe UI", 11f);
            txtPassword.UseSystemPasswordChar = true;
            var passField = Ui.Field(txtPassword, 368, 44);
            passField.Location = new Point(56, 264);
            right.Controls.Add(passField);
            Ui.Placeholder(txtPassword, "Enter your password");

            btnLogin.Size = new Size(368, 46);
            btnLogin.Location = new Point(56, 336);
            btnLogin.Font = new Font("Segoe UI Semibold", 10.5f);
            btnLogin.Click += LoginClicked;
            right.Controls.Add(btnLogin);
            AcceptButton = btnLogin;

            lblStatus.ForeColor = Ui.Danger;
            lblStatus.AutoSize = true;
            lblStatus.MaximumSize = new Size(368, 0);
            lblStatus.Location = new Point(58, 396);
            right.Controls.Add(lblStatus);

            var left = new PaintPanel { Dock = DockStyle.Left, Width = 300, BackColor = Ui.Sidebar };
            left.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var mark = new Rectangle(40, 44, 48, 48);
                using (var path = Ui.Round(new Rectangle(mark.X, mark.Y, mark.Width - 1, mark.Height - 1), 12))
                using (var b = new SolidBrush(Ui.Accent))
                    g.FillPath(b, path);
                TextRenderer.DrawText(g, "E", Ui.FontBig, mark, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, "Enterprise POS", Ui.FontH1, new Rectangle(40, 170, 250, 40), Ui.Text,
                    TextFormatFlags.Left | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, "Sales and inventory,\r\nmade simple.", Ui.FontBody, new Rectangle(40, 214, 250, 50), Ui.Muted,
                    TextFormatFlags.Left | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, "Version 1.0", Ui.FontSmall, new Rectangle(40, 436, 200, 20), Ui.Muted,
                    TextFormatFlags.Left | TextFormatFlags.NoPadding);
            };

            Controls.Add(right);   // Fill first
            Controls.Add(left);    // then Left
            ActiveControl = txtUsername;
        }

        private void LoginClicked(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            var username = txtUsername.Text.Trim();
            if (username.Length == 0 || txtPassword.Text.Length == 0)
            {
                lblStatus.Text = "Enter your username and password.";
                return;
            }
            try
            {
                Cursor = Cursors.WaitCursor;
                var user = new AuthService().Login(username, txtPassword.Text);
                if (user == null)
                {
                    lblStatus.Text = "Invalid credentials or inactive user.";
                    return;
                }
                Hide();
                using (var main = new MainForm(user)) main.ShowDialog(this);
                txtPassword.Clear();
                txtUsername.Clear();
                Show();
                txtUsername.Focus();
            }
            catch (PosException ex) { lblStatus.Text = ex.Message; }
            catch (Exception) { lblStatus.Text = "Unable to sign in. Contact the administrator."; }
            finally { Cursor = Cursors.Default; }
        }
    }
}
