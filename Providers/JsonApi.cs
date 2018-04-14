using System;
using System.IO;
using System.Security.Permissions;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using gpmdp_rdr.Music;

namespace gpmdp_rdr.Providers
{
    public class JsonApi : IProvider
    {
        private Player _player;
        private DateTime _lastUpdate;
        private string _saveLocation;
        private string _jsonApiFile = "playback.json";
        private string _jsonApiDirectory;

        public JsonApi(string JsonStoreDirectory) {
            _lastUpdate = DateTime.UtcNow;
            _jsonApiDirectory = JsonStoreDirectory;
        }

        public void Start() {
            this.Run(_jsonApiDirectory, "current_song.txt");
        }

        public bool IsUseable() {
            return false;
        }

        private bool ValidatePaths(string JsonStoreDirectory, string SaveSongNameLocation) {
            if (!Directory.Exists(JsonStoreDirectory)) { return false; }

            // TODO pop file name and check directory for song

            return true;
        }

        // TODO is this necessary?
        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public void Run(string JsonStoreDirectory, string SaveSongNameLocation) {
            // Handle process exiting here as this is where we have the most context
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);

            _saveLocation = SaveSongNameLocation;

            if (!this.ValidatePaths(JsonStoreDirectory, SaveSongNameLocation)) {
                Console.WriteLine("Paths are not valid");
                Console.WriteLine("Usage: obs-gpmdp <json-store-directory> <write-to>");
                return;
            }

            _player = new Player(SaveSongNameLocation);

            FileSystemWatcher watcher = new FileSystemWatcher() {
                Path = JsonStoreDirectory,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite,
                Filter = "playback.json"
            };
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Error += new ErrorEventHandler(OnError);
            watcher.EnableRaisingEvents = true;

            // TODO use this as a daemon/service
            Console.WriteLine("Press \'q\' to quit the sample.");
            while(Console.Read()!='q');
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Rate limit events
            if ((DateTime.UtcNow - _lastUpdate).TotalSeconds <= 2) {
                return;
            }
            _lastUpdate = DateTime.UtcNow;

            string contents = "";
            using (StreamReader reader = File.OpenText(e.FullPath)) {
                contents = reader.ReadToEnd();
            }

            if (contents == string.Empty) {
                // Don't need to do anything, seems to fix itself pretty quickly
                // Very likely this bug from the GPMDP README:
                //
                // >NOTE: On some linux distros the file system does not lock
                // >the JSON file correctly and sometimes it will be empty
                // >when you try to read it. (This is really rare)
                // - https://github.com/MarshallOfSound/Google-Play-Music-Desktop-Player-UNOFFICIAL-/blob/master/docs/PlaybackAPI.md#playback-information-api
                //
                // As far as I can tell, this happens more than is let on but
                // thats because GPMDP is writing more than once per second.

                return;
            }

            JObject playback = JObject.Parse(contents);
            JToken songJson = playback["song"];

            Song song = songJson.ToObject<Song>();
            song.Playing = playback["playing"].ToObject<bool>();

            Console.WriteLine($"JSON says song is playing: {song.Playing}");

            _player.Update(song);
        }

        private void OnError(object source, System.IO.ErrorEventArgs e) {
            Console.WriteLine($"Error occurred: {e.GetException().Message}");
            Environment.Exit(2);
        }

        private void ProcessExit(object sender, EventArgs e) {
            if (_saveLocation == string.Empty) { return; }

            // Write a final line to the song file
            try {
                File.WriteAllText(_saveLocation, "GPMDP Reader has stopped");
            } catch (Exception exception) {
                // We're exiting, these don't matter
            }

            return;
        }
    }
}