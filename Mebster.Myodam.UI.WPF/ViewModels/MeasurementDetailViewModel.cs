﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Mebster.Myodam.Models.TestSubject;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.View.Services;
using Mebster.Myodam.UI.WPF.Wrapper;
using Prism.Commands;
using Prism.Events;
using SkiaSharp;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class MeasurementDetailViewModel : DetailViewModelBase, IMeasurementDetailViewModel
    {
        private IMeasurementRepository _measurementRepository;
        private MeasurementWrapper _measurement;
        private TestSubject _selectedAvailableTestSubject;
        private TestSubject _selectedAddedTestSubject;
        private List<TestSubject> _allTestSubjects;

        public ISeries[] SeriesCollection { get; set; } = {
            new LineSeries<double>
            {
                Values = new double[] { 2, 1, 3, 5, 3, 4, 6 },
                Fill = new LinearGradientPaint(SKColors.Aqua, new SKColor(50, 40, 130)),
                Name = "Data from MYODAM",
                GeometrySize = 6.0,
            }
        };


        public ICommand AddTestSubjectCommand { get; }

        public ICommand RemoveTestSubjectCommand { get; }

        public ObservableCollection<TestSubject> AddedTestSubjects { get; }

        public ObservableCollection<TestSubject> AvailableTestSubjects { get; }

        public MeasurementWrapper Measurement
        {
            get { return _measurement; }
            private set
            {
                _measurement = value;
                OnPropertyChanged();
            }
        }

        public TestSubject SelectedAvailableTestSubject
        {
            get { return _selectedAvailableTestSubject; }
            set
            {
                _selectedAvailableTestSubject = value;
                OnPropertyChanged();
                ((DelegateCommand)AddTestSubjectCommand).RaiseCanExecuteChanged();
            }
        }

        public TestSubject SelectedAddedTestSubject
        {
            get { return _selectedAddedTestSubject; }
            set
            {
                _selectedAddedTestSubject = value;
                OnPropertyChanged();
                ((DelegateCommand)RemoveTestSubjectCommand).RaiseCanExecuteChanged();
            }
        }

        public MeasurementDetailViewModel(IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IMeasurementRepository measurementRepository) : base(eventAggregator, messageDialogService)
        {
            _measurementRepository = measurementRepository;
            eventAggregator.GetEvent<AfterDetailSavedEvent>().Subscribe(AfterDetailSaved);
            eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);

            AddedTestSubjects = new ObservableCollection<TestSubject>();
            AvailableTestSubjects = new ObservableCollection<TestSubject>();
        }

        public override async Task LoadAsync(int measurementId)
        {
            var measurement = _measurementRepository.GetByIdAsync(measurementId);
            Id = measurementId;

            //_allTestSubjects = await _measurementRepository.GetAllTestSubjectsAsync();
        }

        protected override async void OnDeleteExecute()
        {
            var result = await MessageDialogService.ShowOkCancelDialogAsync($"Do you really want to delete the measurement {Measurement.Title}?", "Question");
            if (result == MessageDialogResult.OK)
            {
                //_measurementRepository.Remove(Measurement.Model);
                await _measurementRepository.SaveAsync();
                RaiseDetailDeletedEvent(Measurement.Id);
            }
        }

        protected override bool OnSaveCanExecute()
        {
            return Measurement != null && !Measurement.HasErrors && HasChanges;
        }

        protected override async void OnSaveExecute()
        {
            await _measurementRepository.SaveAsync();
            HasChanges = _measurementRepository.HasChanges();
            Id = Measurement.Id;
            RaiseDetailSavedEvent(Measurement.Id, Measurement.Title);
        }


        private async void AfterDetailSaved(AfterDetailSavedEventArgs args)
        {
            if (args.ViewModelName == nameof(TestSubjectDetailViewModel))
            {
                await _measurementRepository.ReloadTestSubjectAsync(args.Id);
                _allTestSubjects = await _measurementRepository.GetAllTestSubjectsAsync();
            }
        }

        private async void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            if (args.ViewModelName == nameof(TestSubjectDetailViewModel))
            {
                _allTestSubjects = await _measurementRepository.GetAllTestSubjectsAsync();
            }
        }
    }
}
