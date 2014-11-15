using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinFormsIsolation.Isolation;

namespace WinFormsIsolation.Demo
{
    public class IsolationDemo : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void ShowDialog()
        {
            using (var form = new DemoForm(this))
            {
                form.ShowDialog();
            }
        }

        public IIsolationClient CreateClient(IsolationDemo originalDomain)
        {
            return new IsolationClient { Client = new DemoControl(originalDomain) };
        }

        public IIsolationClient CreateToolStrip()
        {
            return new IsolationClient { Client = new DemoToolStrip() };
        }
    }
}
