// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace dotnet_isolated_azurefunction
{
    using System.Text.Json.Serialization;

    public class Product
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }
        [JsonPropertyName("Description")]
        public string? Description { get; set; }
        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }
    }
}