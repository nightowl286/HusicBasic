using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using HusicBasic.Models;

namespace HusicBasic.Services
{

    public enum RepeatMode
    {
        Dont,
        Current,
        All
    }

    public interface IHusicPlayer : INotifyPropertyChanged
    {
        #region Properties
        public double Volume { get; set; }
        public double Balance { get; set; }
        public TimeSpan Position { get; set; }
        public TimeSpan Duration { get; }
        public bool IsMuted { get; set; }
        public bool IsPaused { get; set; }
        public bool IsPlaying { get; }
        public SongModel CurrentSong { get; }
        public bool CanPlayPrevious { get; }
        public bool CanPlayNext { get; }
        public bool IsLoaded { get; }
        public RepeatMode Repeat { get; set; }
        public IPlayQueue Queue { get; }
        public ISongShuffler Shuffler { get; }
        #endregion

        #region Methods
        void PlayPrevious();
        void PlayNext();
        void SwitchPlaybackTo(SongModel song);
        void Play(PlaylistModel playlist);
        void PlaySong(SongModel song);
        void ToggleMute();
        void TogglePause();
        void Mute();
        void Unmute();
        void Pause();
        void Unpause();
        void CircleRepeatMode();
        void CircleShuffler();
        #endregion
    }
}
