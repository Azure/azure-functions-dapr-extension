// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    class SaveStateParameters
    {
        public string StateStore { get; set; }

        public string Key { get; set; }

        public JToken Value { get; set; }
    }
}