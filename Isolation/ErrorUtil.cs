using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WinFormsIsolation.Isolation
{
    internal static class ErrorUtil
    {
        [DebuggerStepThrough]
        public static int GetHResult(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            return Marshal.GetHRForException(exception);
        }

        [DebuggerStepThrough]
        public static bool ThrowOnFailure(int hr)
        {
            Marshal.ThrowExceptionForHR(hr);
            return hr != 1;
        }
    }
}
