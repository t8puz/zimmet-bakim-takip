using System;
using System.IO;

namespace Zimmet_Bakim_Takip.Utilities
{
    public static class ShortcutHelper
    {
        public static bool CreateDesktopShortcut(string targetExePath, string shortcutName, string? iconPath = null, string? description = null)
        {
            string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            return CreateShortcut(Path.Combine(desktopDir, $"{shortcutName}.lnk"), targetExePath, iconPath, description);
        }

        public static bool CreateStartMenuShortcut(string targetExePath, string shortcutName, string? iconPath = null, string? description = null)
        {
            string startMenuDir = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string programsDir = Path.Combine(startMenuDir, "Programs", "Zimmet Bakım Takip");
            Directory.CreateDirectory(programsDir);
            return CreateShortcut(Path.Combine(programsDir, $"{shortcutName}.lnk"), targetExePath, iconPath, description);
        }

        private static bool CreateShortcut(string shortcutPath, string targetExePath, string? iconPath, string? description)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                {
                    return false;
                }

                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetExePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetExePath);
                shortcut.WindowStyle = 1;
                shortcut.Description = description ?? "Zimmet Bakım Takip Sistemi";
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconPath) ? targetExePath : iconPath;
                shortcut.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

