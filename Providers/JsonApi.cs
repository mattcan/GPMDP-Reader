using gpmdp_rdr.Music;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace gpmdp_rdr.Providers
{
    public class JsonApi : IProvider
    {
        private Player _player;
        private DateTime _lastUpdate;
        private string _saveLocation;
        private string _jsonApiFile = "playback.json";
        private string _jsonApiDirectory;
        private Logger _logger;

        public JsonApi(string JsonStoreDirectory, Logger Logger) {
            _lastUpdate = DateTime.UtcNow;
            _jsonApiDirectory = JsonStoreDirectory;
            _logger = Logger;
        }

        public async Task Start(string saveFileName) {
            _logger.Debug("Starting JSON API Reader");
            this.Run(_jsonApiDirectory, saveFileName);
        }

        public bool IsUseable() {
            if (!Directory.Exists(_jsonApiDirectory)) { return false; }

            if (!File.Exists(Path.Combine(_jsonApiDirectory, _jsonApiFile))) { return false; }

            var lastAccess = File.GetLastAccessTimeUtc(Path.Combine(_jsonApiDirectory, _jsonApiFile));
            if (lastAccess.AddMinutes(15) >= DateTime.UtcNow) { return false; }

            _logger.Debug("JsonAPI is useable");
            return true;
        }

        public void Run(string JsonStoreDirectory, string SaveSongNameLocation) {
            _saveLocation = SaveSongNameLocation;
            _player = new Player(SaveSongNameLocation);

            // Handle process exiting here as this is where we have the most context
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);

            FileSystemWatcher watcher = new FileSystemWatcher() {
                Path = JsonStoreDirectory,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite,
                Filter = _jsonApiFile
            };
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Error += new ErrorEventHandler(OnError);
            watcher.EnableRaisingEvents = true;
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

            _logger.Debug($"JSON says song is playing: {song.Playing}");

            _player.Update(song);
        }

        private void OnError(object source, System.IO.ErrorEventArgs e) {
            Console.WriteLine($"Error occurred: {e.GetException().Message}");
            Program.ExitWith(ExitCode.JSON_API_EXCEPTION);
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