using System;
using System.Threading;
using System.Windows;

namespace MDPlus.Core
{
    /// <summary>
    /// Safe wrapper for Windows clipboard operations that handles temporary locks from other applications.
    /// </summary>
    public static class ClipboardHelper
    {
        public static bool SetText(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    Clipboard.SetDataObject(text, true);
                    return true;
                }
                catch
                {
                    Thread.Sleep(50);
                }
            }

            return false;
        }
    }
}
