using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Demo
{
    public class ShortcutTextBox : TextBox
    {
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Tab:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return base.IsInputKey(keyData);

                default:
                    return true;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == (Keys)8)
            {
                Text = null;
                return;
            }

            var sb = new StringBuilder();

            if ((e.KeyData & Keys.Control) != 0)
                sb.Append("Control+");
            if ((e.KeyData & Keys.Alt) != 0)
                sb.Append("Alt+");
            if ((e.KeyData & Keys.Shift) != 0)
                sb.Append("Shift+");
            sb.Append(e.KeyData & Keys.KeyCode);

            Text = sb.ToString();
        }
    }
}
