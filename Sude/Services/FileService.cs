using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Sude.Services
{
    public class FileService
    {
        public byte[] GetFileBytes(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            return File.ReadAllBytes(filePath);
        }
        public string GetFileName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            return Path.GetFileName(filePath);
        }
    }
}
