// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.HttpRepl.Telemetry.Events
{
    internal class SetHeaderEvent : TelemetryEventBase
    {
        public SetHeaderEvent(string headerName, bool isValueEmpty) : base("SetHeader")
        {
            SetProperty("HeaderName", SanitizeHeaderName(headerName));
            SetProperty("IsValueEmpty", isValueEmpty);
        }

        private static string SanitizeHeaderName(string headerName)
        {
            if (WellKnownHeaders.CommonHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase))
            {
                return headerName;
            }

            return Sha256Hasher.Hash(headerName);
        }
    }
}
