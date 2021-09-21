# q2-dm-stats

q2-dm-stats is a parser to extract statistics from openffa quake2 standard log files.

## Why did I build this?

The idea behind this parser came as a response to the current limitation on the openffa mod for quake2, where you can't see the final score for all participants of a given match (the mod limits to the first 14 players).

As we were running some short tournaments every week at some point on 2021 (yep, tournaments for a 24 years old game), that limitation got in the way of us being able to have a comprehensive view of the overall results and therefore unable to know exactly what was the final results for the tournament.

After looking for alternatives, we were able to identify patterns in the standard quake2 openffa logs that would allow us to parse the text and build the list of players and frags for each match played, which ultimately led to a reliable way to report on the final scores/classification for each tournament.

## Limitations

Unfortunately, the quake2 openffa standard mod doesn't have a clear delimiter to when a match starts (there's no indication in the log for the map begin event).

Given that this parser was used for events where we controlled the maps being played, the most reliable way to overcome that limitation was to use the `gamemap` command, which we were executing via `rcon` to select the next map to be played in every match. That gave us a pattern to look for and identify when the next match begins.

## How to use

I didn't build this parser to be used standalone (because I haven't built distributed applications since around 2009, so I don't know how to do that anymore, and I'm too lazy to try and learn that right now).

That means that in order for you to run the parser, you'll have to run it straight from your IDE (I recommend using Visual Studio Code, which is platform agnostic).

The `Main` method in the `Program` class will be invoked as the entry point of the application and will read the log and create all reports available (feel free to comment the ones you don't need). You'll just have to modify the `logFile` and `reportFolder` constants that the application will use to read from/save to and execute it (in debug or compile mode).

## Dependencies

.net core 5.0