using System;
using System.Collections.Generic;
using System.Data;
namespace DotNetSearchEngine
{
    //Note: these are serializable so we can use the deepclone function to copy the settings object on initialisation of the SearchEngine class

    //This is the orderby structured used for the priority of ordering
    public class SearchOrderType
    {
        public bool isDescending { get; set; }
        public int priorityLevel { get; set; }
    }

    //This class stores all the parameters required for the main search engine to be initialised
    public class SearchSettings : IDisposable
    {
        //Stores the search engine name which is used for caching
        /*Note: this is done so we can have multiple search engines in the same project but still use caching effectively
                all that would be required is to change the name if its for a search on a different data set. */
        public string searchEngineName = "dotnetsearch";

        //Stores the flag for determining if we use a cache on the data and the results
        public bool isCacheEnabled = true;

        //Stores the flag for determining if the cache should auto clear itself or wait for a manual clear
        public bool isCacheManualClearMode = false;

        //Stores the query string
        public string query = null;

        //Stores the datatable records to be searched
        public DataTable table = null;

        //Stores any additional field weighting requirements
        public Dictionary<string, int> weightings = null;   
        
        //Stores any orderby field requirements     
        public Dictionary<string, SearchOrderType> orderBy = null;        
                
        //Flags if we should use a standard weighting for every find
        public bool allowDefault = true;
        
        //Stores any columns which we are to ignore in the datarows
        public string[] ignoreFields = null;

        //Stores the only fields which should be checked in the datarows if specified 
        //Note: if this is not specified then it searchs all but the ignored fields
        public string[] onlyFieldsToCheck = null;

        //Stores how many cores we should be using to process the search results
        public int multiThreadCores = 1;

        //Stores the max results that will be returned at the end
        //Note: zero means all results returned "not capped"
        public int maxReturn = 0;
        
        //Stores additional functions that can be added into the search criteria        
        //Note: a false return will mean the row/record is not included in the weight checking        
        public List<Func<DataRow, bool>> extraVerificationChecks = null;

        //Stores the additional weight checking functions in case there needs to be more complex checks
        public List<Func<DataRow, string, int>> extraWeightChecks = null;

        //Ensures the disposing is only called once
        protected bool _disposed = false;

        //This is the main dispose method
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //This disposes of all the setting variables
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    searchEngineName = null;
                    isCacheEnabled = false;
                    isCacheManualClearMode = false;
                    query = null;
                    table.Dispose();
                    table = null;                    
                    weightings = null;
                    orderBy.Clear();
                    orderBy = null;
                    allowDefault = true;
                    ignoreFields = null;
                    onlyFieldsToCheck = null;
                    multiThreadCores = 0;
                    maxReturn = 0;                    
                    extraVerificationChecks = null;                    
                    extraWeightChecks = null;
                }
                _disposed = true;
            }
        }
    }
}
