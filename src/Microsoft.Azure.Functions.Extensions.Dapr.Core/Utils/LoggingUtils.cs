// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.Functions.Extensions.Dapr.Core.Utils
{
    /// <summary>
    /// Utility methods for logging.
    /// </summary>
    public static class LoggingUtils
    {
        /// <summary>
        /// Creates a category name for Dapr bindings to be used in logging.
        /// </summary>
        /// <param name="bindingScope">The scope of the binding.</param>
        /// <returns>A category name for Dapr bindings.</returns>
        public static string CreateDaprBindingCategory(BindingScope? bindingScope = null)
        {
            if (bindingScope == null)
            {
                return "Host.Bindings.Dapr";
            }
            else
            {
                return $"Host.Binding.Dapr.{bindingScope.Value.BindingType}.{bindingScope.Value.BindingName}";
            }
        }

        /// <summary>
        /// A struct representing the scope of a Dapr binding.
        /// </summary>
        public readonly struct BindingScope
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BindingScope"/> struct.
            /// </summary>
            /// <param name="bindingName">The name of the specific binding.</param>
            /// <param name="bindingType">The type of the specific binding.</param>
            public BindingScope(string bindingName, string bindingType)
            {
                this.BindingName = bindingName;
                this.BindingType = bindingType;
            }

            /// <summary>
            /// Gets the name of the specific binding.
            /// </summary>
            public string BindingName { get; }

            /// <summary>
            /// Gets the type of the specific binding.
            /// </summary>
            public string BindingType { get; }
        }
    }
}