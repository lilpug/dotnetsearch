namespace DotNetSearchEngine
{
    public partial class SearchEngine
    {
        //Holds the internal settings required
        readonly internal int defaultWeight = 1;
        internal SearchSettings settings;

        //Used to lock the thread while adding a row or updating it
        static internal readonly object updateLocker = new object();
        static internal readonly object addLocker = new object();

        //Constructor for loading the passed settings
        public SearchEngine(SearchSettings searchSettings)
        {
            settings = searchSettings;
        }
    }
}
