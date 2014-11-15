using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Isolation
{
    public interface IIsolationClient : IWin32Window, IDisposable
    {
        int SetHost(IIsolationHost host);

        int PreviewKeyDown(Keys keyData);

        int PreProcessMessage(ref NiMessage message, out PreProcessMessageResult preProcessMessageResult);

        int ProcessMnemonic(char charCode);

        int SelectNextControl(bool forward);

        int GetPreferredSize(Size proposedSize, out Size preferredSize);
    }
}
