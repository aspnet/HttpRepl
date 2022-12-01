// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HttpRepl.Telemetry
{
    internal interface IDockerContainerDetector
    {
        IsDockerContainer IsDockerContainer();
    }
}
