using gpmdp_rdr.Music;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Semver;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace gpmdp_rdr.Providers.WebSocket
{
    public class WebsocketApi : IProvider
    {
        #region static
        private static string _connectionUrl = "ws://localhost:5672";

        async public static Task<WebsocketApi> CreateWebsocketApi() {
            ClientWebSocket client = new ClientWebSocket();
            await client.ConnectAsync(new Uri(WebsocketApi._connectionUrl), CancellationToken.None);

            return new WebsocketApi(client);
        }
        #endregion

        private ClientWebSocket _serverConnection = null;

        private Player _player;

        private WebsocketApi(ClientWebSocket connection) {
            _serverConnection = connection;
        }

        public bool IsUseable() {
            return _serverConnection.State == WebSocketState.Open;
        }

        // Documentation: https://github.com/MarshallOfSound/Google-Play-Music-Desktop-Player-UNOFFICIAL-/blob/master/docs/PlaybackAPI_WebSocket.md
        async public void Start(string saveFileName) {
            _player = new Player(saveFileName);

            while (_serverConnection.State == WebSocketState.Open) {
                var socketMessage = await this.RetrieveMessage();
                dynamic message;
                using (var sr = new StreamReader(socketMessage)) {
                    message = JObject.Parse(sr.ReadToEnd());
                }

                if (message.channel == Channel.API_VERSION.GetDescription()) {
                    this.VersionCompatible((string)message.payload);
                    continue;
                }

                if (message.channel == Channel.PLAY_STATE.GetDescription()) {
                    this.PlayState((bool)message.payload);
                    continue;
                }

                if (message.channel == Channel.TRACK.GetDescription()) {
                    this.NewTrack(message.payload);
                    continue;
                }
            }
        }

        async private Task<MemoryStream> RetrieveMessage() {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);
            WebSocketReceiveResult result = null;

            using(var stream = new MemoryStream()) {
                do {
                    result = await _serverConnection.ReceiveAsync(buffer, CancellationToken.None);
                    stream.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            }
        }

        private void VersionCompatible(string apiVersion) {
            SemVersion apiSemVersion;
            if (!SemVersion.TryParse(apiVersion, out apiSemVersion)) {
                Program.ExitWith(ExitCode.WEBSOCKET_UNABLE_TO_PARSE);
            }

            var minVersion = SemVersion.Parse("1.0.0");
            if (apiSemVersion <= minVersion) {
                Program.ExitWith(ExitCode.WEBSOCKET_API_MISMATCH);
            }
        }

        private void PlayState(bool isPlaying) {
            if (isPlaying) { _player.Start(); }
            else { _player.Stop(); }
        }

        private void NewTrack(dynamic track) {
            var song = new Song() {
                Title = track.title,
                Artist = track.artist,
                Album = track.album
            };

            _player.Update(song);
        }
    }
}