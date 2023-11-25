using AvaloniaFirstApp.Models;
using ReactiveUI;
using System;

namespace AvaloniaFirstApp.ViewModels;

public class SearchObjectViewModel : ReactiveUI.ReactiveObject
{
    private bool _circle;

    public bool Circle
    {
        get => _circle;
        set
        {
            this.RaiseAndSetIfChanged(ref _circle, value);
            if(value)
                SetSearchType?.Invoke(SearchObjectType.Circle);
        }
    }

    private bool _line;

    public bool Line
    {
        get => _line;
        set
        {
            this.RaiseAndSetIfChanged(ref _line, value);
            if(value)
                SetSearchType?.Invoke(SearchObjectType.Line);
        }
    }

    private bool _nonDirectLine;

    public bool NonDirectLine
    {
        get => _nonDirectLine;
        set
        {
            this.RaiseAndSetIfChanged(ref _nonDirectLine, value);
            if(value)
                SetSearchType?.Invoke(SearchObjectType.NotDirectLine);
        }
    }

    public event Action<SearchObjectType> SetSearchType;
}
