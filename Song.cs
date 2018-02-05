namespace gpmdp_rdr
{
    public class Song
    {
    /*
    "song": {
        "title": "Skatin Through the City",
        "artist": "MURS",
        "album": "Have a Nice Life",
        "albumArt": "https://lh3.googleusercontent.com/O2n6HpjbGk0uvxqumtxTq3t_e11QB80j0aDMZgvpCZsgc0eWKfPFWfRH1_Qyj8g1D2mwaBzp"
    },
    */

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }

        public override string ToString() {
            return $"{Title} by {Artist}";
        }
    }
}