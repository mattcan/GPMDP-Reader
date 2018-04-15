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

        async public static Task<WebsocketApi> CreateWebsocketApi() {
            ClientWebSocket client = new ClientWebSocket();

            try {
                // TODO use debug method
                Console.WriteLine($"Attempting to connect to {_connectionUrl}");
                await client.ConnectAsync(new Uri(WebsocketApi._connectionUrl), CancellationToken.None);
            } catch (Exception e) {
                // TODO use debug method
                Console.WriteLine($"Unable to connect: {e.Message}");
            }

            return new WebsocketApi(client);
        }
        #endregion

        private ClientWebSocket _serverConnection = null;

        private Player _player;

        private WebsocketApi(ClientWebSocket connection) {
            _serverConnection = connection;
        }

        public bool IsUseable() {
            var useable = _serverConnection.State == WebSocketState.Open;
            // TODO use debug method
            Console.WriteLine($"Websocket is useable: {useable}");
            return useable;
        }

        // Documentation: https://github.com/MarshallOfSound/Google-Play-Music-Desktop-Player-UNOFFICIAL-/blob/master/docs/PlaybackAPI_WebSocket.md
        async public void Start(string saveFileName) {
            _player = new Player(saveFileName);

            while (_serverConnection.State == WebSocketState.Open) {
                var socketMessage = await this.RetrieveMessage();
                dynamic message = null;
                using (var sr = new StreamReader(socketMessage)) {
                    message = JObject.Parse(sr.ReadToEnd());
                }

                // TODO Add a logger with debug command
                // Console.WriteLine($"Channel is {(string)message.channel}");

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