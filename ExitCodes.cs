using System;

namespace gpmdp_rdr
{
    public enum ExitCode : int
    {
        NO_PROBLEMO = 0,
        WEBSOCKET_UNABLE_TO_PARSE = 1,
        WEBSOCKET_API_MISMATCH = 2,
        WEBSOCKET_MESSAGE_RETRIEVAL_FAILED = 6,
        JSON_API_EXCEPTION = 3,
        NO_WORKING_PROVIDER = 4,
        PLAYER_UNABLE_TO_WRITE_FILE = 5
    }
}