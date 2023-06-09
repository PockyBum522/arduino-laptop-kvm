﻿using System;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Config.Net;
using JetBrains.Annotations;
using Serilog;
using WindowsKeyboardMouseServer.Core;
using WindowsKeyboardMouseServer.Core.Logic.Application;
using WindowsKeyboardMouseServer.Core.Models;
using WindowsKeyboardMouseServer.UI.WindowResources.MainWindowResources;
using WindowsKeyboardMouseServer.UI.WindowResources.SettingsWindowResources;

namespace WindowsKeyboardMouseServer.Main;

/// <summary>
/// Builds the container for the local application dependencies. This is then passed to TeakTools.Common
/// dependency injection for library dependencies to get added
/// </summary>
[PublicAPI]
public class DiContainerBuilder
{
    private readonly ContainerBuilder _builder = new ();
    private ILogger? _logger;
    
    private ISettingsApplicationLocal _settingsApplicationLocal;

    /// <summary>
    /// Gets a built container with all local application and TeakTools.Common dependencies in it
    /// </summary>
    /// <returns>A built container with all local application and TeakTools.Common dependencies in it</returns>
    [SupportedOSPlatform("Windows7.0")]
    public IContainer GetBuiltContainer(ILogger? testingLogger = null)
    {
        // Takes testing logger or if that's null builds the normal one and adds to DI container
        RegisterLogger(testingLogger);

        // This is not injected, kind of. It adds the dictionary to Application.Current.Resources.MergedDictionaries
        AddThemeResourceMergedDictionary();
        
        // All of these methods set up and initialize all necessary resources and dependencies,
        // then register the thing for Dependency Injection
        
        RegisterApplicationConfiguration();
        
        RegisterMainDependencies();

        RegisterUiDependencies();

        var container = _builder.Build();
        
        return container;
    }
    
    [SupportedOSPlatform("Windows7.0")]
    private void AddThemeResourceMergedDictionary()
    {
        var resourcePath = ApplicationPaths.DarkThemePath;
        var currentResource = new Uri(resourcePath, UriKind.RelativeOrAbsolute);
        
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = currentResource });
    }

    [SupportedOSPlatform("Windows7.0")]
    private void RegisterLogger(ILogger? testingLogger)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ApplicationPaths.LogPath) ?? "");

        if (testingLogger is not null)
        {
            _builder.RegisterInstance(testingLogger).As<ILogger>().SingleInstance();
            return;
        }

        // Otherwise, if it is null, make new logger:
        _logger = new LoggerConfiguration()
            .Enrich.WithProperty("WindowsKeyboardMouseServerApplication", "WindowsKeyboardMouseServerSerilogContext")
            .MinimumLevel.Debug()
            .WriteTo.File(ApplicationPaths.LogPath, rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .WriteTo.Debug()
            .CreateLogger();
        
        _builder.RegisterInstance(_logger).As<ILogger>().SingleInstance();
    }
    private void RegisterApplicationConfiguration()
    {
        _settingsApplicationLocal = 
            new ConfigurationBuilder<ISettingsApplicationLocal>()
                .UseIniFile(ApplicationPaths.PathSettingsApplicationLocalIniFile)
                .Build();
        
        _builder.RegisterInstance(_settingsApplicationLocal).As<ISettingsApplicationLocal>().SingleInstance();
        
        SetupAnyBlankConfigurationPropertiesAsDefaults();
    }

    private void SetupAnyBlankConfigurationPropertiesAsDefaults()
    {
        if (string.IsNullOrWhiteSpace(_settingsApplicationLocal.LastSelectedComPort))
            _settingsApplicationLocal.LastSelectedComPort = "COM257";
    }

    [SupportedOSPlatform("Windows7.0")]
    private void RegisterMainDependencies()
    {
        _builder.RegisterType<ExceptionHandler>().AsSelf().SingleInstance();
    }
    
    [SupportedOSPlatform("Windows7.0")]
    private void RegisterUiDependencies()
    {
        _builder.RegisterInstance(Dispatcher.CurrentDispatcher).AsSelf().SingleInstance();
        
        _builder.RegisterType<MainWindowViewModel>().AsSelf().SingleInstance();
        _builder.RegisterType<MainWindow>().AsSelf().SingleInstance();
        
        _builder.RegisterType<SettingsWindowViewModel>().AsSelf();
        _builder.RegisterType<SettingsWindow>().AsSelf();
    }
}