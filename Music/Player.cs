using System;
using System.IO;

namespace gpmdp_rdr.Music
{
    public class Player
    {
        private Song _currentSong;
        private string _saveLocation;

        private bool _isPlaying = false;

        public Player(String writeToFilePath) {
            _saveLocation = writeToFilePath;
            _currentSong = new Song();
        }

        public void Update(Song song) {
            if (!SongHasChanged(song)) { return; }

            _currentSong = song;

            WriteToFile($"Current Song: {song.ToString()}");
        }

        public void Stop() {
            _isPlaying = false;
        }

        public void Start() {
            _isPlaying  = true;
        }

        private void WriteToFile(string message) {
            try {
                File.WriteAllText(_saveLocation, message);
            } catch (Exception e) {
                Console.WriteLine($"Error occurred: {e.Message}");
                Environment.Exit(1);
            }
        }

        private bool SongHasChanged(Song NewSong) {
            return _currentSong.Artist != NewSong.Artist
                || _currentSong.Title != NewSong.Title;
        }
    }
}