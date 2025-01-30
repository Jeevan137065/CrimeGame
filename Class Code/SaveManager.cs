using System.IO;

namespace CrimeGame.Class_Code
{
    internal class SaveManager
    {
        private string BasePath;

        public SaveManager(string basePath)
        {
            BasePath = basePath;
            if (!Directory.Exists(BasePath)) { }
        }

        public void SaveFile(string filename, string content)
        {
            string fullPath = Path.Combine(BasePath, filename);
            File.WriteAllText(fullPath, content);
        }

        public string LoadFile(string filename) 
        {

            string fullpath = Path.Combine(BasePath, filename);
            if (File.Exists(fullpath)) { return File.ReadAllText(fullpath); }
            throw new FileNotFoundException($"File '{filename}' not found in '{fullpath}'");
        }
        public void CreateNewFile(string fileName) 
        {
            string fullpath = Path.Combine(BasePath, fileName);
            if (File.Exists(fullpath)) { File.Create(fullpath).Dispose(); }
        }
    }
}
