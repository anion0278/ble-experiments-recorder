using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using BleRecorder.Models.Device;
using BleRecorder.UI.WPF.ViewModels;
using Moq;

namespace BleRecorder.UI.WPF.Tests.ViewModels;

public class StimulationParametersViewModelTests
{
    private StimulationParametersViewModel _vm;
    private Mock<IStimulationParameters> _stimParamsModelMock;

    public StimulationParametersViewModelTests()
    {
        _stimParamsModelMock = new Mock<IStimulationParameters>();
        _vm = new StimulationParametersViewModel(_stimParamsModelMock.Object);
    }


    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    public void SetRestTimeSeconds_ShouldSetModelRestTime_WhenPositiveValueIsProvided(int validTimeInSeconds)
    {
        //var fixture = new Fixture();
        //fixture.Customize(new AutoMoqCustomization());
        //var _stimParamsModelMock = fixture.Freeze<Mock<IStimulationParameters>>();
        //_stimParamsModelMock.SetupProperty( m => m.PulseWidth, StimulationPulseWidth.AvailableOptions[0]);
        //var _vm = fixture.Create<StimulationParametersViewModel>();

        _vm.RestTimeSeconds = validTimeInSeconds;

        _stimParamsModelMock.VerifySet(m => m.RestTime = TimeSpan.FromSeconds(validTimeInSeconds));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-3)]
    public void SetRestTimeSeconds_ShouldThrow_WhenNegativeValueIsProvided(int negativeValue)
    {
        var act = () => _vm.RestTimeSeconds = negativeValue;

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}