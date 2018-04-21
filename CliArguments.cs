using EntryPoint;

namespace gpmdp_rdr
{
    public class CliArguments : BaseCliArguments
    {
        public CliArguments() : base("GPMDP-Reader") {}

        [Option(LongName: "debug", ShortName: 'd')]
        [Help("Enable printing debug information to the console")]
        public bool DebugMode { get; set; }

        [Required, Operand(Position: 1)]
        [Help("Where the GPMDP JSON API file is stored. Should point to a directory. A list is available here: https://github.com/MarshallOfSound/Google-Play-Music-Desktop-Player-UNOFFICIAL-/blob/master/docs/PlaybackAPI.md#playback-information-api")]
        public string JsonApiPath { get; set; }

        [Required, Operand(Position: 2)]
        [Help("Where you would like to store the song name and artist. Should point to a file.")]
        public string SavePath { get; set; }
    }
}