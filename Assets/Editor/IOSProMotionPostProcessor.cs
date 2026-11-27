using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace SheriffAmericanLife.Editor
{
    /// <summary>
    /// Unlocks the full supported iOS refresh-rate range on ProMotion iPhones.
    /// The display and thermal/power state still determine the actual refresh rate.
    /// </summary>
    public sealed class IOSProMotionPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1000;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
                return;

            string plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
            if (!File.Exists(plistPath))
                return;

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetBoolean("CADisableMinimumFrameDurationOnPhone", true);
            plist.WriteToFile(plistPath);
        }
    }
}
