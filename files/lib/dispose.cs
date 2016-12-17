using System;

namespace DotNetSearchEngine
{
    public partial class SearchEngine : IDisposable
    {
        //Ensures the disposing is only called once
        internal bool _disposed = false;

        //This is the main dispose method
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //This disposes of the settings object
        internal void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    settings = null;

                }
                _disposed = true;
            }
        }
    }
}
