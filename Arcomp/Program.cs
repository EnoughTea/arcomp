using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchiveCompare;
using CommandLine;

namespace Arcomp {
    class Program {
        private static readonly string ArchiveStartMark = Environment.NewLine + "Listing archive: ";

        static void Main(string[] args) {
            //AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            var options = new CommandLineOptions();
            if (!Parser.Default.ParseArgumentsStrict(args, options)) { Environment.Exit(1); }

            string archiveName = Path.Combine(Directory.GetCurrentDirectory(), "*");
            var result = ExecuteSevenZipProcess("l " + archiveName); // "-slt"
            Console.WriteLine(result);
            var archives = SevenZip.CreateFromOutput(result).ToArray();
            var first = archives.FirstOrDefault();
            Console.ReadKey();
        }

        /// <summary> Executes the 7-Zip with given command line arguments, returns its stdout as a string. </summary>
        /// <param name="arguments">Command line arguments passed to 7-Zip.</param>
        /// <returns>7-Zip stdout after execution.</returns>
        private static string ExecuteSevenZipProcess(string arguments = "") {
            const string sevenZipPath = "7z\\";
            const string sevenZipExe = "7z.exe";
            var p = new Process {
                StartInfo = {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = Path.Combine(sevenZipPath, sevenZipExe),
                    Arguments = arguments
                }
            };
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs arg) {
            var e = arg.ExceptionObject as Exception;
            if (e != null) {
                Console.WriteLine(e.ToString());
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }
}
