using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Isolation
{
    public abstract class IsolationHost : Control
    {
        private IntPtr _childHwnd;
        private readonly bool _designMode;
        private IIsolationClient _client;
        private readonly Sponsor _sponsor;

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
            _designMode = ControlUtil.GetIsInDesignMode(this);
            _sponsor = new Sponsor();
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

        private class Sponsor : MarshalByRefObject, ISponsor
        {
            private static readonly TimeSpan RenewalInterval = TimeSpan.FromMinutes(2);

            private readonly object _syncRoot = new object();
            private ILease _lease;

            public void Register(MarshalByRefObject obj)
            {
                Debug.Assert(_lease == null);

                var lease = (ILease)obj.GetLifetimeService();
                if (lease == null)
                    return;

                lease.Register(this);

                lock (_syncRoot)
                {
                    _lease = lease;
                }
            }

            public void Unregister()
            {
                Debug.Assert(_lease != null);

                ILease lease;

                lock (_syncRoot)
                {
                    lease = _lease;
                }

                if (lease != null)
                    lease.Unregister(this);
            }

            public TimeSpan Renewal(ILease lease)
            {
                return RenewalInterval;
            }

            public override object InitializeLifetimeService()
            {
                return null;
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
