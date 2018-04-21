using gpmdp_rdr.Music;
using gpmdp_rdr.Providers.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Semver;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace gpmdp_rdr.Providers
{
    public class WebsocketApi : IProvider
    {
        #region static
        private static string _connectionUrl = "ws://localhost:5672";

        async public static Task<WebsocketApi> CreateWebsocketApi(Logger Logger) {
            ClientWebSocket client = new ClientWebSocket();

            try {
                // TODO use debug method
                Logger.Debug($"Attempting to connect to {_connectionUrl}");
                await client.ConnectAsync(new Uri(WebsocketApi._connectionUrl), CancellationToken.None);
            } catch (Exception e) {
                Console.WriteLine($"Unable to connect: {e.Message}");
            }

            return new WebsocketApi(client, Logger);
        }
        #endregion

        private ClientWebSocket _serverConnection = null;

        private Player _player;

        private Logger _logger;

        private WebsocketApi(ClientWebSocket connection, Logger Logger) {
            _serverConnection = connection;
            _logger = Logger;
        }

        public bool IsUseable() {
            var useable = _serverConnection.State == WebSocketState.Open;
            _logger.Debug($"Websocket is useable: {useable}");

            return useable;
        }

        // Documentation: https://github.com/MarshallOfSound/Google-Play-Music-Desktop-Player-UNOFFICIAL-/blob/master/docs/PlaybackAPI_WebSocket.md
        public async Task Start(string saveFileName) {
            _logger.Debug("Starting Websocket API Reader");
            _player = new Player(saveFileName);

            while (_serverConnection.State == WebSocketState.Open) {
                var socketMessage = await this.RetrieveMessage();
                JObject message = null;
                using (var sr = new StreamReader(socketMessage)) {
                    message = JObject.Parse(sr.ReadToEnd());
                }

                string channel = message["channel"].ToObject<string>();
                _logger.Debug($"Channel is {channel}");

                if (channel == Channel.API_VERSION.GetDescription()) {
                    string versionNumber = message["payload"].ToObject<string>();
                    this.VersionCompatible(versionNumber);
                    continue;
                }

                if (channel == Channel.PLAY_STATE.GetDescription()) {
                    bool state = message["payload"].ToObject<bool>();
                    this.PlayState(state);
                    continue;
                }

                if (channel == Channel.TRACK.GetDescription()) {
                    Song song = message["payload"].ToObject<Song>();
                    this.NewTrack(song);
                    continue;
                }
            }
        }

        async private Task<MemoryStream> RetrieveMessage() {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);
            WebSocketReceiveResult result = null;

            var stream = new MemoryStream();
            do {
                try {
                    result = await _serverConnection.ReceiveAsync(buffer, CancellationToken.None);
                    stream.Write(buffer.Array, buffer.Offset, result.Count);
                } catch (Exception e) {
                    Console.WriteLine($"Failed to retrieve message: {e.Message}");
                    Program.ExitWith(ExitCode.WEBSOCKET_MESSAGE_RETRIEVAL_FAILED);
                }
            } while (!result.EndOfMessage);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        private void VersionCompatible(string apiVersion) {
            SemVersion apiSemVersion;
            if (!SemVersion.TryParse(apiVersion, out apiSemVersion)) {
                Program.ExitWith(ExitCode.WEBSOCKET_UNABLE_TO_PARSE);
            }

            var minVersion = SemVersion.Parse("1.0.0");
            if (apiSemVersion < minVersion) {
                Program.ExitWith(ExitCode.WEBSOCKET_API_MISMATCH);
            }
        }

        private void PlayState(bool isPlaying) {
            if (isPlaying) { _player.Start(); }
            else { _player.Stop(); }
        }

        private void NewTrack(Song track) {
            _player.Update(track);
        }
    }
}