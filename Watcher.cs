using System;
using System.IO;
using System.Security.Permissions;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace gpmdp_rdr
{
    public class Watcher
    {
        private Player _player;
        private DateTime _lastUpdate;

        public Watcher() {
            _lastUpdate = DateTime.UtcNow;
        }

        private bool ValidatePaths(string JsonStoreDirectory, string SaveSongNameLocation) {
            if (!Directory.Exists(JsonStoreDirectory)) { return false; }

            // TODO pop file name and check directory for song

            return true;
        }

        // TODO is this necessary?
        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public void Run(string JsonStoreDirectory, string SaveSongNameLocation) {

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
            _player.Update(song);
        }

        private void OnError(object source, System.IO.ErrorEventArgs e) {
            Console.WriteLine($"Error occurred: {e.GetException().Message}");
            Environment.Exit(2);
        }
    }
}