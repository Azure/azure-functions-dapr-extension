// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    class SaveStateParameters
    {
        public SaveStateParameters(JToken value)
        {
            this.Value = value;
        }

        // If not specified, defaults to binding values
        public string? StateStore { get; set;  }

        // If not specified, defaults to binding values
        public string? Key { get; set;  }

        public JToken? Value { get; }
    }
}