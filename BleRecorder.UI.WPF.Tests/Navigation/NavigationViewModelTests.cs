﻿using AutoFixture;
using AutoFixture.AutoMoq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using FluentAssertions.Execution;
using BleRecorder.Business.Device;
using BleRecorder.Business.Exception;
using BleRecorder.DataAccess.DataExport;
using BleRecorder.DataAccess.FileStorage;
using BleRecorder.DataAccess.Repositories;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubjects;
using BleRecorder.UI.WPF.Calibration;
using BleRecorder.UI.WPF.Event;
using BleRecorder.UI.WPF.Exception;
using BleRecorder.UI.WPF.Navigation;
using BleRecorder.UI.WPF.Navigation.Commands;
using BleRecorder.UI.WPF.TestSubjects;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit.Categories;

namespace BleRecorder.UI.WPF.Tests.Navigation;

public class NavigationViewModelTests
{
    private readonly Fixture _fixture;

    public NavigationViewModelTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    [UnitTest]
    public void GetStimulatorAndControllerBatteryPercentage_ShouldReturnZero_WhenNoDevice()
    {
        var bleRecorderManagerMock = _fixture.Freeze<Mock<IBleRecorderManager>>();
        bleRecorderManagerMock
            .SetupGet(m => m.BleRecorderDevice)
            .Returns(() => null);
        var vm = _fixture.Create<NavigationViewModel>();

        using var scope = new AssertionScope();
        vm.ControllerBatteryPercentage.Should().Be(0);
        vm.StimulatorBatteryPercentage.Should().Be(0);
        vm.DeviceError.Should().Be(BleRecorderError.NoError);
    }

    [Fact]
    [UnitTest]
    public async void LoadAsync_ShouldLoadItemsAndLoadInnerViewModels()
    {
        var itemVm1 = new Mock<INavigationItemViewModel>();
        var itemVm2 = new Mock<INavigationItemViewModel>();

        var deviceCalibrationVmMock = _fixture.Freeze<Mock<IDeviceCalibrationViewModel>>();
        var itemVmFactoryMock = _fixture.Freeze<Mock<INavigationItemViewModelFactory>>();
        itemVmFactoryMock.SetupSequence(m => m.GetViewModel(It.IsAny<TestSubject>()))
            .Returns(itemVm1.Object)
            .Returns(itemVm2.Object);

        var tsRepositoryMock = _fixture.Freeze<Mock<ITestSubjectRepository>>();
        tsRepositoryMock.Setup(m => m.GetAllAsync())
            .ReturnsAsync(new TestSubject[] { new(), new() });
        var vm = _fixture.Create<NavigationViewModel>();

        await vm.LoadAsync();

        using var scope = new AssertionScope();
        tsRepositoryMock.Verify(m => m.GetAllAsync(), Times.Once);
        deviceCalibrationVmMock.Verify(m => m.LoadAsync(), Times.Once);

        vm.TestSubjectsNavigationItems.SourceCollection.Should().BeEquivalentTo(
            new List<INavigationItemViewModel>()
            {
                itemVm1.Object,
                itemVm2.Object
            });
    }

    [Fact]
    [UnitTest]
    public void OnBleRecorderErrorChanged_ShouldNotifyErrorChangedAndHandleError_WhenDeviceErrorOccurred()
    {
        var deviceMock = new Mock<IBleRecorderDevice>();
        deviceMock.SetupGet(m => m.Error).Returns(BleRecorderError.StimulatorConnectionLost);

        var deviceManagerMock = _fixture.Freeze<Mock<IBleRecorderManager>>();
        deviceManagerMock.SetupGet(m => m.BleRecorderDevice).Returns(deviceMock.Object);

        var exceptionHandlerMock = _fixture.Freeze<Mock<IGlobalExceptionHandler>>();
        var vm = _fixture.Create<NavigationViewModel>();

        string propName = "";
        vm.PropertyChanged += (_, e) =>
        {
            propName = e.PropertyName;
        };

        deviceManagerMock.Raise(s => s.DeviceErrorChanged += null, EventArgs.Empty);

        using var scope = new AssertionScope();
        propName.Should().Be(nameof(vm.DeviceError));
        exceptionHandlerMock.Verify(m => m.HandleExceptionAsync(It.IsAny<DeviceErrorOccurredException>()));
    }


    [Fact]
    [UnitTest]
    public void OnBleRecorderPropertyChanged_ShouldNotifyAllBleRecorderPropertiesChanged()
    {
        var deviceManagerMock = _fixture.Freeze<Mock<IBleRecorderManager>>();
        var vm = _fixture.Create<NavigationViewModel>();

        var propNames = new List<string>();
        vm.PropertyChanged += (_, e) => { propNames.Add(e.PropertyName); };

        deviceManagerMock.Raise(s => s.DevicePropertyChanged += null, EventArgs.Empty);

        propNames.Should().BeEquivalentTo(
            nameof(INavigationViewModel.ControllerBatteryPercentage),
            nameof(INavigationViewModel.StimulatorBatteryPercentage),
            nameof(INavigationViewModel.BleRecorderAvailability));
    }


    [Fact]
    [UnitTest]
    public void OnBleRecorderAvailabilityChanged_ShouldNotifyAllBleRecorderPropertiesAndCommandsChanged()
    {
        var deviceManagerMock = _fixture.Freeze<Mock<IBleRecorderManager>>();
        var commandFactoryMock = _fixture.Freeze<Mock<INavigationViewModelCommandsFactory>>();

        var exportCommandMock = _fixture.Create<Mock<IAsyncRelayCommand>>();
        var changeBleRecorderConnectionCommandMock = _fixture.Create<Mock<IAsyncRelayCommand>>();

        commandFactoryMock.Setup(m => m.GetExportSelectedCommand(
            It.IsAny<INavigationViewModel>(),
            It.IsAny<IBleRecorderManager>(),
            It.IsAny<ITestSubjectRepository>(),
            It.IsAny<IMessageDialogService>(),
            It.IsAny<IDocumentManager>(),
            It.IsAny<IFileSystemManager>()))
            .Returns(exportCommandMock.Object);

        commandFactoryMock.Setup(m => m.GetChangeBleRecorderConnectionCommand(
                It.IsAny<INavigationViewModel>(),
                It.IsAny<IBleRecorderManager>(),
                It.IsAny<IMessageDialogService>()))
            .Returns(changeBleRecorderConnectionCommandMock.Object);

        var vm = _fixture.Create<NavigationViewModel>();

        var propNames = new List<string>();
        vm.PropertyChanged += (_, e) => { propNames.Add(e.PropertyName); };

        deviceManagerMock.Raise(s => s.BleRecorderAvailabilityChanged += null, EventArgs.Empty);

        propNames.Should().BeEquivalentTo(
            nameof(INavigationViewModel.ControllerBatteryPercentage),
            nameof(INavigationViewModel.StimulatorBatteryPercentage),
            nameof(INavigationViewModel.BleRecorderAvailability));

        exportCommandMock.Verify(m => m.NotifyCanExecuteChanged(), Times.Once);
        changeBleRecorderConnectionCommandMock.Verify(m => m.NotifyCanExecuteChanged(), Times.Once);
    }

    [Fact]
    [UnitTest]
    public async void OnDetailDeleteMessage_ShouldDoNothing_WhenDeletedDetailIsUnrelated()
    {
        // Arrange
        int unrelatedId = 9;
        SetupVm(out var itemVmFactoryMock, out var messenger, out var tsRepositoryMock);
        var vm = _fixture.Create<NavigationViewModel>();
        await vm.LoadAsync();

        // Act
        messenger.Send(new AfterDetailDeletedEventArgs()
        {
            Id = unrelatedId,
            ViewModelName = nameof(TestSubjectDetailViewModel)
        });

        //Assert
        vm.TestSubjectsNavigationItems.SourceCollection.Cast<object>().Count().Should().Be(2);
    }

    [Fact]
    [UnitTest]
    public async void OnDetailDeleteMessage_ShouldDoNothing_WhenDeletedDetailRelatedToItem()
    {
        // Arrange
        int existentId = 1;
        SetupVm(out var itemVmFactoryMock, out var messenger, out var tsRepositoryMock);
        var vm = _fixture.Create<NavigationViewModel>();
        await vm.LoadAsync();

        // Act
        messenger.Send(new AfterDetailDeletedEventArgs()
        {
            Id = existentId,
            ViewModelName = nameof(TestSubjectDetailViewModel)
        });

        //Assert
        vm.TestSubjectsNavigationItems.SourceCollection.Cast<object>().Count().Should().Be(1);
    }

    [Fact]
    [UnitTest]
    public async void OnDetailSavedMessage_ShouldAddItem_WhenDetailItemDoesNotExistYet()
    {
        // Arrange
        int nonExistentId = 9;
        SetupVm(out var itemVmFactoryMock, out var messenger, out var tsRepositoryMock);
        var vm = _fixture.Create<NavigationViewModel>();
        await vm.LoadAsync();
        tsRepositoryMock.Setup(m => m.GetByIdAsync(nonExistentId))
            .ReturnsAsync(() => null);

        // Act
        messenger.Send(new AfterDetailSavedEventArgs()
        {
            Id = nonExistentId,
            ViewModelName = nameof(TestSubjectDetailViewModel)
        });

        //Assert
        using var scope = new AssertionScope();
        tsRepositoryMock.Verify(m => m.GetByIdAsync(nonExistentId), Times.Once);
        itemVmFactoryMock.Verify(m => m.GetViewModel(null), Times.Once);
    }

    [Fact]
    [UnitTest]
    public async void OnDetailSavedMessage_ShouldDoNothing_WhenDetailItemAlreadyExists()
    {
        // Arrange
        int existingId = 1;
        SetupVm(out var itemVmFactoryMock, out var messenger, out var tsRepositoryMock);
        var vm = _fixture.Create<NavigationViewModel>();
        await vm.LoadAsync();
        tsRepositoryMock.Setup(m => m.GetByIdAsync(existingId))
            .ReturnsAsync(() => null);


        // Act
        messenger.Send(new AfterDetailSavedEventArgs()
        {
            Id = existingId,
            ViewModelName = nameof(TestSubjectDetailViewModel)
        });

        //Assert
        using var scope = new AssertionScope();
        tsRepositoryMock.Verify(m => m.GetByIdAsync(existingId), Times.Never);
        itemVmFactoryMock.Verify(m => m.GetViewModel(null), Times.Never);
    }

    private void SetupVm(
        out Mock<INavigationItemViewModelFactory> itemVmFactoryMock,
        out WeakReferenceMessenger messenger,
        out Mock<ITestSubjectRepository> tsRepositoryMock)
    {
        var item1 = new Mock<INavigationItemViewModel>();
        var item2 = new Mock<INavigationItemViewModel>();

        item1.SetupGet(m => m.Id).Returns(1);
        item2.SetupGet(m => m.Id).Returns(2);

        itemVmFactoryMock = _fixture.Freeze<Mock<INavigationItemViewModelFactory>>();
        itemVmFactoryMock.SetupSequence(m => m.GetViewModel(It.IsAny<TestSubject>()))
            .Returns(item1.Object)
            .Returns(item2.Object);

        messenger = new WeakReferenceMessenger();
        _fixture.Inject<IMessenger>(messenger);

        tsRepositoryMock = _fixture.Freeze<Mock<ITestSubjectRepository>>();

        tsRepositoryMock.Setup(m => m.GetAllAsync())
            .ReturnsAsync(new TestSubject[] { new(), new() });
    }


    //[Fact]
    //public async void SetFullNameFilter_ShouldReturnOnlyFilteredItems_WhenGivenMultipleItems()
    //{
    //    var includedItem1 = new Mock<INavigationItemViewModel>();
    //    var includedItem2 = new Mock<INavigationItemViewModel>();
    //    var excludedItem = new Mock<INavigationItemViewModel>();
    //    //excludedItem.SetupProperty(m => m.Model.FirstName, "ExcludedItem");

    //    //includedItem1.SetupProperty(m => m.Model.LastName,"IncludedItem");
    //    //includedItem2.SetupProperty(m => m.Model.FirstName,"IncludedItem");

    //    excludedItem.SetupGet(m => m.ItemName).Returns("ExcludedItem");
    //    includedItem1.SetupGet(m => m.ItemName).Returns("IncludedItem");
    //    includedItem2.SetupGet(m => m.ItemName).Returns("IncludedItem");

    //    var itemVmFactoryMock = _fixture.Freeze<Mock<INavigationItemViewModelFactory>>();
    //    itemVmFactoryMock.SetupSequence(m => m.GetViewModel(It.IsAny<TestSubject>()))
    //        .Returns(includedItem1.Object)
    //        .Returns(excludedItem.Object)
    //        .Returns(includedItem2.Object);

    //    var tsRepositoryMock = _fixture.Freeze<Mock<ITestSubjectRepository>>();
    //    tsRepositoryMock.Setup(m => m.GetAllAsync()).ReturnsAsync(new List<TestSubject>() { new(),  new(), new(), });
    //    var vm = _fixture.Create<NavigationViewModel>();

    //    vm.LoadAsync();

    //    vm.FullNameFilter = "Included";

    //    vm.TestSubjectsNavigationItems.Should()
    //        .BeEquivalentTo(new []{includedItem1.Object, includedItem2.Object});
    //}
}