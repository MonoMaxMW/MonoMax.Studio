using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts
{
    internal static class Extensions
    {
        internal static double ToDouble(this object value) => Convert.ToDouble(value);
        internal static string GetUiLanguageKey(this Thread thread)
        {
            return thread.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
        }
    }

}
