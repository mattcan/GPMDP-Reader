using System;

namespace gpmdp_rdr
{
    public enum ExitCode : int
    {
        NO_PROBLEMO = 0,
        WEBSOCKET_UNABLE_TO_PARSE = 1,
        WEBSOCKET_API_MISMATCH = 2,
        JSON_API_EXCEPTION = 3
    }
}