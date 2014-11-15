using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Isolation
{
    public class IsolationClient : UserControl, IIsolationClient
    {
        private Control FindTarget(IntPtr hWnd)
        {
            // The messages we get here may not be messages of a control that
            // is in our AppDomain. Because of this, we find a control that is
            // in our AppDomain and request that control to handle it. If
            // that control is a NativeWindowHost, that itself will redirect
            // the message to the correct AppDomain.

            while (hWnd != IntPtr.Zero && hWnd != Handle)
            {
                var control = Control.FromHandle(hWnd);

                if (control != null)
                    return control;

                hWnd = NativeMethods.GetParent(hWnd);
            }

            return null;
        }
    }
}
