using AvaloniaFirstApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaFirstApp.Infrastructure.Services.Prediction;

public sealed class PythonScriptPredictionService : IPredictionService, IDisposable
{
    private readonly Process _pythonScriptProcess;
    private readonly StreamWriter _pythonScriptInput;
    private readonly StreamReader _pythonScriptOutput;
    private bool _firstRead = true;

    public PythonScriptPredictionService(string pythonScriptPath, string inputModelPath, string outputImagesDirectoryPath)
    {
        _pythonScriptProcess = Process.Start(new ProcessStartInfo()
        {
            FileName = "python.exe",
            Arguments = pythonScriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
        }) ?? throw new ArgumentException("Cannot start python script process with argument " + pythonScriptPath);

        _pythonScriptInput = _pythonScriptProcess.StandardInput;
        _pythonScriptOutput = _pythonScriptProcess.StandardOutput;

        //Установка конфига прям из скрипта
        _pythonScriptInput.WriteLine(inputModelPath);
        _pythonScriptInput.WriteLine(outputImagesDirectoryPath);

        Thread.Sleep(TimeSpan.FromSeconds(3));
    }

    public Task<IEnumerable<DefectModel>> GetDefectsFromImageAsync(string imagePath, CancellationToken cToken = default)
        => throw new NotImplementedException();

    public async Task<string> GetImageWithDefectsAsync(string inputImagePath, CancellationToken cToken = default)
    {
        if (_firstRead)
        {
            string s = await _pythonScriptOutput.ReadLineAsync(cToken);
            s = await _pythonScriptOutput.ReadLineAsync(cToken);
            _firstRead = false;
        }

        _pythonScriptInput.WriteLine(inputImagePath);
        //_pythonScriptProcess.BeginOutputReadLine();

        string? outputImagePath = await _pythonScriptOutput.ReadLineAsync(cToken);
        if (outputImagePath is null)
            throw new InvalidOperationException("Cannot read output path from python script " + _pythonScriptProcess.StartInfo.Arguments);

        //_pythonScriptProcess.CancelOutputRead();
        return outputImagePath;
    }

    public void Dispose()
    {
        _pythonScriptOutput.Dispose();
        _pythonScriptInput.Dispose();
        _pythonScriptProcess.Dispose();
    }
}
