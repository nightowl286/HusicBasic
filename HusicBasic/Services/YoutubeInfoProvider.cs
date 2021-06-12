using System;
using System.Collections.Generic;
using System.Text;
using HusicBasic.Models;
using HusicBasic.Services.Interfaces;
using TNO.Json.Runtime;

namespace HusicBasic.Services
{
    public class YoutubeInfoProvider : IYoutubeInfoProvider
    {
        #region Private
        private IYoutubeCLI Youtube;
        #endregion
        public YoutubeInfoProvider(IYoutubeCLI youtube)
        {
            Youtube = youtube;
        }

        #region Methods
        public YoutubeSongInfo AboutSong(string url)
        {
            JsonValue json = Youtube.ParseAsJson($"-f bestaudio -j \"{url}\"");
            if (json == null) return null;
            YoutubeSongInfo info = new YoutubeSongInfo()
            {
                Thumbnail = json["thumbnail"],
                Name = json["title"],
                Views = json["view_count"],
                ID = json["id"],
                Duration = TimeSpan.FromSeconds(json["duration"]),
                Channel = json["channel"],
                ChannelUrl = json["channel_url"],
                AverageRating = json["average_rating"],
            };
            string dateStr = json["upload_date"];
            int year = int.Parse(dateStr[0..4]);
            int month = int.Parse(dateStr[4..6]);
            int day = int.Parse(dateStr[6..8]);
            info.UploadDate = new DateTime(year, month, day);
            foreach(JsonValue thumb in json["thumbnails"])
            {
                if (thumb["id"] == "0")
                {
                    info.Thumbnail = thumb["url"];
                    break;
                }
            }


            return info;
        }
        #endregion
    }
}
