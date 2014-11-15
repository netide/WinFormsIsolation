using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Isolation
{
    public class IsolationClient : UserControl, IIsolationClient
    {
        private IIsolationHost _host;
        private int _select;
        private bool _disposed;
        private readonly Sponsor _sponsor = new Sponsor();

        public IsolationClient()
        {
            SetStyle(ControlStyles.Selectable, true);
        }

        public int SetHost(IIsolationHost host)
        {
            try
            {
                if (host == null)
                    throw new ArgumentNullException("host");

                _host = host;
                _sponsor.Register((MarshalByRefObject)host);

                return 0;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        protected override void Select(bool directed, bool forward)
        {
            if (_select > 0)
                return;

            _select++;

            try
            {
                ErrorUtil.ThrowOnFailure(_host.SelectNextControl(!directed || forward));
            }
            finally
            {
                _select--;
            }
        }

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

        int IIsolationClient.PreviewKeyDown(Keys keyData)
        {
            try
            {
                int result = 1;

                var target = FindTarget(NativeMethods.GetFocus());
                if (target != null)
                {
                    var e = new PreviewKeyDownEventArgs(keyData);

                    Stubs.ControlOnPreviewKeyDown(target, e);

                    result = e.IsInputKey ? 0 : 1;
                }

                return result;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        int IIsolationClient.PreProcessMessage(ref NiMessage message, out PreProcessMessageResult preProcessMessageResult)
        {
            preProcessMessageResult = 0;

            try 
            {
                int result = 1;

                var target = FindTarget(message.HWnd);
                if (target != null)
                {
                    Message msg = message;
                    result = target.PreProcessMessage(ref msg) ? 0 : 1;
                    message = msg;

                    if (Stubs.ControlGetState2(target, Stubs.STATE2_INPUTKEY))
                        preProcessMessageResult |= PreProcessMessageResult.IsInputKey;
                    if (Stubs.ControlGetState2(target, Stubs.STATE2_INPUTCHAR))
                        preProcessMessageResult |= PreProcessMessageResult.IsInputChar;
                }

                return result;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        int IIsolationClient.ProcessMnemonic(char charCode)
        {
            try
            {
                return ProcessMnemonic(charCode) ? 0 : 1;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        int IIsolationClient.SelectNextControl(bool forward)
        {
            try
            {
                if (_select > 0)
                    return 1;

                _select++;

                try
                {
                    // The host was the target of the next control selection.
                    // The call is forwarded here and we select our next control.
                    // Wrap is false because if we get to the end, we need to
                    // return false so that the host continues the search.

                    return SelectNextControl(this, forward, true, true, false) ? 0 : 1;
                }
                finally
                {
                    _select--;
                }
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        int IIsolationClient.GetPreferredSize(Size proposedSize, out Size preferredSize)
        {
            preferredSize = new Size();

            try
            {
                preferredSize = GetPreferredSize(proposedSize);

                return 0;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (base.ProcessCmdKey(ref msg, keyData))
                return true;

            NiMessage message = msg;
            bool result = ErrorUtil.ThrowOnFailure(_host.ProcessCmdKey(ref message, keyData));
            msg = message;

            return result;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (base.ProcessDialogKey(keyData))
                return true;

            return ErrorUtil.ThrowOnFailure(_host.ProcessDialogKey(keyData));
        }

        protected override bool ProcessDialogChar(char charCode)
        {
            if (base.ProcessDialogChar(charCode))
                return true;

            return ErrorUtil.ThrowOnFailure(_host.ProcessDialogChar(charCode));
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _sponsor.Unregister();

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
