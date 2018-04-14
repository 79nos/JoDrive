using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoDrive.Utilities
{
    public static class Utility
    {
        public static string AddRandomTail(string file_path)
        {
            string filename = Path.GetFileNameWithoutExtension(file_path) + "_" + Path.GetRandomFileName();
            string filenameandext = filename + "." + Path.GetExtension(file_path);
            return Path.Combine(Path.GetDirectoryName(file_path), filenameandext);
        }

        public static string GetRelativePath(string full_path, string root)
        {
            if (root[root.Length - 1] != '\\' && root[root.Length - 1] != '/')
                root += Path.DirectorySeparatorChar;
            Uri full = new Uri(full_path);
            return new Uri(root).MakeRelativeUri(full).ToString();
        }
        public static bool TryDo(Action action, out Exception ex)
        {
            try
            {
                action();
                ex = null;
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }
        public static bool TryGet<T>(Func<T> func, out T value, out Exception ex)
        {
            try
            {
                T temp = func();
                value = temp;
                ex = null;
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                value = default(T);
                return false;
            }
        }
    }
}
