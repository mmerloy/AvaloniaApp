﻿using AvaloniaFirstApp.Models;
using Domain.Defects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    public async Task<IEnumerable<DefectModel>> GetDefectsFromImageAsync(string imagePath, CancellationToken cToken = default)
    {
        await CheckFirstReadAsync(cToken);

        await _pythonScriptInput.WriteLineAsync(imagePath);

        var (outputImagePath, defectsInformationXmlFilePath) = await GetInfoFromOutputAsync(cToken);

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

    private DefectType GetDefectTypeFromName(string defectName) { return DefectType.None; }

    public async Task<string> GetImageWithDefectsAsync(string inputImagePath, CancellationToken cToken = default)
    {
        await CheckFirstReadAsync(cToken);

        await _pythonScriptInput.WriteLineAsync(inputImagePath);

        var (outputImagePath, _) = await GetInfoFromOutputAsync(cToken);

        return outputImagePath;
    }

    private Task SkipOutputLineAsync(CancellationToken cToken = default)
        => _pythonScriptOutput.ReadLineAsync(cToken).AsTask();

    private async Task<(string ProccessedImagePath, string DefectsInformationPath)> GetInfoFromOutputAsync(CancellationToken cToken = default)
    {
        string? outputImagePath = await _pythonScriptOutput.ReadLineAsync(cToken);
        if (outputImagePath is null)
            throw new InvalidOperationException("Cannot read output image path from python script " + _pythonScriptProcess.StartInfo.Arguments);
        string? defectsInformationPath = await _pythonScriptOutput.ReadLineAsync(cToken);
        if (defectsInformationPath is null)
            throw new InvalidOperationException("Cannot read output defects path from python script " + _pythonScriptProcess.StartInfo.Arguments);

        return (outputImagePath, defectsInformationPath);
    }

    private async Task CheckFirstReadAsync(CancellationToken cToken = default)
    {
        if (_firstRead)
        {
            await SkipOutputLineAsync(cToken);
            await SkipOutputLineAsync(cToken);
            _firstRead = false;
        }
    }

    public void Dispose()
    {
        _pythonScriptOutput.Dispose();
        _pythonScriptInput.Dispose();
        _pythonScriptProcess.Dispose();
    }
}