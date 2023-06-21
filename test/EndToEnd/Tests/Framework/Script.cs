namespace EndToEndTests.Framework
{
    using System.Diagnostics;

    public class Script
    {
        /// <summary>
        /// Represents the result of invoking a bash script.
        /// </summary>
        public class InvokeScriptResult
        {
            public int ExitCode { get; set; }
            public string? StdOut { get; set; }
            public string? StdErr { get; set; }
        }

        /// <summary>
        /// Invokes a bash script on the local machine and returns InvokeScriptResult.
        /// </summary>
        public static InvokeScriptResult InvokeScript(string scriptName, string[] args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = scriptName,
                    Arguments = string.Join(" ", args),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            try
            {
                process.Start();
                process.WaitForExit();

                return new InvokeScriptResult
                {
                    ExitCode = process.ExitCode,
                    StdOut = process.StandardOutput.ReadToEnd(),
                    StdErr = process.StandardError.ReadToEnd(),
                };
            }
            catch (System.Exception ex)
            {
                return new InvokeScriptResult
                {
                    ExitCode = -1,
                    StdErr = ex.ToString(),
                };
            }
            finally
            {
                process.Dispose();
            }
        }
    }
}