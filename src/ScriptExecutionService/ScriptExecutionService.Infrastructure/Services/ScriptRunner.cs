using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CaptureSys.ScriptExecutionService.Application.Interfaces;
using CaptureSys.ScriptExecutionService.Application.Services;
using CaptureSys.ScriptExecutionService.Domain.Entities;

namespace CaptureSys.ScriptExecutionService.Infrastructure.Services;

public class ScriptRunner : IScriptRunner
{
    private readonly ILogger<ScriptRunner> _logger;

    public ScriptRunner(ILogger<ScriptRunner> logger)
    {
        _logger = logger;
    }

    public async Task<ScriptResult> RunScriptAsync(string scriptPath, ScriptType scriptType, Dictionary<string, object> parameters)
    {
        var (executable, arguments) = GetExecutableAndArguments(scriptType, scriptPath);
        
        _logger.LogInformation("Exécution: {Executable} {Arguments}", executable, arguments);

        var processInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Ajouter les paramètres comme variables d'environnement
        foreach (var param in parameters)
        {
            processInfo.EnvironmentVariables[$"SCRIPT_PARAM_{param.Key.ToUpper()}"] = param.Value.ToString();
        }

        try
        {
            using var process = new Process { StartInfo = processInfo };
            
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            if (!string.IsNullOrEmpty(error))
            {
                output += $"\n--- ERREURS ---\n{error}";
            }

            return new ScriptResult
            {
                Output = output,
                ExitCode = process.ExitCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'exécution du script {ScriptPath}", scriptPath);
            return new ScriptResult
            {
                Output = $"Erreur d'exécution: {ex.Message}",
                ExitCode = -1
            };
        }
    }

    private static (string executable, string arguments) GetExecutableAndArguments(ScriptType scriptType, string scriptPath)
    {
        return scriptType switch
        {
            ScriptType.Python => ("python", $"\"{scriptPath}\""),
            ScriptType.PowerShell => ("powershell", $"-ExecutionPolicy Bypass -File \"{scriptPath}\""),
            ScriptType.Bash => ("bash", $"\"{scriptPath}\""),
            ScriptType.Batch => ("cmd", $"/c \"{scriptPath}\""),
            ScriptType.NodeJs => ("node", $"\"{scriptPath}\""),
            ScriptType.Custom => ("cmd", $"/c \"{scriptPath}\""),
            _ => throw new NotSupportedException($"Type de script {scriptType} non supporté")
        };
    }
}
