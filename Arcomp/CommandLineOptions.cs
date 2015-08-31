using System;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace Arcomp {
    /// <summary> Holds all available command line options. </summary>
    internal sealed class CommandLineOptions {
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
            help.AddPreOptionsLine("Usage: arcomp /TODO");
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
