﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nito.AsyncEx;
using WindowsMediaController;

namespace LeStudioCurrentSongCLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var currentSong = new CurrentSong();

            Console.WriteLine(AsyncContext.Run(currentSong.getCurrentSong));

            //DEBUG PURPOSE
            //Console.ReadLine();
        }
    }

    class CurrentSong
    {
        private String titleString = null;
        private String albumString = null;
        private String authorString = null;
        private String imageBase64 = null;
        private Boolean isPlayingBool = false;

        private async Task LoadCurrentSong()
        {
            var mediaManager = new MediaManager();
            try
            {
                await mediaManager.StartAsync();

                var session = mediaManager.GetFocusedSession();

                if(session == null)
                {
                    return;
                }

                var playbackInfo = session.ControlSession.GetPlaybackInfo();
                var mediaProperties = await session.ControlSession.TryGetMediaPropertiesAsync();

                this.isPlayingBool = playbackInfo.PlaybackStatus == Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                this.titleString = mediaProperties.Title;
                this.authorString = mediaProperties.Artist;
                this.albumString = mediaProperties.AlbumTitle;

                try
                {
                    Image image = Image.FromStream((await mediaProperties.Thumbnail.OpenReadAsync()).AsStream());
                    String imgstr;
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);
                        imgstr = base64String;
                    }
                    this.imageBase64 = imgstr;
                }
                catch (System.Exception)
                {
                    this.imageBase64 = null;
                }
            }
            catch (System.Exception) { }
        }

        public async Task<String> getCurrentSong()
        {

            await LoadCurrentSong();

            var values = new Dictionary<string, dynamic>
                {
                    { "currentSongTitle", this.titleString },
                    { "currentSongAuthor", this.authorString },
                    { "currentSongAlbum", this.albumString },
                    { "currentSongImage", this.imageBase64 },
                    { "currentSongIsPlaying", this.isPlayingBool == true ? true : false  },
                };

            var content = JsonConvert.SerializeObject(values);

            return content;
        }
    }
}
