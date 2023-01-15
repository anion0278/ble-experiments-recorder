﻿using System.CodeDom;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Mebster.Myodam.DataAccess.FileStorage;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.UI.WPF.Views.Resouces;

namespace Mebster.Myodam.UI.WPF.ViewModels.Services;

public class AppConfiguration
{
    public bool IsCalibrationEnabled { get; init; }
    public ulong? MyodamAddress { get; init; }

    public DeviceCalibration MyodamCalibration { get; init; }
}

public interface IAppConfigurationLoader
{
    string ConfigurationFileName { get; }
    AppConfiguration Reload();
    AppConfiguration GetConfiguration();
    bool IsConfigurationAvailable();

    void SaveConfig();
}

public class AppConfigurationLoader : IAppConfigurationLoader
{
    private readonly IJsonManager _jsonManager;
    private readonly IDialogHelpers _dialogHelpers;
    private readonly IFileSystemManager _fileManager;
    private AppConfiguration _configuration = GetDefaultConfiguration();

    public string ConfigurationFileName => "config.json";

    public AppConfigurationLoader(IJsonManager jsonManager, IDialogHelpers dialogHelpers, IFileSystemManager fileManager)
    {
        _jsonManager = jsonManager;
        _dialogHelpers = dialogHelpers;
        _fileManager = fileManager;
        Reload();
    }

    private static AppConfiguration GetDefaultConfiguration()
    {
        return new AppConfiguration() { IsCalibrationEnabled = false, MyodamCalibration = DeviceCalibration.GetDefaultValues()};
    }

    public AppConfiguration Reload()
    {
        if (!_fileManager.FileExists(ConfigurationFileName)) return new AppConfiguration();

        _configuration = _jsonManager.Deserialize<AppConfiguration>(_fileManager.ReadAll(ConfigurationFileName))
            ?? GetDefaultConfiguration();
        return _configuration;
    }

    public bool IsConfigurationAvailable()
    {
        return _fileManager.FileExists(ConfigurationFileName);
    }

    public void SaveConfig()
    {
        _fileManager.WriteAll(ConfigurationFileName, _jsonManager.Serialize(_configuration));
    }

    public AppConfiguration GetConfiguration()
    {
        return _configuration;
    }
}