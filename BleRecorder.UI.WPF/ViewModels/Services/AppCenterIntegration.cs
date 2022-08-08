using System.Linq;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Configuration;

namespace BleRecorder.UI.WPF.ViewModels.Services;

public interface IAppCenterIntegration
{
    void TrackException(System.Exception exception);
}

public class AppCenterIntegration : IAppCenterIntegration
{
    public AppCenterIntegration()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<App>().Build();
        var secretProvider = config.Providers.First();
        secretProvider.TryGet("appCenterKey", out var appCenterKey); // this requires to have user-secrets defined

        AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));
    }

    public void TrackException(System.Exception exception)
    {
        Crashes.TrackError(exception);
    }
}