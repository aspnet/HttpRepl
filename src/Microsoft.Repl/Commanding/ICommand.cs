// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Repl.Parsing;

namespace Microsoft.Repl.Commanding
{
    public interface ICommand<in TProgramState, in TParseResult>
        where TParseResult : ICoreParseResult
    {
        /// <summary>
        /// Identifies the command in telemetry events.
        /// </summary>
        string Name { get; }

        string GetHelpSummary(IShellState shellState, TProgramState programState);

        string GetHelpDetails(IShellState shellState, TProgramState programState, TParseResult parseResult);

        IEnumerable<string> Suggest(IShellState shellState, TProgramState programState, TParseResult parseResult);

        bool? CanHandle(IShellState shellState, TProgramState programState, TParseResult parseResult);

        Task ExecuteAsync(IShellState shellState, TProgramState programState, TParseResult parseResult, CancellationToken cancellationToken);
    }
}
