// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace GenericHelpers
{


    public static class AssembliesMirror
    {

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local
        private enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_WRITE_THROUGH = 8
        }
        // ReSharper restore UnusedMember.Local
        // ReSharper restore InconsistentNaming

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "MoveFileEx")]
        private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

        public static string MainDllPath { get; private set; }

        public static ReadOnlyCollection<string> AllDlls { get; private set; }

        public static void Initialize(string binPath, params string[] otherPaths)
        {
            var dlls = new List<string>();

            AllDlls = new ReadOnlyCollection<string>(new List<string>());
            var reversed = otherPaths.Reverse();
            MainDllPath = binPath;
            if (string.IsNullOrWhiteSpace(MainDllPath))
            {
                var tmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tmpPath);
                MainDllPath = tmpPath;
                MoveFileEx(tmpPath, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
            }
            else
            {
                Directory.CreateDirectory(MainDllPath);
            }
            foreach (var dir in reversed)
            {
                var addedDlls = LoadDlls(dir);
                dlls.AddRange(addedDlls);
            }
            AllDlls = new ReadOnlyCollection<string>(dlls);
        }

        private static List<string> LoadDlls(string dir)
        {
            var dlls = new List<string>();
            foreach (var dllFile in Directory.EnumerateFiles(dir, "*.dll"))
            {
                var fileName = Path.GetFileName(dllFile);
                if (fileName == null) continue;
                var resultFile = Path.Combine(MainDllPath, fileName);
                if (File.Exists(resultFile)) continue;
                File.Copy(dllFile, resultFile);
                MoveFileEx(resultFile, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
                dlls.Add(resultFile);
            }
            dlls.Reverse();
            return dlls;
        }

        public static void InitializeAppDomain(AppDomain appDomain)
        {
            appDomain.SetupInformation.PrivateBinPath = MainDllPath;
        }

    }
}
