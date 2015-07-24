#Star Conflict Combat Log Parser

C# parser for the game's combat.log file.

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
