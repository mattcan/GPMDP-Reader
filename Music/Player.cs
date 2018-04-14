using System;
using System.IO;

namespace gpmdp_rdr.Music
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
            if (!SongHasChanged(song)) { return; }

            _currentSong = song;

            Console.WriteLine($"Song playing: {song.Playing}");
            if (song.IsEmpty() || !song.Playing) {
                WriteToFile("Music has stopped");
                return;
            }

            // Ultimately this is the update so, need to make
            // sure we get this far..
            WriteToFile($"Now Playing: {song.ToString()}");
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
            return _currentSong.Playing != NewSong.Playing;
            /*
                || _currentSong.Artist != NewSong.Artist
                || _currentSong.Title != NewSong.Title;
            /**/
        }
    }
}