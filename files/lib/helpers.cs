using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DotNetSearchEngine
{
    public partial class SearchEngine
    {
        //This function clones a serializable object rather than just referencing the original
        protected T DeepClone<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
