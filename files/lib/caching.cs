using System.Collections.Concurrent;
using System.Data;
using System.Linq;

namespace DotNetSearchEngine
{
    public partial class SearchEngine
    {
        //This function is used to keep the cached table and results in sync with the current data being passed
        private void CacheUpToDate()
        {
            //Checks if the search engine name does not exist in the cached results object then it creates it ready
            //Note: basically checks if this is the first run and sets it up if so
            if (!cachedSearchResults.ContainsKey(settings.searchEngineName))
            {
                //Adds a fresh dictionary ready for use by that search engine name
                var temp = new ConcurrentDictionary<string, DataTable>();
                cachedSearchResults.AddOrUpdate(settings.searchEngineName, temp, (key, oldValue) => temp);
            }
                   
            //Checks to see if the current cached datatable under that particular search engine hame has changed as we need to clear the cache if so
            //Note: we do this as the main data coming in has changed and we cannot ensure the cached search results are now accurate so we start fresh.
            if (
                //Checks if its the first load of the cache
                (!cachedTables.ContainsKey(settings.searchEngineName) || cachedTables[settings.searchEngineName] == null || cachedTables[settings.searchEngineName].Rows.Count == 0) ||

                //Checks if the row counts are the same
                (cachedTables[settings.searchEngineName].Rows.Count != settings.table.Rows.Count) ||

                //Checks if the rows are the same literally the same
                !cachedTables[settings.searchEngineName].AsEnumerable().SequenceEqual(settings.table.AsEnumerable(), DataRowComparer.Default)
                )
            {
                //Puts the latest version of the table for that particular search engine name into the cached version as its now different
                cachedTables.AddOrUpdate(settings.searchEngineName, settings.table, (key, oldValue) => settings.table);
                
                //Wipes the current cached search results as it could of changed
                cachedSearchResults[settings.searchEngineName].Clear();
            }
        }

        //This function adds the new search term and results into the cached results
        private void AddCacheResult(string searchValue, DataTable results)
        {   
            cachedSearchResults[settings.searchEngineName].AddOrUpdate(searchValue, results, (key, oldValue) => results);
        }

        //This function gets the search results from the cache for a particular search term if any
        private DataTable GetCacheResult(string searchValue)
        {
            DataTable value = null;
            cachedSearchResults[settings.searchEngineName].TryGetValue(searchValue, out value);
            return value;
        }
    }
}
