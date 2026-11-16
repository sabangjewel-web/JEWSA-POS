using System;
using System.Windows.Forms;

namespace JewSaPOS.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (s, e) =>
                MessageBox.Show("Something went wrong. Please try again or contact your administrator.",
                    "Enterprise POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Run(new LoginForm());
        }
    }
}
