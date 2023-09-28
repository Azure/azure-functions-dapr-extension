// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    /// <summary>
    /// Dictionary Utils.
    /// </summary>
    public static class DictionaryUtils
    {
        /// <summary>
        /// Convert a JsonElement to a dictionary.
        /// </summary>
        /// <param name="element">JsonElement.</param>
        /// <returns>Dictionary.</returns>
        public static Dictionary<string, JsonElement> ToCaseInsensitiveDictionary(this JsonElement element)
        {
            var propertyBag = new Dictionary<string, JsonElement>(StringComparer.InvariantCultureIgnoreCase);

            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
            {
                return propertyLookup;
            }

            foreach (var prop in element.EnumerateObject())
            {
                if (element.TryGetProperty(prop.Name, out JsonElement value) && value.ValueKind != JsonValueKind.Null)
                {
                    propertyLookup[prop.Name] = value;
                }
            }

            return propertyLookup;
        }
    }
}