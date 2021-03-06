﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Demo
{
    public partial class DemoForm : Form
    {
        public DemoForm(IsolationDemo isolationDemo)
        {
            InitializeComponent();

            demoHost1.OriginalDemo = isolationDemo;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Exit on the form");
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Host tool strip button");
        }
    }
}
