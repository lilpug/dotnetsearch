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
        /// <summary>
        /// This variable stores the flag for determining if it should only be pulling back full matches when found
        /// Note: This is used for only pulling full match results out when some are found otherwise will pull partials as well
        /// </summary>
        public bool takeFullMatchOnlyWhenFound = false;
        
        /// <summary>
        /// This variable is used for making the search literally only look for full matches and not partial
        /// </summary>
        public bool isFullMatchOnly = false;
        
        /// <summary>
        /// This variable determines if additional weight should be added to exact matches found in the search process
        /// </summary>
        public bool addExtraFullMatchWeight = false;

        /// <summary>
        /// This variable is used to add the additional weight if an exact match is found and the addExtraFullMatchWeight is enabled
        /// </summary>
        public int extraFullMatchWeight = 100;





        
        
        /// <summary>
        /// This variable stores the search engine name which is used in the caching process
        /// Note: This can be changed when you want to do multiple search engine runs in the same project.
        /// </summary>
        public string searchEngineName = "dotnetsearch";

        /// <summary>
        /// This variable stores the flag for determining if we use a cache on the data found and the results set
        /// </summary>
        public bool isCacheEnabled = true;

        //Stores the flag for determining if the cache should auto clear itself or wait for a manual clear

        /// <summary>
        /// This variable stores the flag for determining if the cache should auto clear itself or wait for a manual clear
        /// </summary>
        public bool isCacheManualClearMode = false;

        /// <summary>
        /// This variable stores the search string
        /// </summary>
        public string searchString = null;

        /// <summary>
        /// This variable stores the datatable to perform the search engine process on
        /// </summary>
        public DataTable table = null;

        /// <summary>
        /// This variable stores any additional field weighting requirements
        /// </summary>
        public Dictionary<string, int> weightings = null;

        /// <summary>
        /// This variable flags if we should order by the search weights before processing the orderBy    
        /// </summary>
        public bool orderByWeightFirst = false;

        /// <summary>
        /// This variable stores any orderby field requirements     
        /// </summary>
        public Dictionary<string, SearchOrderType> orderBy = null;

        /// <summary>
        /// This variable flags if we should use a standard weighting for every find
        /// </summary>
        public bool allowDefault = true;

        /// <summary>
        /// This variable stores any columns which we are to ignore in the search process
        /// </summary>
        public string[] ignoreFields = null;
        
        /// <summary>
        /// This variable stores the only fields which should be checked in the search process if specified 
        /// Note: if this is not specified then it searchs all but the ignored fields
        /// </summary>
        public string[] onlyFieldsToCheck = null;

        /// <summary>
        /// This variable stores how many cores we should be using to process the search results
        /// </summary>
        public int multiThreadedCores = 1;
        
        /// <summary>
        /// Stores the max results that will be returned at the end
        /// Note: zero means all results are returned "no max"
        /// </summary>
        public int maxReturn = 0;
        
        /// <summary>
        /// This variable stores additional functions that can be added into the search criteria        
        /// Note: a false return will mean the row/record is not included in the search process
        /// </summary>
        public List<Func<DataRow, bool>> extraVerificationChecks = null;

        /// <summary>
        /// This variable stores the additional weight checking functions in case there needs to be more complex checks in the search process
        /// </summary>
        public List<Func<DataRow, string, int>> extraWeightChecks = null;

        /// <summary>
        /// This variable ensures the disposing is only called once
        /// </summary>
        protected bool isDisposed = false;

        /// <summary>
        /// This function disposes the variables inside the SearchSettings object
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {   
                searchEngineName = null;
                isCacheEnabled = false;
                isCacheManualClearMode = false;
                searchString = null;
                table.Dispose();
                table = null;
                weightings = null;
                orderByWeightFirst = false;
                orderBy.Clear();
                orderBy = null;
                allowDefault = true;
                ignoreFields = null;
                onlyFieldsToCheck = null;
                multiThreadedCores = 0;
                maxReturn = 0;
                extraVerificationChecks = null;
                extraWeightChecks = null;
                
                isDisposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
