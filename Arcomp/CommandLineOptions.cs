using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace Arcomp {
    /// <summary> Holds all available command line options. </summary>
    internal sealed class CommandLineOptions {
        [OptionList('s', "show", Required = false, DefaultValue = new string[0],
            HelpText = "Shows archive metadata.", MutuallyExclusiveSet = "show")]
        public IList<string> Show { get; set; }

        [OptionList('c', "compare", Required = false, DefaultValue = new string[0],
            HelpText = "Compares two archives.", MutuallyExclusiveSet = "compare")]
        public IList<string> Compare { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage() {
            var help = new HelpText {
                Heading = "arcomp: way to compare archives content.",
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            HandleParsingErrorsInHelp(help);
            help.AddPreOptionsLine("Usage: arcomp -l *");
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
