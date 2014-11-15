using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WinFormsIsolation.Isolation;

namespace WinFormsIsolation.Demo
{
    public class DemoToolStripHost : IsolationHost
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IsolationDemo OriginalDemo { get; set; }

        private bool _disposed;
        private IsolationDomain _domain;

        public DemoToolStripHost()
        {
            _domain = new IsolationDomain();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_domain != null)
                {
                    _domain.Dispose();
                    _domain = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        protected override IIsolationClient CreateWindow()
        {
            var demo = (IsolationDemo)_domain.CreateInstanceAndUnwrap(
                typeof(IsolationDemo).AssemblyQualifiedName
            );

            return demo.CreateToolStrip();
        }
    }
}
