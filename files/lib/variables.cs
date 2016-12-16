namespace DotNetSearchEngine
{
    public partial class SearchEngine
    {
        //Holds the internal settings required
        readonly internal int defaultWeight = 1;
        internal SearchSettings settings;

        //Used to lock the thread while adding a row
        static internal readonly object locker = new object();

        //Constructor for loading the passed settings
        public SearchEngine(SearchSettings searchSettings)
        {
            settings = searchSettings;
        }
    }
}
