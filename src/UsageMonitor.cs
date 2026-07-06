using System;
using Microsoft.Win32;

namespace PrivacyDots
{
    // Reads the Windows Capability Access Manager consent store. Windows tracks
    // every app's camera/microphone sessions there: while a device is in use,
    // the app's LastUsedTimeStop value is 0. This is the same data source the
    // built-in Windows 10/11 mic/camera taskbar indicators use, so no drivers
    // or audio/video APIs are needed and the check is cheap enough to poll.
    public static class UsageMonitor
    {
        const string BasePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\";

        public static bool IsMicrophoneInUse()
        {
            return IsCapabilityInUse("microphone");
        }

        public static bool IsCameraInUse()
        {
            return IsCapabilityInUse("webcam");
        }

        static bool IsCapabilityInUse(string capability)
        {
            return ScanHive(Registry.CurrentUser, capability) || ScanHive(Registry.LocalMachine, capability);
        }

        static bool ScanHive(RegistryKey hive, string capability)
        {
            try
            {
                using (RegistryKey root = hive.OpenSubKey(BasePath + capability))
                {
                    if (root == null) return false;
                    foreach (string name in root.GetSubKeyNames())
                    {
                        using (RegistryKey appKey = root.OpenSubKey(name))
                        {
                            if (appKey == null) continue;
                            if (string.Equals(name, "NonPackaged", StringComparison.OrdinalIgnoreCase))
                            {
                                // Classic desktop apps live one level deeper
                                foreach (string sub in appKey.GetSubKeyNames())
                                {
                                    using (RegistryKey desktopApp = appKey.OpenSubKey(sub))
                                    {
                                        if (SessionActive(desktopApp)) return true;
                                    }
                                }
                            }
                            else if (SessionActive(appKey))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Registry may be briefly locked or a key removed mid-scan; treat as not in use
            }
            return false;
        }

        static bool SessionActive(RegistryKey key)
        {
            if (key == null) return false;
            object start = key.GetValue("LastUsedTimeStart");
            object stop = key.GetValue("LastUsedTimeStop");
            if (start == null || stop == null) return false;
            try
            {
                return Convert.ToInt64(stop) == 0 && Convert.ToInt64(start) != 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
