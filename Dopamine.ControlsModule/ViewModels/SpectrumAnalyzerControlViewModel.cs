﻿using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Playback;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class SpectrumAnalyzerControlViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        private bool showSpectrumAnalyzer;
        private bool isPlaying;
        private double blurRadius = 20;
        private int spectrumBarCount = 74;
        private double spectrumWidth = 275;
        private double spectrumBarWidth = 4;
        private double spectrumBarSpacing = 0;
        private double spectrumPanelHeight = 60;
        #endregion

        #region Properties
        public bool ShowSpectrumAnalyzer
        {
            get { return this.showSpectrumAnalyzer; }
            set { SetProperty<bool>(ref this.showSpectrumAnalyzer, value); }
        }

        public bool IsPlaying
        {
            get { return this.isPlaying; }
            set { SetProperty<bool>(ref this.isPlaying, value); }
        }

        public double BlurRadius
        {
            get { return this.blurRadius; }
            set { SetProperty<double>(ref this.blurRadius, value); }
        }

        public int SpectrumBarCount
        {
            get { return this.spectrumBarCount; }
            set { SetProperty<int>(ref this.spectrumBarCount, value); }
        }

        public double SpectrumWidth
        {
            get { return this.spectrumWidth; }
            set {
                SetProperty<double>(ref this.spectrumWidth, value);
                OnPropertyChanged(() => this.SpectrumPanelWidth);
            }
        }

        public double SpectrumBarWidth
        {
            get { return this.spectrumBarWidth; }
            set { SetProperty<double>(ref this.spectrumBarWidth, value); }
        }

        public double SpectrumBarSpacing
        {
            get { return this.spectrumBarSpacing; }
            set { SetProperty<double>(ref this.spectrumBarSpacing, value); }
        }

        public double SpectrumPanelWidth
        {
            get { return this.SpectrumWidth * 2; }
        }

        public double SpectrumPanelHeight
        {
            get { return this.spectrumPanelHeight; }
            set { SetProperty<double>(ref this.spectrumPanelHeight, value); }
        }
        #endregion

        #region Construction
        public SpectrumAnalyzerControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.playbackService.SpectrumVisibilityChanged += isSpectrumVisible => this.ShowSpectrumAnalyzer = isSpectrumVisible;

            this.playbackService.PlaybackFailed += (_, __) => this.IsPlaying = false;
            this.playbackService.PlaybackStopped += (_, __) => this.IsPlaying = false;
            this.playbackService.PlaybackPaused += (_, __) => this.IsPlaying = false;
            this.playbackService.PlaybackResumed += (_, __) => this.IsPlaying = true;
            this.playbackService.PlaybackSuccess += (_) => this.IsPlaying = true;

            this.ShowSpectrumAnalyzer = SettingsClient.Get<bool>("Playback", "ShowSpectrumAnalyzer");

            // Initial value
            if (!this.playbackService.IsStopped & this.playbackService.IsPlaying)
            {
                this.IsPlaying = true;
            }
            else
            {
                this.IsPlaying = false;
            }
        }
        #endregion
    }
}
