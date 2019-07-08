// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class UICommandTests : CommandHelper<UICommand>
    {
        public UICommandTests()
            : base(new UICommand(new UriLauncher()))
        {
        }

        [Fact]
        public void CanHandle_WithNoParseResultSections_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: string.Empty);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithMoreThanOneParseResultSections_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "ui test");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithInvalidName_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "test");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidName_ReturnsTrue()
        {
            bool? result = CanHandle(parseResultSections: "ui");

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            string result = GetHelpSummary();

            Assert.Equal(Strings.UICommand_Description, result);
        }

        [Fact]
        public void GetHelpDetails_WithInvalidParseResultSection_ReturnsNull()
        {
            string result = GetHelpDetails(parseResultSections: "section1");

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithMoreThanOneParseResultSections_ReturnsNull()
        {
            string result = GetHelpDetails(parseResultSections: "ui section2");

            Assert.Null(result);
        }

        [Fact]
        public async Task ExecuteAsync_WithHttpStateBaseAddressSetToNull_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            string parseResultSections = "ui";

            await ExecuteAsyncWithInvalidParseResultSections(parseResultSections, shellState);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidHttpStateBaseAddress_DoesNotThrowException()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("ui");
            HttpClient httpClient = new HttpClient();
            HttpState httpState = new HttpState(httpClient);
            Uri uri = new Uri("https://localhost:44366/");
            httpState.BaseAddress = uri;

            MockUriLauncher mockUriLauncher = new MockUriLauncher(true);
            UICommand uiCommand = new UICommand(mockUriLauncher);

            var exception = await Record.ExceptionAsync(async () => await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None));

            Assert.Null(exception);
        }

        [Fact]
        public async Task ExecuteAsync_WithLaunchUriFailure_ThrowsException()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("ui");
            HttpClient httpClient = new HttpClient();
            HttpState httpState = new HttpState(httpClient);
            Uri uri = new Uri("https://localhost:44366/");
            httpState.BaseAddress = uri;

            MockUriLauncher mockUriLauncher  = new MockUriLauncher(false);
            UICommand uiCommand = new UICommand(mockUriLauncher);

            var exception = await Record.ExceptionAsync(async () => await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None));
            string expectedErrorMessage = "Unable to launch https://localhost:44366/swagger";

            Assert.NotNull(exception);
            Assert.Equal(expectedErrorMessage, exception.Message);
        }
    }
}
