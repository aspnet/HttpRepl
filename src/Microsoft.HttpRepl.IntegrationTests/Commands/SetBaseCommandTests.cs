// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class SetBaseCommandTests : BaseIntegrationTest, IClassFixture<DualHttpCommandsFixture<SampleApiServerConfig>>
    {
        private readonly SampleApiServerConfig _swaggerServerConfig;
        private readonly SampleApiServerConfig _nonSwaggerServerConfig;

        public SetBaseCommandTests(DualHttpCommandsFixture<SampleApiServerConfig> fixture)
        {
            _swaggerServerConfig = fixture.SwaggerConfig;
            _nonSwaggerServerConfig = fixture.NonSwaggerConfig;
        }

        [Fact]
        public async Task WithSwaggerAndValidUri_ShowsCorrectOutput()
        {
            string scriptText = $@"set base {_swaggerServerConfig.BaseAddress}";

            string output = await RunTestScript(scriptText, _swaggerServerConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]
Using swagger metadata from [BaseUrl]/swagger/v1/swagger.json

[BaseUrl]/~", null);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async Task WithoutSwaggerAndValidUri_ShowsCorrectOutput()
        {
            string scriptText = $@"set base {_nonSwaggerServerConfig.BaseAddress}";

            string output = await RunTestScript(scriptText, _nonSwaggerServerConfig.BaseAddress);

            string expected = NormalizeOutput(@"(Disconnected)~ set base [BaseUrl]

[BaseUrl]/~", null);

            Assert.Equal(expected, output);
        }
    }
}