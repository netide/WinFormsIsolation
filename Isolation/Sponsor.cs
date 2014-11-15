using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Lifetime;
using System.Text;

namespace WinFormsIsolation.Isolation
{
    internal class Sponsor : MarshalByRefObject, ISponsor
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