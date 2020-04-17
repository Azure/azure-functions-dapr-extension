// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The parameters for a Dapr save actor state operation.
    /// </summary>
    public class DaprActorStateRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprActorStateRecord"/> class.
        /// </summary>
        /// <param name="value">The value of the state record.</param>
        public DaprActorStateRecord(JToken? value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprActorStateRecord"/> class.
        /// <param name="key">The key of the state record.</param>
        /// <param name="contentStream">The contentStream of the state record.</param>
        public DaprActorStateRecord(string key, Stream contentStream)
        {
            this.Key = key;
            this.ContentStream = contentStream;
        }

        /// <summary>
        /// Gets the value of the actor state record.
        /// </summary>
        [JsonProperty("key")]
        public string? Key { get; internal set; }

        /// <summary>
        /// Gets the value of the actor state record.
        /// </summary>
        [JsonProperty("value")]
        public JToken? Value { get; internal set; }

        /// <summary>
        ///  Gets or sets the content stream of the actor state record. 
        ///  Populates when reading actor state record from the dapr state store.
        /// </summary>
        internal Stream ContentStream { get; set; } = Stream.Null;
    }
}