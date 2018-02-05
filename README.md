# GPMDP Reader

A utility to take the current song from [Google Play Music Desktop Player][1]
and stuff it into a text file of your choosing. The original intent was to
show the current song while streaming using the [OBS][2] Text Source.

[1]: https://www.googleplaymusicdesktopplayer.com
[2]: https://obsproject.com/

*This is an alpha project, many sharp edges. Feel free to leave an issue if you get cut*

## How to use

First, make sure you have the *JSON API* setting enabled in [GPMDP][1].

Next, run this utility from a terminal or command prompt and pass in
the location of the JSON output and where you want to save the song
title.

```sh
# On linux
gpmdp-rdr \
    ~/.config/Google\ Play\ Music\ Desktop\ Player/json_store \
    ~/StreamAssets/current_song.txt
```

If you're on another platform, checkout [this list of locations][3].
Just don't include the **playback.json** file in the path, things
will not work.

[3]: https://github.com/MarshallOfSound/Google-Play-Music-Desktop-Player-UNOFFICIAL-/blob/master/docs/PlaybackAPI.md#playback-information-api

Once things are running, you can use the Text Source in OBS Studio
to read from a file.

To exit the GPMDP Reader, pop over to your terminal and then you can
press <kbd>q</kbd> followed by <kbd>Enter</kbd> or <kbd>Ctrl</kbd>+<kbd>c</kbd>.

## Future

So many little bugs and edge cases I didn't take into account. Might
just look at the Websocket API instead to save myself the headache.