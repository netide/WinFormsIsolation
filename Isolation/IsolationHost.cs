using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Isolation
{
    public abstract class IsolationHost : Control, IIsolationHost
    {
        private IntPtr _childHwnd;
        private readonly bool _designMode;
        private IIsolationClient _client;
        private readonly Sponsor _sponsor = new Sponsor();
        private int _select;

        public IIsolationClient Client
        {
            get
            {
                if (_client == null)
                    CreateHandle();

                return _client;
            }
        }

        public event EventHandler WindowCreated;

        protected virtual void OnWindowCreated(EventArgs e)
        {
            var ev = WindowCreated;
            if (ev != null)
                ev(this, e);
        }

        protected IsolationHost()
        {
            SetStyle(ControlStyles.Selectable, true);

            _designMode = ControlUtil.GetIsInDesignMode(this);
        }

        protected abstract IIsolationClient CreateWindow();

        protected void SetChildHwnd(IntPtr handle)
        {
            _childHwnd = handle;

            if (_childHwnd != IntPtr.Zero)
                ParentChild();
        }

        private void ParentChild()
        {
            NativeMethods.SetParent(new HandleRef(this, _childHwnd), new HandleRef(this, Handle));

            PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            if (_childHwnd == IntPtr.Zero)
                return;

            var size = ClientSize;

            NativeMethods.SetWindowPos(
                new HandleRef(this, _childHwnd),
                new HandleRef(),
                0,
                0,
                size.Width,
                size.Height,
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOMOVE
            );
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            NativeMethods.SetFocus(new HandleRef(this, _childHwnd));
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (_designMode || _client != null)
                return;

            _client = CreateWindow();

            if (_client == null)
                throw new InvalidOperationException("CreateWindow did not create a client");

            _sponsor.Register((MarshalByRefObject)_client);
            _client.SetHost(this);

            SetChildHwnd(_client.Handle);

            OnWindowCreated(EventArgs.Empty);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);

            if (_client != null)
            {
                _sponsor.Unregister();

                _client.Dispose();
                _client = null;
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = ErrorUtil.ThrowOnFailure(_client.PreviewKeyDown(e.KeyData));
        }

        public override bool PreProcessMessage(ref Message msg)
        {
            NiMessage message = msg;
            PreProcessMessageResult preProcessMessageResult;
            bool processed = ErrorUtil.ThrowOnFailure(_client.PreProcessMessage(ref message, out preProcessMessageResult));
            msg = message;

            Stubs.ControlSetState2(
                this,
                Stubs.STATE2_INPUTKEY,
                (preProcessMessageResult & PreProcessMessageResult.IsInputKey) != 0
            );

            Stubs.ControlSetState2(
                this,
                Stubs.STATE2_INPUTCHAR,
                (preProcessMessageResult & PreProcessMessageResult.IsInputChar) != 0
            );

            return processed;
        }

        protected override bool ProcessMnemonic(char charCode)
        {
            return ErrorUtil.ThrowOnFailure(_client.ProcessMnemonic(charCode));
        }

        protected override void Select(bool directed, bool forward)
        {
            if (_select > 0)
                return;

            _select++;

            try
            {
                // We were the target of select next control. Forward the
                // call to the isolation client which does its search. If it
                // matches a control, we need to make ourselves active.

                if (ErrorUtil.ThrowOnFailure(_client.SelectNextControl(!directed || forward)))
                {
                    base.Select(directed, forward);
                    return;
                }

                // If the client wasn't able to select something, we continue the
                // search from here. One small detail is that SelectNextControl
                // does not match itself. When it would match an IsolationClient,
                // this would mean that the search does not go into the
                // IsolationHost. We specifically match this case by first doing
                // a non-wrapping search and matching the root for IsolationClient.
                // If that matches, we allow the IsolationClient to continue
                // the search upwards. Otherwise, we continue the search
                // from the root as usual.

                var root = ControlUtil.GetRoot(this);

                if (root.SelectNextControl(this, !directed || forward, true, true, !(root is IsolationClient)))
                    return;

                if (root is IsolationClient)
                    Stubs.ControlSelect(root, directed, forward);
            }
            finally
            {
                _select--;
            }
        }

        int IIsolationHost.ProcessCmdKey(ref NiMessage message, Keys keyData)
        {
            try
            {
                Message msg = message;
                bool result = ProcessCmdKey(ref msg, keyData);
                message = msg;

                return result ? 0 : 1;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        int IIsolationHost.ProcessDialogKey(Keys keyData)
        {
            try
            {
                return ProcessDialogKey(keyData) ? 0 : 1;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        int IIsolationHost.ProcessDialogChar(char charData)
        {
            try
            {
                return ProcessDialogChar(charData) ? 0 : 1;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        int IIsolationHost.SelectNextControl(bool forward)
        {
            try
            {
                if (_select > 0)
                    return 1;

                _select++;

                try
                {
                    // The client was the target of select next control. It
                    // has handed the search over to us and we continue from
                    // here. We need to wrap to get the correct behavior.

                    return ControlUtil.GetRoot(this).SelectNextControl(this, forward, true, true, true) ? 0 : 1;
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
    }

    public abstract class IsolationHost<T> : IsolationHost
        where T : class, IIsolationClient
    {
        public new T Client
        {
            get { return (T)base.Client; }
        }
    }
}
