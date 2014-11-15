﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Isolation
{
    internal class ControlUtil
    {
        public static bool GetIsInDesignMode(Control control)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;

            for (; control != null; control = control.Parent)
            {
                if (control.Site != null && control.Site.DesignMode)
                    return true;
            }

            return false;
        }
    }
}
