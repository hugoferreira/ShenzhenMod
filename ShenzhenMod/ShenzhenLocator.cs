﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Win32;

namespace ShenzhenMod
{
    public static class ShenzhenLocator
    {
        private static readonly log4net.ILog sm_log = log4net.LogManager.GetLogger(typeof(ShenzhenLocator));

        public static string FindShenzhenDirectory()
        {
            string steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) as string;
            sm_log.InfoFormat("Steam install path: \"{0}\"", steamPath);
            if (steamPath != null)
            {
                var shenzhenDir = Path.Combine(steamPath, @"steamapps\common\SHENZHEN IO");
                if (Directory.Exists(shenzhenDir))
                {
                    sm_log.InfoFormat("Found SHENZHEN I/O directory: \"{0}\"", shenzhenDir);
                    return shenzhenDir;
                }
                else
                {
                    sm_log.InfoFormat("Could not find SHENZHEN I/O directory: directory \"{0}\" does not exist", shenzhenDir);
                }
            }

            return null;
        }

        public static string FindUnpatchedShenzhenExecutable(string shenzhenDir)
        {
            sm_log.InfoFormat("Looking for unpatched SHENZHEN I/O executable in \"{0}\"", shenzhenDir);
            const string unpatchedHash = "1D-65-40-5A-63-82-77-4F-2E-99-F2-00-B0-59-9B-4D-B3-71-2B-1B-C1-01-8A-4B-D6-02-C4-7A-8B-11-DB-E8";
            string path = FindExecutableWithHash(shenzhenDir, unpatchedHash);
            if (path == null)
            {
                throw new Exception("Cannot locate unpatched SHENZHEN I/O executable");
            }

            sm_log.InfoFormat("Found unpatched SHENZHEN I/O executable: \"{0}\"", path);
            return path;
        }

        private static string FindExecutableWithHash(string dir, string expectedHash)
        {
            foreach (string file in Directory.GetFiles(dir, "*.exe"))
            {
                try
                {
                    string hash = CalculateHash(file);
                    if (hash == expectedHash)
                    {
                        return file;
                    }
                }
                catch (Exception e)
                {
                    sm_log.InfoFormat("Error calculating hash for file \"{0}\": {1}", file, e.Message);
                }
            }

            return null;
        }

        private static string CalculateHash(string file)
        {
            sm_log.InfoFormat("Calculating hash for \"{0}\"", file);
            using (var stream = File.OpenRead(file))
            {
                var sha = SHA256.Create();
                byte[] hash = sha.ComputeHash(stream);
                string hashString = BitConverter.ToString(hash);

                sm_log.InfoFormat("Calculated hash: \"{0}\"", hashString);
                return hashString;
            }
        }
    }
}