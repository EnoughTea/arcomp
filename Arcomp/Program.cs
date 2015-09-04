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

        private static void ShowArchiveMetadata(IList<string> show) {
            foreach (var archive in show) {
                // TODO: support wildcards?
                string archiveName = archive;
                if (!Path.IsPathRooted(archive)) {
                    archiveName = Path.Combine(Directory.GetCurrentDirectory(), archive);
                }

                var fi = new FileInfo(archiveName);
                if (fi.Exists) {
                    var result = ExecuteSevenZipProcess("l -slt " + archiveName);
                    var archives = SevenZip.CreateFromOutput(result).ToArray();
                    var firstArchive = archives.FirstOrDefault();
                    if (firstArchive != null) {
                        ConsoleTools.Info(firstArchive.ToString());
                    }
                }

                ConsoleTools.WriteLine();
            }
        }

        private static void CompareArchives(IList<string> compare) {
            throw new NotImplementedException();
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
                FileName = Path.Combine(sevenZipPath, sevenZipExe),
                Arguments = arguments
            };

            using (var p = new Process { StartInfo = startInfo }) {
                p.Start();
                return ReadAllOutput(p.StandardOutput);
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
