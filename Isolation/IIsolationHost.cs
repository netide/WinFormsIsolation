using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Isolation
{
    public interface IIsolationHost
    {
        int ProcessCmdKey(ref NiMessage message, Keys keyData);

        int ProcessDialogKey(Keys keyData);

        int ProcessDialogChar(char charData);

        int SelectNextControl(bool forward);
    }
}
