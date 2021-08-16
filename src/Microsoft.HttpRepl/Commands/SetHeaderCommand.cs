// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Resources;
using Microsoft.HttpRepl.Suggestions;
using Microsoft.HttpRepl.Telemetry;
using Microsoft.HttpRepl.Telemetry.Events;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl.Commands
{
    public class SetHeaderCommand : ICommand<HttpState, ICoreParseResult>
    {
        private const string CommandName = "set";
        private const string SubCommand = "header";

        private readonly ITelemetry _telemetry;

        public string Name => "setHeader";
        public static string Description => Strings.SetHeaderCommand_HelpSummary;

        public SetHeaderCommand(ITelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        public bool? CanHandle(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            return parseResult.ContainsAtLeast(minimumLength: 3, CommandName, SubCommand)
                ? (bool?)true
                : null;
        }

        public Task ExecuteAsync(IShellState shellState, HttpState programState, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            bool isValueEmpty;
            if (parseResult.Sections.Count == 3)
            {
                programState.Headers.Remove(parseResult.Sections[2]);
                isValueEmpty = true;
            }
            else
            {
                programState.Headers[parseResult.Sections[2]] = parseResult.Sections.Skip(3).ToList();
                isValueEmpty = false;
            }

            _telemetry.TrackEvent(new SetHeaderEvent(parseResult.Sections[2], isValueEmpty));

            return Task.CompletedTask;
        }

        public string GetHelpDetails(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            if (parseResult.ContainsAtLeast(CommandName, SubCommand))
            {
                StringBuilder helpText = new StringBuilder();
                helpText.Append(Strings.Usage.Bold());
                helpText.AppendLine("set header {name} [value]");
                helpText.AppendLine();
                helpText.AppendLine(Strings.SetHeaderCommand_HelpDetails);
                return helpText.ToString();
            }

            return null;
        }

        public string GetHelpSummary(IShellState shellState, HttpState programState)
        {
            return Description;
        }

        public IEnumerable<string> Suggest(IShellState shellState, HttpState programState, ICoreParseResult parseResult)
        {
            parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

            if (parseResult.Sections.Count == 0)
            {
                return new[] { CommandName };
            }

            if (parseResult.Sections.Count > 0 && parseResult.SelectedSection == 0 && CommandName.StartsWith(parseResult.Sections[0].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase))
            {
                return new[] { CommandName };
            }

            if (string.Equals(CommandName, parseResult.Sections[0], StringComparison.OrdinalIgnoreCase) && parseResult.SelectedSection == 1 && (parseResult.Sections.Count < 2 || SubCommand.StartsWith(parseResult.Sections[1].Substring(0, parseResult.CaretPositionWithinSelectedSection), StringComparison.OrdinalIgnoreCase)))
            {
                return new[] { SubCommand };
            }

            if (parseResult.Sections.Count > 2
                && string.Equals(CommandName, parseResult.Sections[0], StringComparison.OrdinalIgnoreCase)
                && string.Equals(SubCommand, parseResult.Sections[1], StringComparison.OrdinalIgnoreCase) && parseResult.SelectedSection == 2)
            {
                string prefix = parseResult.Sections[2].Substring(0, parseResult.CaretPositionWithinSelectedSection);
                return HeaderCompletion.GetCompletions(null, prefix);
            }

            if (parseResult.Sections.Count > 3
                && string.Equals(CommandName, parseResult.Sections[0], StringComparison.OrdinalIgnoreCase)
                && string.Equals(SubCommand, parseResult.Sections[1], StringComparison.OrdinalIgnoreCase) && parseResult.SelectedSection == 3)
            {
                string prefix = parseResult.Sections[3].Substring(0, parseResult.CaretPositionWithinSelectedSection);
                return HeaderCompletion.GetValueCompletions(null, string.Empty, parseResult.Sections[2], prefix, programState);
            }

            return null;
        }
    }
}
