#Star Conflict Log Parser

A library written in C# for parsing the game's combat.log and game.log files. It can either parse the file as a whole or continously listen to the stream for changes, excellent for writing a live log analyzer.

**Note:** The GameLogParser class can only parse a few types of log entries.

##Usage
````C#
var logParser = new CombatLogParser( "combat.log" );

foreach( LogEntry entry in logParser ) {
  // do something with the parsed entry
  if ( entry is SpellEvent ) {
    var spell = (SpellEvent)entry;
    Console.WriteLine( spell.PlayerName );
  }
}

logParser.Dispose();
````
##TODO

- Some tests (I'm lazy)
- Polish damage entry parsing
