using gpmdp_rdr.Music;

namespace gpmdp_rdr.Providers
{
    public class WebsocketApi : IProvider
    {
        private object _serverConnection = null;

        private Player _player;

        public WebsocketApi() {
            // try to create a connection to GPMDP websocket server
            // retry a few times
            // save connection
            // handle exceptions appropriately
        }

        public bool IsUseable() {
            return _serverConnection != null;
        }

        // Documentation: https://github.com/MarshallOfSound/Google-Play-Music-Desktop-Player-UNOFFICIAL-/blob/master/docs/PlaybackAPI_WebSocket.md
        public void Start() {
            // Setup player
            // parse incoming messages for these channels
            //  * api version
            //  * play state
            //  * track
            throw new System.NotImplementedException();
        }

        private bool VersionCompatible(string apiVersion) { return false; }
    }
}