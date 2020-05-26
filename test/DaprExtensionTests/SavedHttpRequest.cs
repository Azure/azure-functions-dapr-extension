// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests
{
    using Microsoft.AspNetCore.Http;

    class SavedHttpRequest
    {
        internal SavedHttpRequest(HttpRequest request, string content)
        {
            this.Method = request.Method;
            this.Path = ForceEncode(request.Path.ToUriComponent());
            this.Query = request.QueryString;
            this.ContentType = request.ContentType;
            this.ContentAsString = content;
        }

        public string Method { get; }

        public string Path { get; }

        public QueryString Query { get; }

        public string ContentType { get; }

        public string ContentAsString { get; }

        /// <summary>
        /// Work-around for unit test support to avoid ToString() from de-encoding special characters
        /// ex: encoded uri path: "/1.0/state/http%3A%2F%2Fdapr.io%2Fkeys%2FMyStore"
        /// => ToString() de-encodes "%3A" to ":": /v1.0/state/http:%2F%2Fdapr.io%2Fkeys%2FMyStore
        /// https://stackoverflow.com/questions/52106567/how-to-stop-httprequestmessage-from-unencoding-3a-to-a-colon-in-the-request-uri
        /// https://stackoverflow.com/questions/43778900/system-uri-and-encoded-colon
        /// </summary>
        static string ForceEncode(string input)
        {
            if (input.Contains(':'))
            {
                // %3A
                input = input.Replace(":", "%3A");
            }

            if (input.Contains(' '))
            {
                // %20
                input = input.Replace(" ", "%20");
            }

            if (input.Contains('*'))
            {
                // %2A
                input = input.Replace("*", "%2A");
            }

            return input;
        }
    }
}
