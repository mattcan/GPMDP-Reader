# GPMDP Reader

A utility to take the current song from [Google Play Music Desktop Player][1]
and stuff it into a text file of your choosing. The original intent was to
show the current song while streaming using the [OBS][2] [Text Source][3].

[1]: https://www.googleplaymusicdesktopplayer.com
[2]: https://obsproject.com/
[3]: https://www.reddit.com/r/Twitch/comments/4k6jpv/obs_studio_text_source_from_file/d3ckpqd/

*This is an alpha project, many sharp edges. Feel free to leave an issue if you get cut*

## How to use

### Enable an API

GPMDP offers two different APIs: JSON and Playback.

The Playback API is more stable but involves GPMDP starting up a web server, which
can be resource intensive for some and a security concern. Playback API also allows
other applications to control your music. This application does not do that.

If you start both APIs, I'm not sure why you would, this application will prefer
the Playback API.

### Start application

Next, run this utility from a terminal or command prompt and pass in
the location of the JSON output and where you want to save the song
title. Since there isn't a packaged build yet, you will need to use
`dotnet run`.

```sh
git clone https://github.com/mattcan/GPMDP-Reader.git
cd GPMDP-Reader
dotnet run \
    ~/.config/Google\ Play\ Music\ Desktop\ Player/json_store \
    ~/StreamAssets/current_song.txt
```

If you need to find out where the JSON API is writing to, checkout
[the official unofficial documentation][4]. Just don't include the **playback.json**
file in the path, things will not work.

[4]: https://github.com/MarshallOfSound/Google-Play-Music-Desktop-Player-UNOFFICIAL-/blob/master/docs/PlaybackAPI.md#playback-information-api

### Using with OBS

Once things are running, you can use the [Text Source in OBS Studio][3]
to read from a file.


### Stop it

To exit the GPMDP Reader, pop over to your terminal and then you can
press <kbd>q</kbd> followed by <kbd>Enter</kbd> or press the combination of <kbd>Ctrl</kbd>+<kbd>c</kbd>.
