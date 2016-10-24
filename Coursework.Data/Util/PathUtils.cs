using System;
using System.IO;
using System.Reflection;

namespace Coursework.Data.Util
{
    public class PathUtils
    {
        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string GetFileFullPath(string relativePath)
        {
            return Path.Combine(AssemblyDirectory, relativePath);
        }
    }
}
