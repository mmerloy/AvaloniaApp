using Avalonia;
using Avalonia.Media.Imaging;
using AvaloniaFirstApp.Models;
using Domain.Defects;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaFirstApp.Infrastructure.Services.Prediction;

public class PredictionServiceStub : IPredictionService
{
    private readonly Random _rg;

    public PredictionServiceStub(int seed)
    {
        _rg = new Random(seed);
    }

    public Task<IEnumerable<DefectModel>> GetDefectsFromImageAsync(string imagePath, CancellationToken cToken = default)
    {
        Bitmap bitmap = new(imagePath);
        Size bitmapSize = bitmap.Size;

        List<DefectType> existedDefects = Enum.GetValues(typeof(DefectType)).Cast<DefectType>().ToList();

        return Task.FromResult(Enumerable.Range(0, 10)
            .Select(i =>
            {
                int y = _rg.Next(0, (int)bitmapSize.Height);
                int x = _rg.Next(0, (int)bitmapSize.Width);

                return new DefectModel
                {
                    Type = existedDefects[_rg.Next(0, (int)existedDefects.Max() + 1)],
                    Location = new RectangleInfo
                    {
                        Width = _rg.Next(x, (int)(bitmapSize.Width * 9 / 10)),
                        Height = _rg.Next(y, (int)(bitmapSize.Height * 9 / 10)),
                        StartPoint = new RectanglePoint()
                        {
                            X = x,
                            Y = y
                        }
                    }
                };
            })
        );
    }
}
