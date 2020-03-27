// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DaprExtensionTests.Logging
{
    using System;
    using Microsoft.Extensions.Logging;

    class LogEntry
    {
        public LogEntry(LogLevel level, string message)
        {
            this.LogLevel = level;
            this.Message = message;
            this.Timestamp = DateTime.Now;
        }

        public DateTime Timestamp { get; }

        public LogLevel LogLevel { get; }

        public string Message { get; }

        public override string ToString()
        {
            return $"{this.Timestamp:o}: {this.Message}";
        }
    }
}
