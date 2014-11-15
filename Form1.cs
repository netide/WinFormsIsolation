using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFormsIsolation.Demo;

namespace WinFormsIsolation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var domain = new IsolationDomain())
            {
                var demo = (IsolationDemo)domain.CreateInstanceAndUnwrap(
                    typeof(IsolationDemo).AssemblyQualifiedName
                );

                demo.ShowDialog();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            button1.PerformClick();
        }
    }
}
