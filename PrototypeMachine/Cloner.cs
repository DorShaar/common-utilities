using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PrototypeMachine
{
    public class Cloner
    {
        public T ShallowClone<T>(T ObjectToClone)
        {
            return (T)MemberwiseClone();
        }

        public T DeepClone<T>(T ObjectToClone) where T : class
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, ObjectToClone);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
