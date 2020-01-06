using GameLauncherReborn;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;

namespace GameLauncher.App.Classes
{
    class HashChecker
    {
        private static List<byte>[] Entities =
        {
            Self.StrToBytes("SHA1"),
            Self.StrToBytes("game"),
            Self.StrToBytes("launcher"),
            Self.StrToBytes("folder"),
            Self.StrToBytes("https://damp-forest-50246.herokuapp.com/sha_knower?target=")
        };
        private static List<byte>[] ExceptionMessages = 
        {
            Self.StrToBytes("Game complexity exception"),
            Self.StrToBytes("Launcher complexity exception")/*,
            Self.StrToBytes("SBRW complexity exception")*/ 
        };
        //private static byte[] HashDirectory(DirectoryInfo directoryInfo)
        //{
        //    using (var hashAlgorithm = HashAlgorithm.Create(HAName))
        //    {
        //        using (var cryptoStream = new CryptoStream(Stream.Null, hashAlgorithm, CryptoStreamMode.Write))
        //        using (var binaryWriter = new BinaryWriter(cryptoStream))
        //        {
        //            FileSystemInfo[] infos = directoryInfo.GetFileSystemInfos();
        //            Array.Sort(infos, (a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        //            foreach (FileSystemInfo info in infos)
        //            {
        //                if ((info.Attributes & FileAttributes.Directory) == 0 && (info.Name.ToLower().Contains("dll") || info.Name.ToLower().Contains("exe")))
        //                {
        //                    binaryWriter.Write(info.Name);
        //                    binaryWriter.Write((byte)'F');
        //                    binaryWriter.Write(HashFile((FileInfo)info));
        //                }
        //            }
        //        }
        //        return hashAlgorithm.Hash;
        //    }
        //}

        private static byte[] HashFile(FileInfo fileInfo)
        {
            using (var hashAlgorithm = HashAlgorithm.Create(Self.StrFromSec(Entities[0])))
            using (var inputStream = fileInfo.OpenRead())
            {
                return hashAlgorithm.ComputeHash(inputStream);
            }
        }

        private static byte[] GetGameHash(string gamePath) => HashFile(new FileInfo(gamePath));
        private static byte[] GetLauncherHash(string launcherPath) => HashFile(new FileInfo(launcherPath));
        //private static byte[] GetLauncherFolderHash(string launcherFolderPath) => HashDirectory(new DirectoryInfo(launcherFolderPath));

        private static bool CheckHash(string target, byte[] localHash)
        {
            string remoteHash;
            using (var wc = new WebClient()) remoteHash = wc.DownloadString($"{Self.StrFromSec(Entities[4])}{target}");
            return (target.ToLower().Contains(Self.StrFromSec(Entities[3])) ? BitConverter.ToString(localHash).Replace(" - ", "") : Convert.ToBase64String(localHash)) == remoteHash.TrimEnd(Environment.NewLine.ToCharArray());
        }

        public static void CheckGame(string gamePath)
        {
            if (!CheckHash(Self.StrFromSec(Entities[1]), GetGameHash(gamePath)))
            {
                throw new Exception(Self.StrFromSec(ExceptionMessages[0]));
            }
        }

        public static void CheckLauncher(string launcherPath)
        {
            if (!CheckHash(Self.StrFromSec(Entities[2]), GetLauncherHash(launcherPath)))
            {
                throw new Exception(Self.StrFromSec(ExceptionMessages[1]));
            }
        }

        //public static void CheckLauncherFolder(string launcherFolderPath)
        //{
        //    if (!CheckHash($"sbrw_{Self.StrFromSec(Entities[3])}", GetLauncherFolderHash(launcherFolderPath)))
        //    {
        //        throw new Exception(Self.StrFromSec(ExceptionMessages[2]));
        //    }
        //}
    }
    class AntiCheat
    {
        private static List<byte>[] Entities =
        {
            Self.StrToBytes("GameFrame"),
            Self.StrToBytes("ac.dll"),
            Self.StrToBytes("NextHook"),
            Self.StrToBytes("anticheatlog.txt"),
            Self.StrToBytes("https://damp-forest-50246.herokuapp.com/white_list"),
            Self.StrToBytes("http://launcher.worldunited.gg/report")
        };
        public static IntPtr SetHook()
        {
            uint pid = 0;
            Process process = Process.GetProcessById(process_id);
            uint tid = User32.GetWindowThreadProcessId(User32.FindWindow(Self.StrFromSec(Entities[0]), process.MainWindowTitle), out pid);
            IntPtr LibHandle = Kernel32.LoadLibrary(Self.StrFromSec(Entities[1]));
            IntPtr Hook = User32.SetWindowsHookEx(3, Kernel32.GetProcAddress(LibHandle, Self.StrFromSec(Entities[2])), LibHandle, tid);
            return Hook;
        }
        public static int process_id = 0;

        public static string serverip = String.Empty;
        public static string user_id = String.Empty;
        public static string persona_name = String.Empty;
        public static int event_id = 0;
        public static bool cheats_detected = false;

        public static void enableChecks()
        {
            Process process = Process.GetProcessById(process_id);
            string contents;
            using (var wc = new WebClient()) contents = wc.DownloadString(Self.StrFromSec(Entities[4]));
            List<string> knownModules = contents.Split(' ').ToList();
            var thread = new Thread(() => {
                while (true)
                {
                    if (process.Modules.Count > knownModules.Count)
                    {
                        List<string> unknownModules = new List<string>();
                        for (int i = 0; i < process.Modules.Count; i++)
                        {
                            bool knownFlag = false;
                            foreach (string moduleName in knownModules)
                            {
                                if (process.Modules[i].FileName == moduleName) knownFlag = true;
                            }
                            if (knownFlag) File.AppendAllText("unknown", $@"{process.Modules[i].FileName}{Environment.NewLine}");
                        }
                    }
                    Thread.Sleep(100);
                }
            }) { IsBackground = true };
            thread.Start();
        }

        public static void disableChecks() {
            string[] logContent = File.ReadAllText(Self.StrFromSec(Entities[3])).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string strContent in logContent)
            {
                if (strContent.Contains("detect"))
                {
                    cheats_detected = true;
                    break;
                }
            }
            if (cheats_detected) {
                //Not nice. Send to global registry of cheaters.

                String responseString;
                try
                {
                    Uri sendReport = new Uri(Self.StrFromSec(Entities[5]));
                    var request = (HttpWebRequest)WebRequest.Create(sendReport);
                    var postData = "serverip=" + AntiCheat.serverip + "&user_id=" + AntiCheat.user_id + "&persona_name=" + AntiCheat.persona_name + "&event_session=" + AntiCheat.event_id + "&cheat_type=" + AntiCheat.cheats_detected + "&hwid=" + Security.FingerPrint.Value();
                    var data = Encoding.ASCII.GetBytes(postData);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    var response = (HttpWebResponse)request.GetResponse();
                    responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                }
                catch { }
            }
            cheats_detected = false;
        }
    }
}
