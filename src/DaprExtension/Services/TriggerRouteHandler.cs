// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    internal class TriggerRouteHandler
    {
        readonly IDictionary<string, Func<HttpContext, Task>> routeHandlers = new Dictionary<string, Func<HttpContext, Task>>();

        public TriggerRouteHandler()
        {
        }

        public void AddRoute(HttpMethod method, string path, Func<HttpContext, Task> routeRequestHandler)
        {
            string normalizedRoute = NormalizeRouteKey(method, path);
            if (this.routeHandlers.ContainsKey(normalizedRoute))
            {
                throw new InvalidOperationException($"A route already exists for {normalizedRoute}");
            }

            this.routeHandlers.Add(normalizedRoute, routeRequestHandler);
        }

        public void DeleteRoute(HttpMethod method, string path)
        {
            string normalizedRoute = NormalizeRouteKey(method, path);
            this.routeHandlers.Remove(normalizedRoute);
        }

        public Task RouteToTriggerAsync(HttpContext context)
        {
            string normalizedRoute = NormalizeRouteKey(context.Request);
            if (this.routeHandlers.TryGetValue(normalizedRoute, out Func<HttpContext, Task> triggerHandler))
            {
                return triggerHandler.Invoke(context);
            }
            else
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            }
        }

        private static string NormalizeRouteKey(HttpMethod method, string path)
        {
            return $"{method}:{path.ToLowerInvariant()}";
        }

        private static string NormalizeRouteKey(HttpRequest request)
        {
            return $"{request.Method}:{request.Path.ToString().Trim('/').ToLowerInvariant()}";
        }
    }
}
