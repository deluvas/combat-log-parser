#Star Conflict Combat Log Parser

A parser writte in C# for the game's combat.log files. It can either parse the file as a whole or continously listen to the stream for changes, excellent for writing a live log analyzer.

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
- Separate program from library
