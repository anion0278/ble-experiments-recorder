using System.CodeDom;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Mebster.Myodam.DataAccess.FileStorage;

namespace Mebster.Myodam.UI.WPF.ViewModels.Services;

public class AppConfiguration
{
    public bool IsCalibrationEnabled { get; init; }
}

public interface IAppConfigurationLoader
{
    AppConfiguration Reload();
    AppConfiguration GetConfiguration();
}

public class AppConfigurationLoader : IAppConfigurationLoader
{
    private readonly IJsonManager _jsonManager;
    private readonly IFileManager _fileManager;
    private string _configurationFileName = "config.json";
    private AppConfiguration _configuration = GetDefaultConfiguration();

    public AppConfigurationLoader(IJsonManager jsonManager, IFileManager fileManager)
    {
        _jsonManager = jsonManager;
        _fileManager = fileManager;
        Reload();
    }

    private static AppConfiguration GetDefaultConfiguration()
    {
        return new AppConfiguration() { IsCalibrationEnabled = false };
    }

    public AppConfiguration Reload()
    {
        if (!_fileManager.FileExists(_configurationFileName)) return new AppConfiguration();

        _configuration = _jsonManager.Deserialize<AppConfiguration>(_fileManager.ReadAll(_configurationFileName))
            ?? GetDefaultConfiguration();
        return _configuration;
    }

    public AppConfiguration GetConfiguration()
    {
        return _configuration;
    }
}