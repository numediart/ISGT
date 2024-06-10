using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils
{
    /// <summary>
    ///  Folder dialog class to open a folder dialog on Windows, Mac, and Linux
    ///  This class is not supported on other platforms like Android, iOS, WebGL, etc.
    /// </summary>
    public class FolderDialog : MonoBehaviour
    {
        /// <summary>
        ///  Open a folder dialog to select a folder path on Windows, Mac, and Linux platforms
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public string OpenFolderDialog(string title)
        {
#if UNITY_EDITOR
            // Unity Editor uses its own dialog
            return EditorUtility.OpenFolderPanel(title, "", "");
#elif UNITY_STANDALONE_WIN
        return OpenFolderDialogWindows(title);
#elif UNITY_STANDALONE_OSX
        return OpenFolderDialogMac(title);
#elif UNITY_STANDALONE_LINUX
        return OpenFolderDialogLinux(title);
#else
        throw new PlatformNotSupportedException("This platform is not supported.");
#endif
        }

#if UNITY_STANDALONE_WIN
        /// <summary>
        /// BROWSEINFO struct for Windows platform to open a folder dialog
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }
        /// <summary>
        ///  Use shell32.dll to open a folder dialog on Windows platform
        /// </summary>
        /// <param name="bi"></param>
        /// <returns></returns>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO bi);

        /// <summary>
        ///  Use shell32.dll to get the path from the IDList on Windows platform
        /// </summary>
        /// <param name="pidl"></param>
        /// <param name="pszPath"></param>
        /// <returns></returns>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);
        

        /// <summary>
        ///  Get the active window handle
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        /// <summary>
        ///  Open a folder dialog on Windows platform
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string OpenFolderDialogWindows(string title)
        {
            BROWSEINFO bi = new BROWSEINFO();
            bi.hwndOwner = GetActiveWindow();
            bi.pidlRoot = IntPtr.Zero;
            bi.pszDisplayName = Marshal.AllocHGlobal(260);
            bi.lpszTitle = title;
            bi.ulFlags = 0x00000040; // BIF_RETURNONLYFSDIRS
            bi.lpfn = IntPtr.Zero;
            bi.lParam = IntPtr.Zero;
            bi.iImage = 0;

            IntPtr pidl = SHBrowseForFolder(ref bi);

            if (pidl != IntPtr.Zero)
            {
                StringBuilder path = new StringBuilder(260);
                bool success = SHGetPathFromIDList(pidl, path);
                Marshal.FreeHGlobal(bi.pszDisplayName);
                if (success)
                {
                    return path.ToString();
                }
            }

            Marshal.FreeHGlobal(bi.pszDisplayName);
            return null;
        }
#endif
        // part for Mac and Linux platforms are not tested yet and may not work as expected
#if UNITY_STANDALONE_OSX
    [DllImport("__Internal")]
    private static extern IntPtr NSOpenPanel();

    [DllImport("__Internal")]
    private static extern void NSOpenPanelRelease(IntPtr panel);

    [DllImport("__Internal")]
    private static extern bool NSOpenPanelRunModal(IntPtr panel);

    [DllImport("__Internal")]
    private static extern IntPtr NSOpenPanelURL(IntPtr panel);

    private string OpenFolderDialogMac(string title)
    {
        IntPtr panel = NSOpenPanel();
        try
        {
            if (NSOpenPanelRunModal(panel))
            {
                IntPtr url = NSOpenPanelURL(panel);
                if (url != IntPtr.Zero)
                {
                    return Marshal.PtrToStringAuto(url);
                }
            }
        }
        finally
        {
            NSOpenPanelRelease(panel);
        }
        return null;
    }
#endif

#if UNITY_STANDALONE_LINUX
    private string OpenFolderDialogLinux(string title)
    {
        string scriptPath = "/tmp/OpenFolderDialog.sh";
        string script = $"#!/bin/bash\nzenity --file-selection --directory --title=\"{title}\"";

        System.IO.File.WriteAllText(scriptPath, script);
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = scriptPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadLine();
        process.WaitForExit();
        System.IO.File.Delete(scriptPath);
        return string.IsNullOrEmpty(result) ? null : result;
    }
#endif
    }
}