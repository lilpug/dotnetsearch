using System;
using System.Collections.Concurrent;
using System.Data;

namespace DotNetSearchEngine
{
    public partial class SearchEngine : IDisposable
    {
        //Holds the internal default weight to use
        readonly internal int defaultWeight = 1;

        //Holds the settings object        
        internal SearchSettings settings;

        //Used to lock the thread while adding a row or updating it
        static internal readonly object updateLocker = new object();
        static internal readonly object addLocker = new object();

        //These are used for the caching section if its enabled

        //This stores the cached table data being passed
        static internal ConcurrentDictionary<string, DataTable> cachedTables = new ConcurrentDictionary<string, DataTable>();

        //This stores all the queries that we have saved overtime to make calculation times fast if the datatable data is the same
        static internal ConcurrentDictionary<string, ConcurrentDictionary<string, DataTable>> cachedSearchResults = new ConcurrentDictionary<string, ConcurrentDictionary<string, DataTable>>();

        //Constructor for loading the passed settings
        public SearchEngine(SearchSettings searchSettings)
        {
            settings = searchSettings;

            //Checks if caching is enabled and if so determines if the data has changed thats coming in
            if(settings.isCacheEnabled)
            {
                CacheUpToDate();
            }
        }
    }
}
