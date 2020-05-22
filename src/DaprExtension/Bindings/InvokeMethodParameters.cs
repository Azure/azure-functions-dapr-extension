// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Parameters for Dapr invoke-method operations.
    /// </summary>
    public class InvokeMethodParameters
    {
        /// <summary>
        /// Gets or sets the ID of the app containing the method to invoke.
        /// </summary>
        public string? AppId { get; set; }

        /// <summary>
        /// Gets or sets the name of the method to invoke.
        /// </summary>
        public string? MethodName { get; set; }

        /// <summary>
        /// Gets or sets the HTTP verb associated with the method to invoke.
        /// </summary>
        public string HttpVerb { get; set; } = "GET";

        /// <summary>
        /// Gets or sets the body of the invoke-method operation.
        /// </summary>
        public JToken? Body { get; set; }
    }
}