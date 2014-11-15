using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFormsIsolation.Isolation;

namespace WinFormsIsolation.Demo
{
    public partial class DemoControl : IsolationClient
    {
        public DemoControl(IsolationDemo originalDomain)
        {
            InitializeComponent();

            demoToolStripHost1.OriginalDemo = originalDomain;
        }
    }
}
