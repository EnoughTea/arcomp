﻿using System;
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
            bool debug = Debugger.IsAttached;
            if (!debug) { AppDomain.CurrentDomain.UnhandledException += OnUnhandledException; }

            var options = new CommandLineOptions();
            if (Parser.Default.ParseArgumentsStrict(args, options)) {
                if (options.Show) {
                    ShowArchiveMetadata(options.ArchiveFiles);
                } else if (options.Compare) {
                    CompareArchives(options.ArchiveFiles);
                }

                if (debug) { Console.ReadKey(); }
            }
        }

        private static void ShowArchiveMetadata(IList<string> show) {
            var archiveFiles = PathTools.GatherFiles(show).ToArray();
            var archives = CreateArchivesFromFiles(archiveFiles);
            if (archives.Count == 0) {
                ConsoleTools.Error("Expected one or more archives to be shown, got 0.");
                return;
            }

            foreach (var archive in archives) {
                // Show archive properties:
                ConsoleTools.Info(archive.ToString());
                // and entries:
                if (archive.Contents.Any()) {
                    ConsoleTools.WriteLine("{");
                    ConsoleTools.Indent();
                    foreach (var entry in archive.Contents) {
                        ConsoleTools.WriteLine(entry.ToString());
                    }

                    ConsoleTools.Unindent();
                    ConsoleTools.WriteLine("}");
                }

                ConsoleTools.WriteLine();
            }
        }

        private static void CompareArchives(IList<string> compare) {
            var archiveFiles = PathTools.GatherFiles(compare).ToArray();
            var archives = CreateArchivesFromFiles(archiveFiles);
            if (archives.Count != 2) {
                ConsoleTools.Error($"Expected 2 archives to be compared, got {archiveFiles.Length}.");
                return;
            }

            var left = archives[0];
            var right = archives[1];
            var propertiesDiff = Archive.PropertiesDiff(left, right).ToArray();
            // Sort entries so folders will be first, and files second, and then sort them again by filename:
            var entriesDiff = Archive.EntriesDiff(left, right)
                .OrderByDescending(cmp => cmp.EntryType)
                .ThenBy(cmp => cmp.LeftVersion?.Path ?? (cmp.RightVersion?.Path ?? string.Empty)).ToArray();

            // Show properties diff:
            string header = $"Property difference between '{left.Path}' and '{right.Path}'";
            ConsoleTools.Info(header);
            ConsoleTools.Info("=".Repeat(header.Length));
            ConsoleTools.Indent();
            if (propertiesDiff.Any()) {
                foreach (var propertyDiff in propertiesDiff) {
                    ConsoleTools.WriteLine(propertyDiff.ToString());
                }
            } else {
                ConsoleTools.Info("Properties are identical.");
            }

            ConsoleTools.WriteLine(2);
            ConsoleTools.Unindent();

            // Show entries diff:
            header = $"State of '{right.Path}' entries compared to '{left.Path}'";
            ConsoleTools.Info(header);
            ConsoleTools.Info("=".Repeat(header.Length));
            ConsoleTools.Indent();
            if (entriesDiff.Any()) {
                foreach (var entryDiff in entriesDiff) {
                    if (entryDiff.State == EntryModificationState.Added) {
                        ConsoleTools.PushForeground(ConsoleColor.Green);
                    } else if (entryDiff.State == EntryModificationState.Removed) {
                        ConsoleTools.PushForeground(ConsoleColor.Red);
                    } else {
                        ConsoleTools.PushForeground(ConsoleColor.Yellow);
                    }

                    ConsoleTools.WriteLine(entryDiff.ToString());
                    ConsoleTools.PopForegroundOnce();
                }
            } else {
                ConsoleTools.Info("Entries are identical.");
            }

            ConsoleTools.Unindent();
        }

        /// <summary> Used to parse 7-Zip output for given files. </summary>
        /// <param name="archiveFiles">Archive files to parse.</param>
        /// <returns>Parsed archives.</returns>
        private static List<Archive> CreateArchivesFromFiles(IEnumerable<FileInfo> archiveFiles) {
            List<Archive> created = new List<Archive>();
            var sevenZipOutput = new StringBuilder();
            foreach (var archiveFile in archiveFiles) {
                if (archiveFile.Extension == ".bsa") {
                    string path = archiveFile.FullName;
                    using (var bsaData = File.OpenRead(path)) {
                        var archive = Bsa.ArchiveFromStream(path, archiveFile.LastWriteTimeUtc, bsaData);
                        if (archive != null) {
                            created.Add(archive);
                        }
                    }
                } else {
                    sevenZipOutput.Append(ExecuteSevenZipProcess("l -slt " + archiveFile));
                }
            }

            if (sevenZipOutput.Length > 0) {
                created.AddRange(SevenZip.ArchivesFromOutput(sevenZipOutput.ToString()));
            }

            return created;
        }

        /// <summary> Executes the 7-Zip with given command line arguments, returns its stdout as a string. </summary>
        /// <remarks> Password-protected archives are not supported. </remarks>
        /// <param name="arguments">Command line arguments passed to 7-Zip.</param>
        /// <param name="pathTo7Z">The path to 7-Zip console executable.</param>
        /// <returns> 7-Zip stdout after execution. </returns>
        private static string ExecuteSevenZipProcess(string arguments = "", string pathTo7Z = null) {
            if (string.IsNullOrWhiteSpace(pathTo7Z)) {
                pathTo7Z = "7z" + Path.DirectorySeparatorChar + "7z.exe";
            }

            var startInfo = new ProcessStartInfo {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,   // 7z can ask for password.
                FileName = pathTo7Z,
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

            Environment.Exit(1);
        }
    }
}
