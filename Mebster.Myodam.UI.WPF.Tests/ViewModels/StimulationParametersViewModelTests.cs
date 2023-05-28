using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.UI.WPF.Measurements;
using Mebster.Myodam.UI.WPF.ViewModels;
using Moq;
using Xunit.Categories;

namespace Mebster.Myodam.UI.WPF.Tests.ViewModels;

public class StimulationParametersViewModelTests
{
    private StimulationParametersViewModel _vm;
    private StimulationParameters _stimParamsModel;

    public StimulationParametersViewModelTests()
    {
        _stimParamsModel = new StimulationParameters(0, 0, StimulationPulseWidth.AvailableOptions[0], TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, 0);
        _vm = new StimulationParametersViewModel(_stimParamsModel);
    }


    [Theory]
    [UnitTest]
    [InlineData(5)]
    [InlineData(10)]
    public void SetRestTimeSeconds_ShouldSetModelRestTime_WhenPositiveValueIsProvided(int validTimeInSeconds)
    {
        _vm.RestTimeSeconds = validTimeInSeconds;

        _stimParamsModel.RestTime.Should().Be(TimeSpan.FromSeconds(validTimeInSeconds));
    }

    [Theory]
    [UnitTest]
    [InlineData(-1)]
    [InlineData(-3)]
    public void SetRestTimeSeconds_ShouldThrow_WhenNegativeValueIsProvided(int negativeValue)
    {
        var act = () => _vm.RestTimeSeconds = negativeValue;

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}