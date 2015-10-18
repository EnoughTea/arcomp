using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace Arcomp {
    /// <summary> Holds all available command line options. </summary>
    /// <remarks> Note that filenames are divided by space, so they must be quoted.</remarks>
    internal sealed class CommandLineOptions {
        [Option('s', "show", Required = false, DefaultValue = false,
            HelpText = "Shows properties and entries for the given archives.", MutuallyExclusiveSet = "show")]
        public bool Show { get; set; }

        [Option('c', "compare", Required = false, DefaultValue = false,
            HelpText = "Compares properties and entries of the given 2 archives.", MutuallyExclusiveSet = "compare")]
        public bool Compare { get; set; }

        /// <summary> Gets the passed archive files. </summary>
        [ValueList(typeof(List<string>))]
        public IList<string> ArchiveFiles { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage() {
            var help = new HelpText {
                Heading = "arcomp is a simple command-line utility used to compare archives or show their content.",
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            HandleParsingErrorsInHelp(help);
            help.AddPreOptionsLine("Password-protected archives are not supported.");
            help.AddPostOptionsLine(Environment.NewLine + "Usage examples");
            help.AddPostOptionsLine(string.Empty);
            help.AddPostOptionsLine("Shows properties and entries of one or more archives:");
            help.AddPostOptionsLine("arcomp -s \"archive 1.zip\" \"..\\another archive 2.rar\" \"C:\\some folder\\other archive 3.7z\"");
            help.AddPostOptionsLine(string.Empty);
            help.AddPostOptionsLine("Compares two archives and shows their difference in properties and entries:");
            help.AddPostOptionsLine("arcomp -c \"archive 1.zip\" \"..\\another archive 2.rar\"");
            help.AddOptions(this);

            return help;
        }

        private void HandleParsingErrorsInHelp(HelpText help) {
            if (LastParserState != null && LastParserState.Errors.Any()) {
                var errors = help.RenderParsingErrorsText(this, 2);
                if (!string.IsNullOrEmpty(errors)) {
                    help.AddPreOptionsLine(Environment.NewLine + "Error(s):");
                    help.AddPreOptionsLine(errors);
                }
            }
        }
    }
}
