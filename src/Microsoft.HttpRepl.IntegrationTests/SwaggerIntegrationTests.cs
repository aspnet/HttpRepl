// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.IntegrationTests.Commands;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Microsoft.HttpRepl.IntegrationTests.Utilities;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests
{
    public class SwaggerIntegrationTests : BaseIntegrationTest, IClassFixture<HttpCommandsFixture<SampleApiServerConfig>>
    {
        private readonly SampleApiServerConfig _serverConfig;

        public SwaggerIntegrationTests(HttpCommandsFixture<SampleApiServerConfig> fixture)
        {
            _serverConfig = fixture.Config;
        }
        
        [Fact]
        public async Task ListCommand_WithSwagger_ShowsAvailableSubpaths()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
ls
cd api
ls";
            var console = new LoggingConsoleManagerDecorator(new NullConsoleManager());
            using (var script = new TestScript(scriptText))
            {
                await new Program().Start($"run {script.FilePath}".Split(' '), console);
            }

            string output = console.LoggedOutput;
            // remove the first line because it has the randomly generated script file name.
            output = output.Substring(output.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
            output = NormalizeOutput(output, _serverConfig.BaseAddress);

            // make sure to normalize newlines in the expected output
            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ ls
.     []
api   []

[BaseUrl]/~ cd api
/api    []

[BaseUrl]/api~ ls
.        []
..       []
Values   [get|post]

[BaseUrl]/api~", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task ListCommand_WithSwagger_ShowsControllerActionsWithHttpVerbs()
        {
            string scriptText = $@"set base {_serverConfig.BaseAddress}
cd api/Values
ls";
            var console = new LoggingConsoleManagerDecorator(new NullConsoleManager());
            using (var scriptFile = new TestScript(scriptText))
            {
                await new Program().Start($"run {scriptFile.FilePath}".Split(' '), console);
            }
            string output = console.LoggedOutput;
            // remove the first line because it has the randomly generated script file name.
            output = output.Substring(output.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
            output = NormalizeOutput(output, _serverConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~ cd api/Values
/api/Values    [get|post]

[BaseUrl]/api/Values~ ls
.      [get|post]
..     []
{id}   [get|put|delete]

[BaseUrl]/api/Values~", null);

            Assert.Equal(expected, output);
        }
    }
}
