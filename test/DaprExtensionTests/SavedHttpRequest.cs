// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DaprExtensionTests
{
    using Microsoft.AspNetCore.Http;

    class SavedHttpRequest
    {
        internal SavedHttpRequest(HttpRequest request, string content)
        {
            this.Method = request.Method;
            this.Path = request.Path;
            this.Query = request.QueryString;
            this.ContentType = request.ContentType;
            this.ContentAsString = content;
        }

        public string Method { get; }

        public PathString Path { get; }

        public QueryString Query { get; }

        public string ContentType { get; }

        public string ContentAsString { get; }
    }
}
