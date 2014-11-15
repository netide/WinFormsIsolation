using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Demo
{
    public partial class DemoToolStrip : UserControl
    {
        public DemoToolStrip()
        {
            InitializeComponent();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return toolStrip1.GetPreferredSize(proposedSize);
        }
    }
}
