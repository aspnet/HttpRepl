// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await new Program().Start(args, new ConsoleManager());
        }

        public async Task Start(string[] args, IConsoleManager console)
        { 
            IFileSystem fileSystem = new RealFileSystem();
            HttpState state = CreateHttpState(fileSystem);

            if (Console.IsOutputRedirected && !console.AllowOutputRedirection)
            {
                Reporter.Error.WriteLine(Resources.Strings.Error_OutputRedirected.SetColor(state.ErrorColor));
                return;
            }

            var dispatcher = DefaultCommandDispatcher.Create(state.GetPrompt, state);
            dispatcher.AddCommand(new ChangeDirectoryCommand());
            dispatcher.AddCommand(new ClearCommand());
            //dispatcher.AddCommand(new ConfigCommand());
            dispatcher.AddCommand(new DeleteCommand(fileSystem));
            dispatcher.AddCommand(new EchoCommand());
            dispatcher.AddCommand(new ExitCommand());
            dispatcher.AddCommand(new HeadCommand(fileSystem));
            dispatcher.AddCommand(new HelpCommand());
            dispatcher.AddCommand(new GetCommand(fileSystem));
            dispatcher.AddCommand(new ListCommand());
            dispatcher.AddCommand(new OptionsCommand(fileSystem));
            dispatcher.AddCommand(new PatchCommand(fileSystem));
            dispatcher.AddCommand(new PrefCommand());
            dispatcher.AddCommand(new PostCommand(fileSystem));
            dispatcher.AddCommand(new PutCommand(fileSystem));
            dispatcher.AddCommand(new RunCommand(fileSystem));
            dispatcher.AddCommand(new SetBaseCommand());
            dispatcher.AddCommand(new SetDiagCommand());
            dispatcher.AddCommand(new SetHeaderCommand());
            dispatcher.AddCommand(new SetSwaggerCommand());
            dispatcher.AddCommand(new UICommand());

            CancellationTokenSource source = new CancellationTokenSource();
            Shell shell = new Shell(dispatcher, consoleManager: console);
            shell.ShellState.ConsoleManager.AddBreakHandler(() => source.Cancel());
            if (args.Length > 0)
            {
                if (string.Equals(args[0], "--help", StringComparison.OrdinalIgnoreCase) || string.Equals(args[0], "-h", StringComparison.OrdinalIgnoreCase))
                {
                    shell.ShellState.ConsoleManager.WriteLine(Resources.Strings.Help_Usage);
                    shell.ShellState.ConsoleManager.WriteLine("  dotnet httprepl [<BASE_ADDRESS>] [options]");
                    shell.ShellState.ConsoleManager.WriteLine();
                    shell.ShellState.ConsoleManager.WriteLine(Resources.Strings.Help_Arguments);
                    shell.ShellState.ConsoleManager.WriteLine(string.Format(Resources.Strings.Help_BaseAddress, "<BASE_ADDRESS>"));
                    shell.ShellState.ConsoleManager.WriteLine();
                    shell.ShellState.ConsoleManager.WriteLine(Resources.Strings.Help_Options);
                    shell.ShellState.ConsoleManager.WriteLine(string.Format(Resources.Strings.Help_Help, "-h|--help"));

                    shell.ShellState.ConsoleManager.WriteLine();
                    shell.ShellState.ConsoleManager.WriteLine(Resources.Strings.Help_REPLCommands);
                    new HelpCommand().CoreGetHelp(shell.ShellState, (ICommandDispatcher<HttpState, ICoreParseResult>)shell.ShellState.CommandDispatcher, state);
                    return;
                }

                // allow running a script file directly.
                if (string.Equals(args[0], "run"))
                {
                    shell.ShellState.CommandDispatcher.OnReady(shell.ShellState);
                    shell.ShellState.InputManager.SetInput(shell.ShellState, string.Join(' ', args));
                    await shell.ShellState.CommandDispatcher.ExecuteCommandAsync(shell.ShellState, CancellationToken.None).ConfigureAwait(false);
                    return;
                }

                shell.ShellState.CommandDispatcher.OnReady(shell.ShellState);
                shell.ShellState.InputManager.SetInput(shell.ShellState, $"set base \"{args[0]}\"");
                await shell.ShellState.CommandDispatcher.ExecuteCommandAsync(shell.ShellState, CancellationToken.None).ConfigureAwait(false);
            }
            Task result = shell.RunAsync(source.Token);
            await result.ConfigureAwait(false);
        }

        private static HttpState CreateHttpState(IFileSystem fileSystem)
        {
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            IPreferences preferences = new Preferences.UserFolderPreferences(fileSystem, userProfileDirectoryProvider);
            HttpState state = new HttpState(fileSystem, preferences);

            return state;
        }
    }
}
