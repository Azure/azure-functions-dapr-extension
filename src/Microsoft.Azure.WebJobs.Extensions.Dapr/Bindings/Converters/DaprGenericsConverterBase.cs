// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Bindings.Converters
{
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;

    /// <summary>
    /// A base class for generic converters.
    /// </summary>
    /// <typeparam name="T1">Type of the input data.</typeparam>
    /// <typeparam name="T2">Type of the output data after conversion.</typeparam>
    internal abstract class DaprGenericsConverterBase<T1, T2> : IAsyncConverter<T1, T2>
    {
        readonly DaprServiceClient daprClient;

        public DaprGenericsConverterBase(DaprServiceClient daprClient)
        {
            this.daprClient = daprClient;
        }

        /// <summary>
        /// Gets the string representation of the input data of type T1.
        /// </summary>
        /// <param name="input">The input data to be serialized.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public abstract Task<string> GetStringContentAsync(T1 input, CancellationToken cancellationToken);

        /// <summary>
        /// Converts the input data of type T1 to the output data of type T2.
        /// </summary>
        /// <param name="input">The input data to be converted.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        async Task<T2> IAsyncConverter<T1, T2>.ConvertAsync(
             T1 input,
             CancellationToken cancellationToken)
        {
            string result = await this.GetStringContentAsync(input, cancellationToken);
            return JsonSerializer.Deserialize<T2>(result)
                ?? throw new InvalidDataException("Unable to deserialize the secret.");
        }
    }
}