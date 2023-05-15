using System.Security.Cryptography.X509Certificates;
using ABI.Windows.Services.Maps;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentAssertions.Execution;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Business.Exception;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Exception;
using Mebster.Myodam.UI.WPF.Navigation;
using Mebster.Myodam.UI.WPF.ViewModels;
using Moq;
using Xunit.Categories;

namespace Mebster.Myodam.UI.WPF.Tests.Navigation;

public class NavigationViewModelTests
{
    private Fixture _fixture;

    public NavigationViewModelTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    [UnitTest]
    public void GetStimulatorAndControllerBatteryPercentage_ShouldReturnZero_WhenNoDevice()
    {
        var myodamManagerMock = _fixture.Freeze<Mock<IMyodamManager>>();
        myodamManagerMock
            .SetupGet(m => m.MyodamDevice)
            .Returns(() => null); 
        var vm = _fixture.Create<NavigationViewModel>();

        using var scope = new AssertionScope();
        vm.ControllerBatteryPercentage.Should().Be(0);
        vm.StimulatorBatteryPercentage.Should().Be(0);
        vm.DeviceError.Should().Be(MyodamError.NoError);
    }

    [Fact]
    [UnitTest]
    public async void LoadAsync_ShouldLoadItemsAndLoadInnerViewModels()
    {
        var itemVm1 = new Mock<INavigationTestSubjectItemViewModel>();
        var itemVm2 = new Mock<INavigationTestSubjectItemViewModel>();

        var deviceCalibrationVmMock = _fixture.Freeze<Mock<IDeviceCalibrationViewModel>>();
        var itemVmFactoryMock = _fixture.Freeze<Mock<INavigationItemViewModelFactory>>();
        itemVmFactoryMock.SetupSequence(m => m.GetViewModel(It.IsAny<TestSubject>()))
            .Returns(itemVm1.Object)
            .Returns(itemVm2.Object);

        var tsRepositoryMock = _fixture.Freeze<Mock<ITestSubjectRepository>>();
        tsRepositoryMock.Setup(m => m.GetAllAsync()).ReturnsAsync(
            new List<TestSubject>()
            {
                new TestSubject(),
                new TestSubject(),
            });
        var vm = _fixture.Create<NavigationViewModel>();

        await vm.LoadAsync();

        using var scope = new AssertionScope();
        tsRepositoryMock.Verify(m => m.GetAllAsync(), Times.Once);
        deviceCalibrationVmMock.Verify(m => m.LoadAsync(), Times.Once);

        vm.TestSubjectsNavigationItems.SourceCollection.Should().BeEquivalentTo(
            new List<INavigationTestSubjectItemViewModel>()
            {
                itemVm1.Object,
                itemVm2.Object
            });
    }

    [Fact]
    [UnitTest]
    public void OnMyodamErrorChanged_ShouldNotifyErrorChangedAndHandleError_WhenDeviceErrorOccurred()
    {
        var deviceMock = new Mock<IMyodamDevice>(); //MyodamDeviceUiWrapper
        deviceMock.SetupGet(m => m.Error).Returns(MyodamError.StimulatorConnectionLost);
        var deviceManagerMock = _fixture.Freeze<Mock<IMyodamManager>>();
        deviceManagerMock.SetupGet(m => m.MyodamDevice).Returns(deviceMock.Object);
        var exceptionHandlerMock = _fixture.Freeze<Mock<IGlobalExceptionHandler>>();
        var vm = _fixture.Create<NavigationViewModel>();

        bool isNotificationRaised = false;
        string propName = "";
        vm.PropertyChanged += (_,e) =>
        {
            propName = e.PropertyName;
            isNotificationRaised = true;
        };

        deviceManagerMock.Raise(s => s.DeviceErrorChanged += null, EventArgs.Empty);

        using var scope = new AssertionScope();
        isNotificationRaised.Should().BeTrue();
        propName.Should().Be(nameof(vm.DeviceError));
        exceptionHandlerMock.Verify( m=>m.HandleExceptionAsync(It.IsAny<DeviceErrorOccurredException>()));
    }

}