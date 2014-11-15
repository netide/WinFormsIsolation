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
    public partial class DemoToolStrip : IsolationClient
    {
        public DemoToolStrip()
        {
            InitializeComponent();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return toolStrip1.GetPreferredSize(proposedSize);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Nested button");
        }
    }
}
