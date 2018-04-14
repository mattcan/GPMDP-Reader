namespace gpmdp_rdr.Music
{
    // TODO add some tests, this should be dummy proof..
    public class Song
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;

        public bool Playing { get; set; } = false;

        public override string ToString() {
            return $"{Title} by {Artist}";
        }

        public bool IsEmpty() {
            return  (Title == string.Empty) &&
                    (Artist == string.Empty) &&
                    (Album == string.Empty);
        }
    }
}