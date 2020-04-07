using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts
{
    public static class AssetRepository
    {
        static AssetRepository()
        {
            var fi = new FileInfo(typeof(AssetRepository).Assembly.Location);

            AssetDirectory = Path.Combine(
                fi.DirectoryName,
                "Assets",
                "components");
        }

        public static string AssetDirectory { get; }

        public static string GetImagePath(string imageKey, int size) => Path.Combine(AssetDirectory, "machining_icons", imageKey + $"_{size}.png");
        public static string GetFullAssetpath(string assetKey) => Path.Combine(AssetDirectory, assetKey);
    }
}
