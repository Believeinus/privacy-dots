using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace PrivacyDots
{
    // Reads the Windows Capability Access Manager consent store. Windows tracks
    // every app's camera/microphone sessions there: while a device is in use,
    // the app's LastUsedTimeStop value is 0. This is the same data source the
    // built-in Windows 10/11 mic/camera taskbar indicators use, so no drivers
    // or audio/video APIs are needed and the check is cheap enough to poll.
    //
    // The store can contain STALE entries: if an app crashes, the machine loses
    // power mid-use, or (notably on Windows 10) the stop timestamp is simply
    // never written, LastUsedTimeStop stays 0 forever and would show the dot
    // permanently. So an entry only counts if the app it names is still running.
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
            // Subkey names: package family name for Store apps, exe path with
            // '\' replaced by '#' for classic desktop apps (under NonPackaged).
            List<string> packagedApps = new List<string>();
            List<string> desktopApps = new List<string>();
            CollectActiveEntries(Registry.CurrentUser, capability, packagedApps, desktopApps);
            CollectActiveEntries(Registry.LocalMachine, capability, packagedApps, desktopApps);

            if (packagedApps.Count == 0 && desktopApps.Count == 0) return false;

            foreach (string keyName in desktopApps)
            {
                if (DesktopAppRunning(keyName)) return true;
            }
            return packagedApps.Count > 0 && AnyPackagedAppRunning(packagedApps);
        }

        static void CollectActiveEntries(RegistryKey hive, string capability,
            List<string> packagedApps, List<string> desktopApps)
        {
            try
            {
                using (RegistryKey root = hive.OpenSubKey(BasePath + capability))
                {
                    if (root == null) return;
                    foreach (string name in root.GetSubKeyNames())
                    {
                        using (RegistryKey appKey = root.OpenSubKey(name))
                        {
                            if (appKey == null) continue;
                            if (string.Equals(name, "NonPackaged", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (string sub in appKey.GetSubKeyNames())
                                {
                                    using (RegistryKey desktopApp = appKey.OpenSubKey(sub))
                                    {
                                        if (SessionActive(desktopApp) && !desktopApps.Contains(sub))
                                            desktopApps.Add(sub);
                                    }
                                }
                            }
                            else if (SessionActive(appKey) && !packagedApps.Contains(name))
                            {
                                packagedApps.Add(name);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Registry may be briefly locked or a key removed mid-scan; skip
            }
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

        // NonPackaged subkey names encode the exe path, e.g.
        // "C:#Program Files#App#app.exe". The entry is only live if a process
        // with that image name is running. When in doubt, err on showing the
        // dot - a false alarm beats a missed one for a privacy indicator.
        static bool DesktopAppRunning(string keyName)
        {
            try
            {
                int i = keyName.LastIndexOf('#');
                string exe = (i >= 0) ? keyName.Substring(i + 1) : keyName;
                if (exe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    exe = exe.Substring(0, exe.Length - 4);
                if (exe.Length == 0) return true;

                Process[] procs = Process.GetProcessesByName(exe);
                bool running = procs.Length > 0;
                foreach (Process p in procs) p.Dispose();
                return running;
            }
            catch
            {
                return true;
            }
        }

        const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPackageFamilyName(IntPtr hProcess, ref uint packageFamilyNameLength, StringBuilder packageFamilyName);

        // Packaged (Store app) subkey names are package family names. Ask each
        // running process for its package family name and look for a match.
        // Only called while at least one packaged entry claims to be in use.
        static bool AnyPackagedAppRunning(List<string> familyNames)
        {
            Process[] procs;
            try
            {
                procs = Process.GetProcesses();
            }
            catch
            {
                return true; // cannot verify - err on showing the dot
            }
            try
            {
                StringBuilder buffer = new StringBuilder(1024);
                foreach (Process p in procs)
                {
                    IntPtr handle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, p.Id);
                    if (handle == IntPtr.Zero) continue;
                    try
                    {
                        uint length = (uint)buffer.Capacity;
                        buffer.Length = 0;
                        if (GetPackageFamilyName(handle, ref length, buffer) == 0 && buffer.Length > 0)
                        {
                            string family = buffer.ToString();
                            foreach (string wanted in familyNames)
                            {
                                if (string.Equals(family, wanted, StringComparison.OrdinalIgnoreCase))
                                    return true;
                            }
                        }
                    }
                    finally
                    {
                        CloseHandle(handle);
                    }
                }
                return false;
            }
            catch
            {
                return true; // cannot verify - err on showing the dot
            }
            finally
            {
                foreach (Process p in procs) p.Dispose();
            }
        }
    }
}
