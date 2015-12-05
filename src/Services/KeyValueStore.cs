using Newtonsoft.Json;
using System.IO;

namespace Autopost.Services
{
    public interface IKeyValueStore
    {
        void Set<T>(string key, T value);
        T Get<T>(string key);
    }

    public class FileBasedKeyValueStore
        : IKeyValueStore
    {
        private readonly string _basePath;
        
        public FileBasedKeyValueStore(string basePath)
        {
            _basePath = basePath;
        }

        public void Set<T>(string key, T value) =>
            File.WriteAllText(Path(key), Serialize<T>(value));

        public T Get<T>(string key)
        {
            var path = Path(key);
            if(File.Exists(path))
            {
                return Unserialize<T>(File.ReadAllText(path));
            }

            return default(T);
        }

        private string Path(string key) => System.IO.Path.Combine(_basePath, key + ".json");

        private string Serialize<T>(T value) => JsonConvert.SerializeObject(value, Formatting.Indented);
        private T Unserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value);
    }
}
