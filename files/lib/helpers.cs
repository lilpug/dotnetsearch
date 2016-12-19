using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DotNetSearchEngine
{
    public partial class SearchEngine
    {
        //This function clones a serializable object rather than just referencing the original
        protected SearchSettings PureCopy(SearchSettings obj)
        {
            using (var stream = new MemoryStream())
            {
                //Note: you cannot serial function pointers/delegates so thats why we are copying the references from one object to another
                
                //Stores the function references
                var tempFunc = obj.extraVerificationChecks;
                var tempFunc2 = obj.extraWeightChecks;

                //Removes the references ready for serialization
                obj.extraWeightChecks = null;
                obj.extraVerificationChecks = null;

                //Serializes the rest of the settings object so we can create a real copy
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                var temp = (SearchSettings)formatter.Deserialize(stream);

                //Puts the references back in the original object                
                obj.extraVerificationChecks = tempFunc;
                obj.extraWeightChecks = tempFunc2;

                //Adds the delegate references of the object over to the new settings object
                temp.extraVerificationChecks = tempFunc;
                temp.extraWeightChecks = tempFunc2;

                return temp;
            }
        }
    }
}
