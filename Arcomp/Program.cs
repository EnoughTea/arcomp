using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using ArchiveCompare;
using CommandLine;

namespace Arcomp {
    class Program {
        static void Main(string[] args) {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
#endif
            var options = new CommandLineOptions();
            if (Parser.Default.ParseArgumentsStrict(args, options)) {
                if (options.Show.Any()) {
                    ShowArchiveMetadata(options.Show);
                } else if (options.Compare.Any()) {
                    CompareArchives(options.Compare);
                }

                Console.ReadKey();
            }
        }

        private static Archive[] CreateArchivesFromFiles(FileInfo[] archiveFiles) {
            StringBuilder totalOutput = new StringBuilder();
            foreach (var archiveFile in archiveFiles) {
                var fileOutput = ExecuteSevenZipProcess("l -slt " + archiveFile);
                totalOutput.Append(fileOutput);
            }

            return SevenZip.ArchivesFromOutput(totalOutput.ToString()).ToArray();
        }

        private static void ShowArchiveMetadata(IList<string> show) {
            var archiveFiles = PathTools.GatherFiles(show).ToArray();
            var archives = CreateArchivesFromFiles(archiveFiles);
            foreach (var archive in archives) {
                ConsoleTools.Info(archive.ToString());
                ConsoleTools.WriteLine();
            }
        }

        private static void CompareArchives(IList<string> compare) {
            var archiveFiles = PathTools.GatherFiles(compare).ToArray();
            var archives = CreateArchivesFromFiles(archiveFiles);
            if (archives.Length == 2) {
                var left = archives[0];
                var right = archives[1];
                var propertiesDiff = Archive.PropertiesDiff(left, right);
                var entriesDiff = Archive.EntriesDiff(left, right)
                    .OrderBy(cmp => cmp.EntryType)
                    .ThenBy(cmp => cmp.LeftVersion?.Path ?? (cmp.RightVersion?.Path ?? string.Empty));

                ConsoleTools.Info("Properties diff");
                ConsoleTools.WriteLine("===============");
                ConsoleTools.WriteLine();
                foreach (var propertyDiff in propertiesDiff) {
                    ConsoleTools.WriteLine(propertyDiff.ToString());
                }
                ConsoleTools.WriteLine();
                ConsoleTools.WriteLine();
                ConsoleTools.Info("Entries diff");
                ConsoleTools.Info("============");
                ConsoleTools.WriteLine();
                foreach (var entryDiff in entriesDiff) {
                    ConsoleTools.WriteLine(entryDiff.ToString());
                    ConsoleTools.WriteLine();
                }

            } else {
                ConsoleTools.Error($"Expected 2 archives to be compared, got {archiveFiles.Length}.");
            }
        }

        /// <summary> Executes the 7-Zip with given command line arguments, returns its stdout as a string. </summary>
        /// <param name="arguments">Command line arguments passed to 7-Zip.</param>
        /// <returns>7-Zip stdout after execution.</returns>
        private static string ExecuteSevenZipProcess(string arguments = "") {
            const string sevenZipPath = "7z\\";
            const string sevenZipExe = "7z.exe";
            var startInfo = new ProcessStartInfo {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,   // 7z can ask for password.
                FileName = Path.Combine(sevenZipPath, sevenZipExe),
                Arguments = arguments
            };
            // Yeah, obviously password-protected archives are not supported.
            using (var p = new Process { StartInfo = startInfo }) {
                p.Start();
                p.StandardInput.AutoFlush = true;
                string output = string.Empty;
                while (!p.HasExited) {
                    output += ReadAllOutput(p.StandardOutput);
                    p.StandardInput.WriteLine("dummy line in case 7z is asking for password");
                }

                return output;
            }
        }

        private static string ReadAllOutput(TextReader textReader) {
            Contract.Requires(textReader != null);

            // Using stream.Read instead of ReadLine/ReadToEnd is required not to block on input prompts and such.
            const int readBufferSize = 1024;
            var sb = new StringBuilder();
            char[] buffer = new char[readBufferSize];
            while (textReader.Peek() > -1) {
                int charsRead = textReader.Read(buffer, 0, buffer.Length);
                sb.Append(buffer, 0, charsRead);
            }

            return sb.ToString();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs arg) {
            var e = arg.ExceptionObject as Exception;
            if (e != null) {
                ConsoleTools.Exception(e);
            }
#if DEBUG
            Console.ReadKey();
#endif
            Environment.Exit(1);
        }
    }
}
