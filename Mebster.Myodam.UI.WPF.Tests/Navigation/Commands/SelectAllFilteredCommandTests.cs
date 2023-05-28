﻿using System.Windows.Data;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Navigation;
using Mebster.Myodam.UI.WPF.Navigation.Commands;
using Mebster.Myodam.UI.WPF.ViewModels;
using Moq;
using Xunit.Categories;

namespace Mebster.Myodam.UI.WPF.Tests.Navigation.Commands;

public class SelectAllFilteredCommandTests
{
    [Fact]
    [UnitTest]
    public void Execute_ShouldDeselectAllItems()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());
        var vmMock = new Mock<INavigationViewModel>();
        fixture.Inject(StimulationPulseWidth.AvailableOptions[0]);
        fixture.Inject(fixture.Build<TestSubject>().Without(t => t.Measurements).Create());
        var list = new List<NavigationItemViewModel>();
        for (int i = 0; i < 3; i++)
        {
            list.Add(fixture
                .Build<NavigationItemViewModel>()
                .With(s => s.IsSelectedForExport, false)
                .Create());
        };
        vmMock.Setup(m => m.TestSubjectsNavigationItems).Returns(new ListCollectionView(list));
        var command = new SelectAllFilteredCommand(vmMock.Object);


        command.Execute(null);


        list.Should().AllSatisfy(item =>
        {
            item.IsSelectedForExport.Should().BeTrue();
        });
    }
}