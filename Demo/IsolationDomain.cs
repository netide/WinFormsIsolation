using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WinFormsIsolation.Demo
{
    public class IsolationDomain : IDisposable
    {
        private bool _disposed;
        private AppDomain _appDomain;

        public IsolationDomain()
        {
            var setup = new AppDomainSetup
            {
                ApplicationName = "Isolation Domain",
                ApplicationBase = Path.GetDirectoryName(GetType().Assembly.Location)
            };

            _appDomain = AppDomain.CreateDomain(
                setup.ApplicationName,
                AppDomain.CurrentDomain.Evidence,
                setup
            );
        }

        public object CreateInstanceAndUnwrap(string type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            string[] parts = type.Split(',');

            if (parts.Length < 2)
                throw new ArgumentOutOfRangeException("type");

            return _appDomain.CreateInstanceAndUnwrap(parts[1], parts[0]);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_appDomain != null)
                {
                    AppDomain.Unload(_appDomain);
                    _appDomain = null;
                }

                _disposed = true;
            }
        }
    }
}
