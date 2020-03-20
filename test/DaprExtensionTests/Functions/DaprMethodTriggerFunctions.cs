// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Dapr;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DaprExtensionTests.Functions
{
    static class DaprMethodTriggerFunctions
    {
        [FunctionName(nameof(Add))]
        public static string Add([DaprMethodTrigger] JObject args, ILogger log)
        {
            log.LogInformation("C# processed a request from the Dapr runtime");

            double result = (double)args["arg1"] + (double)args["arg2"];
            return result.ToString();
        }

        [FunctionName(nameof(EchoStream))]
        public static Stream EchoStream([DaprMethodTrigger] Stream input)
        {
            return input;
        }

        [FunctionName(nameof(EchoBytes))]
        public static byte[] EchoBytes([DaprMethodTrigger] byte[] input)
        {
            return input;
        }

        [FunctionName(nameof(EchoPOCO))]
        public static CustomType EchoPOCO([DaprMethodTrigger] CustomType input)
        {
            return input;
        }

        [FunctionName(nameof(GetState1))]
        public static string GetState1(
            [DaprMethodTrigger] string stateKey,
            [DaprState(Key = "{stateKey}")] string existingState)
        {
            return existingState;
        }

        [FunctionName(nameof(GetState2))]
        public static string GetState2(
            [DaprMethodTrigger] JObject input,
            [DaprState(Key = "{input.stateKey}")] string existingState)
        {
            // TODO: Not sure yet if this binding expression will work - needs testing.
            return existingState;
        }

        public class CustomType
        {
            public string P1 { get; set; }
            public int P2 { get; set; }
            public DateTime P3 { get; set; }
        }
    }
}
