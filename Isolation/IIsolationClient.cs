using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Isolation
{
    public interface IIsolationClient : IWin32Window, IDisposable
    {
        int PreFilterMessage(ref NiMessage message);

        int IsInputKey(Keys key);

        int IsInputChar(char charCode);
    }
}
