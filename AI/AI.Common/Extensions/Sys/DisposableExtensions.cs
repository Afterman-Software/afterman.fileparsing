using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public static class DisposableExtensions
    {
        public static bool TryDispose(this IDisposable disposable)
        {
            bool success = false;
            try
            {
                disposable.Dispose();
            }
            catch
            {
                success = false;
            }
            return success;
        }
    }

