using System;
using System.Collections.Generic;
using System.Data;
namespace DotNetSearchEngine
{
    //This is the orderby structured used for the priority of ordering
    public class SearchOrderType
    {
        public bool isDescending { get; set; }
        public int priorityLevel { get; set; }
    }

    //This class stores all the parameters required for the main search engine to be initialised
    public class SearchSettings
    {
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
    }
}
