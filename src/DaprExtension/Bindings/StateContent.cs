// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    class StateContent
    {
        public StateContent(string? key, JToken? value)
        {
            this.Key = key ?? throw new ArgumentNullException(nameof(key));
            this.Value = value;
        }

        public string Key { get; }

        public JToken? Value { get; }
    }
}