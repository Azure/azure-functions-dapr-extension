// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Worker.Extensions.Dapr
{
    using System;

    /// <summary>
    /// Parameter attribute for the Dapr secret input binding.
    /// </summary>
    public class DaprSecretInputAttribute : DaprBaseInputAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprSecretInputAttribute"/> class.
        /// </summary>
        /// <param name="secretStoreName">The name of the secret store to get the secret from.</param>
        /// <param name="key">The key identifying the name of the secret to get.</param>
        public DaprSecretInputAttribute(string secretStoreName, string key)
        {
            this.SecretStoreName = secretStoreName ?? throw new ArgumentNullException(nameof(secretStoreName));
            this.Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Gets the name of the secret store to get the secret from.
        /// </summary>
        public string? SecretStoreName { get; private set; }

        /// <summary>
        /// Gets the key identifying the name of the secret to get.
        /// </summary>
        public string? Key { get; private set; }

        /// <summary>
        /// Gets or sets an array of metadata properties in the form "key1=value1&amp;key2=value2".
        /// </summary>
        public string? Metadata { get; set; }
    }
}