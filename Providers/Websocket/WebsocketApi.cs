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
using Timers = System.Timers;

namespace gpmdp_rdr.Providers
{
    /// <summary>
    /// Connects to GPMDP WebsocketAPI
    /// Documentation: https://github.com/MarshallOfSound/Google-Play-Music-Desktop-Player-UNOFFICIAL-/blob/master/docs/PlaybackAPI_WebSocket.md
    /// </summary>
    public class WebsocketApi : IProvider, IDisposable
    {
        #region static
        private static string _connectionUrl = "ws://localhost:5672";

        public async static Task<WebsocketApi> CreateWebsocketApi(Logger Logger) {
            ClientWebSocket client = new ClientWebSocket();

            try {
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
        private Task _readTask;
        private CancellationTokenSource _cancelMaster;

        private WebsocketApi(ClientWebSocket connection, Logger Logger) {
            _serverConnection = connection;
            _logger = Logger;
        }

        public bool IsUseable() {
            var useable = _serverConnection.State == WebSocketState.Open;
            _logger.Debug($"Websocket is useable: {useable}");

            return useable;
        }

        public void Start(string saveFileName) {
            _logger.Debug("Starting Websocket API Reader");
            _player = new Player(saveFileName);

            _cancelMaster = new CancellationTokenSource();

            _readTask = new Task(this.ReadFromSocket, _cancelMaster.Token);
            _readTask.Start();
        }

        private void ReadFromSocket() {
            while(_serverConnection.State == WebSocketState.Open) {
                var socketMessage = this.RetrieveMessage().Result;
                JObject message = null;
                using (var sr = new StreamReader(socketMessage)) {
                    message = JObject.Parse(sr.ReadToEnd());
                }

                string channel = message["channel"].ToObject<string>();
                _logger.Debug($"Channel is {channel}");

                this.PerformAction(channel, message["payload"]);
            }

            _cancelMaster.Cancel();
        }

        private void PerformAction(string channel, JToken payload) {
            if (channel == Channel.API_VERSION.GetDescription()) {
                string versionNumber = payload.ToObject<string>();
                this.VersionCompatible(versionNumber);
                return;
            }

            if (channel == Channel.PLAY_STATE.GetDescription()) {
                bool state = payload.ToObject<bool>();
                this.PlayState(state);
                return;
            }

            if (channel == Channel.TRACK.GetDescription()) {
                Song song = payload.ToObject<Song>();
                this.NewTrack(song);
                return;
            }
        }

        private async Task<MemoryStream> RetrieveMessage() {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);
            WebSocketReceiveResult result = null;

            var stream = new MemoryStream();
            do {
                try {
                    result = await _serverConnection.ReceiveAsync(buffer, CancellationToken.None);
                    stream.Write(buffer.Array, buffer.Offset, result.Count);
                } catch (Exception e) {
                    _logger.Debug($"Failed to retrieve message: {e.Message}");
                    Console.WriteLine("GPMDP has quit, so will I");
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

        public void Dispose()
        {
            _cancelMaster.Cancel();
            _serverConnection
                .CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None)
                .RunSynchronously();
        }
    }
}