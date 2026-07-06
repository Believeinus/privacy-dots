using System;
using System.Globalization;
using System.IO;
using Microsoft.Win32;

namespace PrivacyDots
{
    public enum DotPosition
    {
        TopLeft = 0,
        TopCenter = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomRight = 4
    }

    public class AppSettings
    {
        public int DotSize = 12;                       // dot diameter in pixels
        public DotPosition Position = DotPosition.TopRight;
        public int Margin = 14;                        // distance from screen edge in pixels

        public const int MinDotSize = 6;
        public const int MaxDotSize = 40;
        public const int MaxMargin = 400;

        const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string RunValueName = "PrivacyDots";

        static string SettingsDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PrivacyDots"); }
        }

        static string SettingsFile
        {
            get { return Path.Combine(SettingsDir, "settings.ini"); }
        }

        public static AppSettings Load()
        {
            AppSettings s = new AppSettings();
            try
            {
                if (File.Exists(SettingsFile))
                {
                    foreach (string line in File.ReadAllLines(SettingsFile))
                    {
                        int eq = line.IndexOf('=');
                        if (eq <= 0) continue;
                        string key = line.Substring(0, eq).Trim();
                        string value = line.Substring(eq + 1).Trim();
                        int n;
                        if (key == "DotSize" && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                            s.DotSize = n;
                        else if (key == "Position" && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                            s.Position = (DotPosition)n;
                        else if (key == "Margin" && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                            s.Margin = n;
                    }
                }
            }
            catch
            {
                // fall back to defaults on any parse/IO problem
            }
            s.Clamp();
            return s;
        }

        public void Save()
        {
            try
            {
                Clamp();
                Directory.CreateDirectory(SettingsDir);
                File.WriteAllLines(SettingsFile, new string[]
                {
                    "DotSize=" + DotSize.ToString(CultureInfo.InvariantCulture),
                    "Position=" + ((int)Position).ToString(CultureInfo.InvariantCulture),
                    "Margin=" + Margin.ToString(CultureInfo.InvariantCulture)
                });
            }
            catch
            {
            }
        }

        void Clamp()
        {
            if (DotSize < MinDotSize) DotSize = MinDotSize;
            if (DotSize > MaxDotSize) DotSize = MaxDotSize;
            if (Margin < 0) Margin = 0;
            if (Margin > MaxMargin) Margin = MaxMargin;
            if (Position < DotPosition.TopLeft || Position > DotPosition.BottomRight) Position = DotPosition.TopRight;
        }

        public void CopyFrom(AppSettings other)
        {
            DotSize = other.DotSize;
            Position = other.Position;
            Margin = other.Margin;
        }

        public AppSettings Snapshot()
        {
            AppSettings copy = new AppSettings();
            copy.CopyFrom(this);
            return copy;
        }

        public static bool GetRunAtStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath))
                {
                    return key != null && key.GetValue(RunValueName) != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SetRunAtStartup(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
                {
                    if (key == null) return;
                    if (enabled)
                        key.SetValue(RunValueName, "\"" + System.Windows.Forms.Application.ExecutablePath + "\"");
                    else if (key.GetValue(RunValueName) != null)
                        key.DeleteValue(RunValueName);
                }
            }
            catch
            {
            }
        }
    }
}
