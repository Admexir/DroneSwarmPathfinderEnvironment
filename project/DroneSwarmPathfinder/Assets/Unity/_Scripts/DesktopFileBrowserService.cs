using System;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DroneSwarmPathfinder.Unity.Services
{
    /// <summary>
    /// Desktop implementation of the file browser
    /// </summary>
    public class DesktopFileBrowserService : IFileBrowserService
    {
        public string RequestLoadPath(string title, string extension)
        {
#if UNITY_EDITOR
            return EditorUtility.OpenFilePanel(title, Application.dataPath, extension);
#else
            //TODO: add at least a linux implementation
            return WindowsFileDialog.ShowLoadDialog(title, Application.dataPath, extension);
#endif
        }

        public string RequestSavePath(string title, string defaultName, string extension)
        {
#if UNITY_EDITOR
            return EditorUtility.SaveFilePanel(title, Application.dataPath, defaultName, extension);
#else
            //TODO: add at least a linux implementation
            return WindowsFileDialog.ShowSaveDialog(title, Application.dataPath, defaultName, extension);
#endif
        }
    }

    // NATIVE WINDOWS IMPLEMENTATION, ai usage (I have no idea how the windows API for this works and couldn't figure it out from the docs... :) ): https://share.gemini.google/hR11QZ3MFVLI
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
    public static class WindowsFileDialog
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public string filter = null;
            public string customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public string file = null;
            public int maxFile = 0;
            public string fileTitle = null;
            public int maxFileTitle = 0;
            public string initialDir = null;
            public string title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public string defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public string templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

        private const int OFN_EXPLORER = 0x00080000;
        private const int OFN_NOCHANGEDIR = 0x00000008; // Crucial for Unity so it doesn't break relative paths
        private const int OFN_OVERWRITEPROMPT = 0x00000002;

        public static string ShowLoadDialog(string title, string directory, string extension)
        {
            OpenFileName ofn = CreateDefaultStruct(title, directory, extension, "");
            ofn.flags = OFN_EXPLORER | OFN_NOCHANGEDIR;

            if (GetOpenFileName(ofn))
                return ofn.file;
            return string.Empty;
        }

        public static string ShowSaveDialog(string title, string directory, string defaultName, string extension)
        {
            OpenFileName ofn = CreateDefaultStruct(title, directory, extension, defaultName);
            ofn.flags = OFN_EXPLORER | OFN_NOCHANGEDIR | OFN_OVERWRITEPROMPT;

            if (GetSaveFileName(ofn))
                return ofn.file;
            return string.Empty;
        }

        private static OpenFileName CreateDefaultStruct(string title, string directory, string extension, string defaultName)
        {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);

            string filterExt = string.IsNullOrEmpty(extension) ? "*.*" : "*." + extension;
            string filterName = string.IsNullOrEmpty(extension) ? "All Files" : extension.ToUpper() + " Files";
            
            // Filters in Windows use null characters \0 to separate the label and the actual filter
            ofn.filter = $"{filterName}\0{filterExt}\0All Files\0*.*\0";
            
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            
            if (!string.IsNullOrEmpty(defaultName))
            {
                ofn.file = defaultName.PadRight(256, '\0');
            }

            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = directory;
            ofn.title = title;
            ofn.defExt = extension;

            return ofn;
        }
    }
#endif
}