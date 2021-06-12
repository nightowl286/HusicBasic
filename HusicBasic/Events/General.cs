using System;
using System.Collections.Generic;
using System.Text;
using HusicBasic.Models;
using Prism.Events;

namespace HusicBasic.Events
{
    public class SongRemovedEvent : PubSubEvent<SongModel> { }
    public class PlaylistRemovedEvent : PubSubEvent<PlaylistModel> { }
    public class VolumeChangeRequestEvent : PubSubEvent<double> { }
}
