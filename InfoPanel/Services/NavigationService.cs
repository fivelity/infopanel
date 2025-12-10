using System;
using Microsoft.UI.Xaml.Controls;

namespace InfoPanel.Services;

/// <summary>
/// Interface for navigation service in WinUI 3
/// </summary>
public interface INavigationService
{
    Frame? Frame { get; set; }
    bool CanGoBack { get; }
    bool Navigate(Type pageType, object? parameter = null);
    bool GoBack();
}

/// <summary>
/// Navigation service for WinUI 3 applications
/// </summary>
public class NavigationService : INavigationService
{
    private Frame? _frame;

    public Frame? Frame
    {
        get => _frame;
        set => _frame = value;
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public bool Navigate(Type pageType, object? parameter = null)
    {
        if (_frame == null)
            return false;

        return _frame.Navigate(pageType, parameter);
    }

    public bool GoBack()
    {
        if (_frame == null || !_frame.CanGoBack)
            return false;

        _frame.GoBack();
        return true;
    }
}
