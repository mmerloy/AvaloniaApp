using AvaloniaFirstApp.Models;
using Domain.Defects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Utils.Xml;

namespace AvaloniaFirstApp.Infrastructure.Services.Prediction;

public sealed class PythonScriptPredictionService : IPredictionService, IDisposable
{
    private readonly Process _pythonScriptProcess;
    private readonly StreamWriter _pythonScriptInput;
    private readonly StreamReader _pythonScriptOutput;

    private const string _imagePathPrompt = "Введите путь к входному изображению:";
    private const string _mLModelPathPrompt = "Введите путь модели нейронной сети:";
    private const string _outputDirPathPrompt = "Введите путь папки для сохранения размеченных изображений:";
    private const string _defectsNotFoundedPrompt = "Дефектов не выявлено";
    private const string _defectsExistPrompt = "Выявлены дефекты";
    private const string _pyExceptionCatch = "An exception occurred";
    //private readonly CancellationTokenSource _cancellationTokenSource = new();

    public PythonScriptPredictionService(string pythonScriptPath, string inputModelPath, string outputImagesDirectoryPath)
    {
        bool isExecutable = pythonScriptPath.EndsWith(".exe");
        string executableFile = isExecutable ? pythonScriptPath : "python.exe";
        string executingArgs = isExecutable ? string.Empty : pythonScriptPath;

        _pythonScriptProcess = Process.Start(new ProcessStartInfo()
        {
            FileName = executableFile,
            Arguments = executingArgs,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
        }) ?? throw new ArgumentException($"Cannot start python script process {executableFile} with args {executingArgs ?? "no args"}");

        //_pythonScriptProcess.OutputDataReceived += PyProcessOutputLineReceived;
        _pythonScriptInput = _pythonScriptProcess.StandardInput;
        _pythonScriptOutput = _pythonScriptProcess.StandardOutput;

        //Установка конфига прям из скрипта
        _pythonScriptInput.WriteLine(inputModelPath);
        _pythonScriptInput.WriteLine(outputImagesDirectoryPath);

        Thread.Sleep(TimeSpan.FromSeconds(3));
    }

    public async Task<IEnumerable<DefectModel>?> GetDefectsFromImageAsync(string imagePath, CancellationToken cToken = default)
    {
        await ReadToPrompt(_imagePathPrompt, cToken);

        await _pythonScriptInput.WriteLineAsync(imagePath);

        MLAnswer mLAnswer = await ReadToSuccessOrNotPrompt(cToken);

        if (mLAnswer == MLAnswer.DefectsNotFound)
            return Enumerable.Empty<DefectModel>();

        if (mLAnswer == MLAnswer.Error)
            return null;

        var (outputImagePath, defectsInformationXmlFilePath) = await GetDefectsInfoFromOutputAsync(cToken);

        using var fileStream = new FileStream(defectsInformationXmlFilePath, FileMode.Open);
        XmlDocument xmlDocument = new();
        xmlDocument.Load(fileStream);

        const string errxmlStructureMsg = "Wrong defects data file structure got.";

        if (xmlDocument.DocumentElement is null)
            throw new ArgumentException(errxmlStructureMsg);

        IEnumerator nodesIterator = xmlDocument.DocumentElement.GetChildNodes("object").GetEnumerator();

        List<DefectModel> defects = new();

        while (nodesIterator.MoveNext())
        {
            var node = nodesIterator.Current as XmlElement;

            if (node is null || node.Name != "name" || node.InnerText is null)
                throw new ArgumentException(errxmlStructureMsg + " Defect name node incorrect.");

            string defectName = node.InnerText;

            nodesIterator.MoveNext();
            node = nodesIterator.Current as XmlElement;

            if (node is null || node.Name != "bndbox")
                throw new ArgumentException(errxmlStructureMsg + " Defect box node incorrect.");

            const string errLocationNodesMsg = errxmlStructureMsg + " Defect box node incorrect location info.";

            var boxInfoNodesList = node.ChildNodes;
            if (boxInfoNodesList.Count != 4)
                throw new ArgumentException(errLocationNodesMsg);
            int xmin, xmax, ymin, ymax;
            try
            {
                xmin = int.Parse(boxInfoNodesList[0]?.InnerText!);
                xmax = int.Parse(boxInfoNodesList[1]?.InnerText!);
                ymin = int.Parse(boxInfoNodesList[2]?.InnerText!);
                ymax = int.Parse(boxInfoNodesList[3]?.InnerText!);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(errLocationNodesMsg, ex);
            }

            defects.Add(new DefectModel()
            {
                Type = GetDefectTypeFromName(defectName),
                Location = new RectangleInfo
                {
                    StartPoint = new RectanglePoint
                    {
                        X = xmin,
                        Y = ymin,
                    },
                    Width = Math.Abs(xmax - xmin),
                    Height = Math.Abs(ymax - ymin)
                }
            });
        }
        return defects;
    }

    private static DefectType GetDefectTypeFromName(string defectName)
    {
        if (!Enum.TryParse(defectName, out DefectType defect))
            throw new ArgumentException("Wrong defect name got.");
        return defect;
    }

    public async Task<string> GetImageWithDefectsAsync(string inputImagePath, CancellationToken cToken = default)
    {
        await ReadToPrompt(_imagePathPrompt, cToken);

        await _pythonScriptInput.WriteLineAsync(inputImagePath);

        var (outputImagePath, _) = await GetDefectsInfoFromOutputAsync(cToken);

        return outputImagePath;
    }

    private async Task<(string ProccessedImagePath, string DefectsInformationPath)> GetDefectsInfoFromOutputAsync(CancellationToken cToken = default)
    {
        string? outputImagePath = await _pythonScriptOutput.ReadLineAsync(cToken);
        if (outputImagePath is null)
            throw new InvalidOperationException("Cannot read output image path from python script " + _pythonScriptProcess.StartInfo.Arguments);
        string? defectsInformationPath = await _pythonScriptOutput.ReadLineAsync(cToken);
        if (defectsInformationPath is null)
            throw new InvalidOperationException("Cannot read output defects path from python script " + _pythonScriptProcess.StartInfo.Arguments);

        return (outputImagePath, defectsInformationPath);
    }

    private enum MLAnswer : byte
    {
        Success = 0,
        DefectsNotFound,
        Error
    }

    private async Task<MLAnswer> ReadToSuccessOrNotPrompt(CancellationToken cToken = default)
    {
        string? inputString;
        do
        {
            inputString = await _pythonScriptOutput.ReadLineAsync(cToken);
        } while (inputString != _defectsExistPrompt && inputString != _defectsNotFoundedPrompt && inputString != _pyExceptionCatch);

        return inputString switch
        {
            _defectsExistPrompt => MLAnswer.Success,
            _defectsNotFoundedPrompt => MLAnswer.DefectsNotFound,
            _ => MLAnswer.Error,
        };
    }

    private async Task ReadToPrompt(string prompt, CancellationToken cToken = default)
    {
        string? inputString;
        do
        {
            inputString = await _pythonScriptOutput.ReadLineAsync(cToken);

        } while (inputString != prompt);
    }

    public void Dispose()
    {
        //_pythonScriptOutput.Dispose();
        //_pythonScriptInput.Dispose();
        //_pythonScriptProcess.CloseMainWindow();
        _pythonScriptProcess.Close();
        _pythonScriptProcess.Dispose();
    }
}
