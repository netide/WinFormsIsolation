using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;

namespace WinFormsIsolation.Isolation
{
    public class IsolationClient : MarshalByRefObject, IIsolationClient
    {
        private static readonly MethodInfo IsInputKeyMethod = typeof(Control).GetMethod(
            "IsInputKey",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            new[] { typeof(Keys) },
            null
        );

        private static readonly MethodInfo IsInputCharMethod = typeof(Control).GetMethod(
            "IsInputChar",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            new[] { typeof(char) },
            null
        );

        private delegate bool IsInputKeyDelegate(Control control, Keys key);
        private delegate bool IsInputCharDelegate(Control control, char charData);

        private static readonly IsInputKeyDelegate _isInputKeyDelegate;
        private static readonly IsInputCharDelegate _isInputCharDelegate;

        static IsolationClient()
        {
            _isInputKeyDelegate = BuildIsInputKeyDelegate();
            _isInputCharDelegate = BuildIsInputCharDelegate();
        }

        private static IsInputKeyDelegate BuildIsInputKeyDelegate()
        {
            var isInputKeyMethod = new DynamicMethod(
                "IsInputKey",
                typeof(bool),
                new[] { typeof(Control), typeof(Keys) },
                typeof(IsolationClient).Module,
                true
            );

            var il = isInputKeyMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, IsInputKeyMethod);
            il.Emit(OpCodes.Ret);

            return (IsInputKeyDelegate)isInputKeyMethod.CreateDelegate(typeof(IsInputKeyDelegate));
        }

        private static IsInputCharDelegate BuildIsInputCharDelegate()
        {
            var isInputCharMethod = new DynamicMethod(
                "IsInputChar",
                typeof(bool),
                new[] { typeof(Control), typeof(char) },
                typeof(IsolationClient).Module,
                true
            );

            var il = isInputCharMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, IsInputCharMethod);
            il.Emit(OpCodes.Ret);

            return (IsInputCharDelegate)isInputCharMethod.CreateDelegate(typeof(IsInputCharDelegate));
        }

        private bool _hwndCreated;
        private IntPtr _hwnd;
        private bool _disposed;

        public IWin32Window Client { get; set; }

        IntPtr IWin32Window.Handle
        {
            get
            {
                if (!_hwndCreated)
                    CreateHandle();

                return _hwnd;
            }
        }

        private void CreateHandle()
        {
            _hwndCreated = true;

            if (Client != null)
                _hwnd = Client.Handle;
        }

        public int PreFilterMessage(ref NiMessage message)
        {
            try
            {
                if (Client == null)
                    return 1;

                var control = FindTarget(message.HWnd);

                if (control == null)
                    return 1;

                Message msg = message;
                bool processed = control.PreProcessMessage(ref msg);
                message = msg;

                return processed ? 0 : 1;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        public int IsInputKey(Keys key)
        {
            try
            {
                var focused = FindTarget(NativeMethods.GetFocus());
                if (focused != null)
                    return _isInputKeyDelegate(focused, key) ? 0 : 1;

                return 1;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        public int IsInputChar(char charCode)
        {
            try
            {
                var focused = FindTarget(NativeMethods.GetFocus());
                if (focused != null)
                    return _isInputCharDelegate.Invoke(focused, charCode) ? 0 : 1;

                return 1;
            }
            catch (Exception ex)
            {
                return ErrorUtil.GetHResult(ex);
            }
        }

        private Control FindTarget(IntPtr hWnd)
        {
            // The messages we get here may not be messages of a control that
            // is in our AppDomain. Because of this, we find a control that is
            // in our AppDomain and request that control to handle it. If
            // that control is a NativeWindowHost, that itself will redirect
            // the message to the correct AppDomain.

            while (hWnd != IntPtr.Zero && hWnd != Client.Handle)
            {
                var control = Control.FromHandle(hWnd);

                if (control != null)
                    return control;

                hWnd = NativeMethods.GetParent(hWnd);
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                var disposable = Client as IDisposable;

                if (disposable != null)
                    disposable.Dispose();

                _disposed = true;
            }
        }
    }
}
