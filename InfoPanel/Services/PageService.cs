// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.

#nullable enable

using System;
using Microsoft.UI.Xaml;

namespace InfoPanel.Services;

/// <summary>
/// Interface for page service in WinUI 3
/// </summary>
public interface IPageService
{
    T? GetPage<T>() where T : class;
    FrameworkElement? GetPage(Type pageType);
}

/// <summary>
/// Service that provides pages for navigation.
/// </summary>
public class PageService : IPageService
{
    /// <summary>
    /// Service which provides the instances of pages.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates new instance and attaches the <see cref="IServiceProvider"/>.
    /// </summary>
    public PageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public T? GetPage<T>() where T : class
    {
        if (!typeof(FrameworkElement).IsAssignableFrom(typeof(T)))
            throw new InvalidOperationException("The page should be a WinUI control.");

        return (T?)_serviceProvider.GetService(typeof(T));
    }

    /// <inheritdoc />
    public FrameworkElement? GetPage(Type pageType)
    {
        if (!typeof(FrameworkElement).IsAssignableFrom(pageType))
            throw new InvalidOperationException("The page should be a WinUI control.");

        return _serviceProvider.GetService(pageType) as FrameworkElement;
    }
}
