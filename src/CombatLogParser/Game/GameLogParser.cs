using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace CombatLogParser
{
    public class GameLogParser : LogParser
    {
        public GameLogParser( string logFile )
            : this( logFile, StreamParsingType.Default )
        {
        }

        public GameLogParser( string logFile, StreamParsingType streamType )
            : base( logFile, streamType )
        {
            // List of functions to try/parse the line
            parsers = new Func<string, LogEntry>[] {
                ParseAddPlayer, ParseLevelStarting, ParseLevelStarted
            };
        }

        protected PlayerAddedEntry ParseAddPlayer( string line )
        {
            if ( line.Length < textMarkerSize || 
                line.IndexOf( "client: ADD_PLAYER", textMarkerSize ) < 0 )
                return null;

            // client: ADD_PLAYER 4 (LocoPadre [SALT], 00072A26) status 4 team 1
            // client: ADD_PLAYER 11 (Gizmomac [], 00042896) status 4 team 2
            // client: ADD_PLAYER 5 (NaJeep, 00028727) status 4 team 2

            var pattern = @"ADD_PLAYER (\d+) \(([^ ]+)(?: \[([^\]]*)\])?, ([^\)]+)\) status (\d+) team (\d+)";
            var regex = Regex.Match( line, pattern );

            var e = new PlayerAddedEntry {
                PlayerName = regex.Groups[2].Value,
                PlayerCorp = regex.Groups[3].Value,
                PlayerId = regex.Groups[4].Value,
                TeamId = int.Parse( regex.Groups[6].Value )
            };
            return e;
        }

        protected LevelStartedEntry ParseLevelStarted( string line )
        {
            if ( line.Length < textMarkerSize ||
                line.IndexOf( "====== level started:", textMarkerSize ) < 0 )
                return null;

            // ====== level started:  'levels\area2\s1340_thar_aliendebris13' success ======

            var pattern = @"'([^']+)' success";
            var regex = Regex.Match( line, pattern );

            var e = new LevelStartedEntry {
                LevelName = regex.Groups[1].Value
            };
            return e;
        } 

        protected LevelStartingEntry ParseLevelStarting( string line )
        {
            if ( line.Length < textMarkerSize ||
                line.IndexOf( "====== starting level:", textMarkerSize ) < 0 )
                return null;

            // ====== starting level: 'levels\area2\s1340_thar_aliendebris13' KingOfTheHill client ======
            // ====== starting level: 'levels/mainmenu/mainmenu'  ======

            var pattern = @"starting level: '([^']+)'(?: ([^ ]+) client)?";
            var regex = Regex.Match( line, pattern );

            var e = new LevelStartingEntry {
                Gamemode = regex.Groups[2].Value,
                LevelName = regex.Groups[1].Value
            };
            return e;
        }    
    }
}
