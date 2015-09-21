using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace Arcomp {
    /// <summary> Holds all available command line options. </summary>
    /// <remarks> Note that filenames are divided by ':' symbol instead of space.</remarks>
    internal sealed class CommandLineOptions {
        [OptionList('s', "show", Required = false, DefaultValue = new string[0],
            HelpText = "Shows properties and entries for the given archives.", MutuallyExclusiveSet = "show")]
        public IList<string> Show { get; set; }

        [OptionList('c', "compare", Required = false, DefaultValue = new string[0],
            HelpText = "Compares properties and entries of the given 2 archives.", MutuallyExclusiveSet = "compare")]
        public IList<string> Compare { get; set; }

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
            help.AddPreOptionsLine("Please, note that filenames are divided by ':' instead of space. " +
                " Password-protected archives are not supported.");
            help.AddPreOptionsLine(Environment.NewLine + "Usage examples:");
            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine("Show properties and entries of 3 archives using different paths:");
            help.AddPreOptionsLine("Usage: arcomp -s archive 1.zip:..\\archive 2.rar:C:\\Some folder\\some archive.7z");
            help.AddPreOptionsLine(string.Empty);
            help.AddPreOptionsLine("Compares two archives and shows the difference in properties and entries:");
            help.AddPreOptionsLine("Usage: arcomp -c archive 1.zip:archive 2.rar");
            help.AddOptions(this);

            return help;
        }

        private void HandleParsingErrorsInHelp(HelpText help) {
            if (LastParserState.Errors.Any()) {
                var errors = help.RenderParsingErrorsText(this, 2);
                if (!string.IsNullOrEmpty(errors)) {
                    help.AddPreOptionsLine(Environment.NewLine + "Error(s):");
                    help.AddPreOptionsLine(errors);
                }
            }
        }
    }
}
