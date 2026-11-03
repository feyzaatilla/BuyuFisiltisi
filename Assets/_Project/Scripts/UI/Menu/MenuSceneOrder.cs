using System;
using System.Collections.Generic;

namespace BuyuFisiltisi.UI.Menu
{
    public static class MenuSceneOrder
    {
        public const string SampleSceneName = "SampleScene";

        public static List<string> Arrange(string menuPath, string gamePath, IEnumerable<string> current)
        {
            var result = new List<string>();
            Add(result, menuPath);
            Add(result, gamePath);

            if (current == null)
                return result;

            foreach (var path in current)
            {
                if (IsSample(path))
                    continue;

                Add(result, path);
            }

            return result;
        }

        public static bool IsSample(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var slash = path.LastIndexOf('/');
            var name = slash >= 0 ? path.Substring(slash + 1) : path;
            return string.Equals(name, SampleSceneName + ".unity", StringComparison.OrdinalIgnoreCase);
        }

        private static void Add(List<string> list, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            foreach (var existing in list)
            {
                if (string.Equals(existing, path, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            list.Add(path);
        }
    }
}
