using DiscordRPC;
using DiscordRPC.Logging;
using Dopamine.Services.Entities;
using Dopamine.Services.Playback;
using System;
using System.Text;

namespace Dopamine.Services.DiscordRPC
{
    public class DiscordRPCService : IDiscordRPCService
    {
        private IPlaybackService PlaybackService;
        private DiscordRpcClient RpcClient;
        private RichPresence Presence => RpcClient.CurrentPresence;
        private TrackViewModel CurrentTrack => PlaybackService.CurrentTrack;

        public DiscordRPCService(IPlaybackService playbackService)
        {
            PlaybackService = playbackService;
            PlaybackService.PlaybackSuccess += OnPlaybackStarted;
            PlaybackService.PlaybackResumed += OnPlaybackResumed;
            PlaybackService.PlaybackPaused += OnPlaybackPaused;
            PlaybackService.PlaybackStopped += OnPlaybackStopped;

            RpcClient = new DiscordRpcClient("592217166770864138");
            RpcClient.Initialize();
            RpcClient.SetPresence(new RichPresence());
            Presence.Assets = new Assets();

            RpcClient.OnReady += (send, s) => Console.WriteLine("Received Ready from user {0}", s.User.Username);
            RpcClient.OnPresenceUpdate += (send, s) => Console.WriteLine("Received Update! {0}", s.Presence);
            RpcClient.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
        }

        private void OnPlaybackPaused(object sender, PlaybackPausedEventArgs e)
        {
            Presence.Timestamps.Start = null;
            Presence.Timestamps.End = null;
            PopulateWithTrack();
            RpcClient.SetPresence(Presence);
        }

        public void OnPlaybackStarted(object sender, PlaybackSuccessEventArgs e)
        {
            PopulateWithTrack();
            PopulateTimestamps();
            PushState();
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            RpcClient.ClearPresence();
            PushState();
        }


        public void OnPlaybackResumed(object sender, EventArgs e)
        {
            EnsurePopulatedPresence();
            PopulateWithTrack();
            PopulateTimestamps();
            PushState();
        }

        public void EnsurePopulatedPresence()
        {
            if(Presence == null)
                RpcClient.SetPresence(new RichPresence());
        }

        public void PopulateWithTrack()
        {
            string str = PlaybackService.IsPlaying ? "Playing" : "Paused on";
            Presence.Details = $"{str} {CurrentTrack.TrackTitle}";
            Presence.State = $"by {CurrentTrack.AlbumArtist}\non {CurrentTrack.AlbumTitle}";
            Assets assets = new Assets
            {
                LargeImageKey = $"{CurrentTrack.AlbumTitle} {CurrentTrack.ArtistName}".Replace(" ", "").ToLower(),
                SmallImageKey = "dopamine"
            };
            Presence.Assets = assets;
        }

        public void PopulateTimestamps()
        {
            double length = PlaybackService.GetTotalTime.TotalSeconds;
            double progressFraction = PlaybackService.Progress;
            if (double.IsNaN(progressFraction))
                progressFraction = 1.0;
            double progress = length * progressFraction;

            DateTime start = DateTime.Now - TimeSpan.FromSeconds(progress);
            DateTime end = DateTime.Now + TimeSpan.FromSeconds(length - progress);

            Timestamps timestamps = new Timestamps
            {
                Start = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc),
                End = end
            };
            Presence.Timestamps = timestamps;
        }

        public void PushState()
        {
            RpcClient.SetPresence(Presence);
        }

        public string ToBase64(string inp)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(inp));
        }
    }
}
