// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Utils
{
    /// <summary>
    /// Utility methods for logging.
    /// </summary>
    public static class LoggingUtils
    {
        /// <summary>
        /// Creates a category name for Dapr bindings to be used in logging.
        /// </summary>
        /// <returns>A category name for Dapr bindings.</returns>
        public static string CreateDaprBindingCategory()
        {
            return "Host.Bindings.Dapr";
        }

        /// <summary>
        /// Creates a category name for Dapr binding to be used in logging.
        /// </summary>
        /// <param name="bindingType">The type of the trigger.</param>
        /// <returns>A category name for Dapr binding.</returns>
        public static string CreateDaprBindingCategory(string bindingType)
        {
            return $"Host.Bindings.Dapr.{bindingType}";
        }

        /// <summary>
        /// Creates a category name for Dapr triggers to be used in logging.
        /// </summary>
        /// <returns>A category name for Dapr triggers.</returns>
        public static string CreateDaprTriggerCategory()
        {
            return $"Host.Triggers.Dapr";
        }

        /// <summary>
        /// Creates a category name for Dapr triggers to be used in logging.
        /// </summary>
        /// <param name="triggerType">The type of the trigger.</param>
        /// <returns>A category name for Dapr triggers.</returns>
        public static string CreateDaprTriggerCategory(string triggerType)
        {
            return $"Host.Triggers.Dapr.{triggerType}";
        }
    }
}