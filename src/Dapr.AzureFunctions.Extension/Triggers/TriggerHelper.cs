// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using System.Reflection;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    internal static class TriggerHelper
    {
        public static string ResolveTriggerName(ParameterInfo parameter, INameResolver nameResolver, string? triggerName)
        {
            if (triggerName == null)
            {
                MemberInfo method = parameter.Member;
                return method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? method.Name;
            }
            else if (nameResolver.TryResolveWholeString(triggerName, out string? resolvedTriggerName))
            {
                return resolvedTriggerName;
            }
            else
            {
                return triggerName;
            }
        }
    }
}
