using System;
using System.IO;

namespace gpmdp_rdr
{
    public class Player
    {
        private Song _currentSong;
        private string _saveLocation;

        public Player(String writeToFilePath) {
            _saveLocation = writeToFilePath;
            _currentSong = new Song();
        }

        public void Update(Song song) {
            if (_currentSong.Title == song.Title && _currentSong.Artist == song.Artist) {
                return;
            }
            _currentSong = song;

            try {
                File.WriteAllText(_saveLocation, $"Now playing: {song.ToString()}");
            } catch (Exception e) {
                Console.WriteLine($"Error occurred: {e.Message}");
                Environment.Exit(1);
            }
        }
    }
}