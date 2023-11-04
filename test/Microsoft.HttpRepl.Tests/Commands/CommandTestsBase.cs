// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Moq;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class CommandTestsBase
    {
        protected static void ArrangeInputs(string parseResultSections,
            out MockedShellState shellState,
            out HttpState httpState,
            out ICoreParseResult parseResult,
            string baseAddress = "",
            int caretPosition = -1,
            string responseContent = "")
        {
            parseResult = CoreParseResultHelper.Create(parseResultSections, caretPosition);
            shellState = new MockedShellState();
            IDictionary<string, string> urlsWithResponse = new Dictionary<string, string>();
            urlsWithResponse.Add(baseAddress, responseContent);

            httpState = GetHttpState(out _, out _, urlsWithResponse: urlsWithResponse);
        }

        protected void ArrangeInputs(string commandText,
            string baseAddress,
            string path,
            IDictionary<string, string> urlsWithResponse,
            out MockedShellState shellState,
            out HttpState httpState,
            out ICoreParseResult parseResult,
            out MockedFileSystem fileSystem,
            out IPreferences preferences,
            string header = "",
            bool readBodyFromFile = false,
            string fileContents = "",
            string contentType = "")
        {
            parseResult = CoreParseResultHelper.Create(commandText);
            shellState = new MockedShellState();

            httpState = GetHttpState(out fileSystem,
                out preferences,
                baseAddress,
                header,
                path,
                urlsWithResponse,
                readBodyFromFile,
                fileContents,
                contentType);
        }

        protected void ArrangeInputsWithOptional(string commandText,
            string baseAddress,
            string path,
            IDictionary<string, string> urlsWithResponse,
            out MockedShellState shellState,
            out HttpState httpState,
            out ICoreParseResult parseResult,
            ref MockedFileSystem fileSystem,
            ref IPreferences preferences,
            string header = "",
            bool readBodyFromFile = false,
            string fileContents = "",
            string contentType = "")
        {
            parseResult = CoreParseResultHelper.Create(commandText);
            shellState = new MockedShellState();

            httpState = GetHttpStateWithOptional(ref fileSystem,
                ref preferences,
                baseAddress,
                header,
                path,
                urlsWithResponse,
                readBodyFromFile,
                fileContents,
                contentType);
        }

        protected static void VerifyErrorMessageWasWrittenToConsoleManagerError(IShellState shellState)
        {
            Mock<IWritable> error = Mock.Get(shellState.ConsoleManager.Error);

            error.Verify(s => s.WriteLine(It.IsAny<string>()), Times.Once);
        }

        protected static HttpState GetHttpStateWithOptional(ref MockedFileSystem fileSystem,
            ref IPreferences preferences,
            string baseAddress = "",
            string header = "",
            string path = "",
            IDictionary<string, string> urlsWithResponse = null,
            bool readFromFile = false,
            string fileContents = "",
            string contentType = "")
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            responseMessage.Content = new MockHttpContent(string.Empty);
            MockHttpMessageHandler messageHandler = new MockHttpMessageHandler(urlsWithResponse, header, readFromFile, fileContents, contentType);
            HttpClient httpClient = new HttpClient(messageHandler);
            fileSystem ??= new MockedFileSystem();
            preferences ??= new NullPreferences();

            HttpState httpState = new HttpState(preferences, httpClient);

            if (!string.IsNullOrWhiteSpace(baseAddress))
            {
                httpState.BaseAddress = new Uri(baseAddress);
            }
            if (!string.IsNullOrWhiteSpace(path))
            {
                httpState.BaseAddress = new Uri(baseAddress);

                string[] pathParts = path.Split('/');

                foreach (string pathPart in pathParts)
                {
                    httpState.PathSections.Push(pathPart);
                }
            }

            return httpState;
        }

        protected static HttpState GetHttpState(out MockedFileSystem fileSystem,
            out IPreferences preferences,
            string baseAddress = "",
            string header = "",
            string path = "",
            IDictionary<string, string> urlsWithResponse = null,
            bool readFromFile = false,
            string fileContents = "",
            string contentType = "")
        {
            fileSystem = new MockedFileSystem();
            preferences = new NullPreferences();

            return GetHttpStateWithOptional(ref fileSystem,
                ref preferences,
                baseAddress,
                header,
                path,
                urlsWithResponse,
                readFromFile,
                fileContents,
                contentType);
        }

        protected ICoreParseResult CreateCoreParseResult(string commandText)
        {
            return CoreParseResultHelper.Create(commandText);
        }
    }
}
