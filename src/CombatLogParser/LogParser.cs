using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace CombatLogParser
{
    public abstract class LogParser : IEnumerable<LogEntry>, IDisposable
    {
        protected string logFile;
        protected StreamReader stream;
        protected StreamParsingType readType;
        protected Func<string, LogEntry>[] parsers;
        protected const byte timestampSize = 12;
        protected const byte textMarkerSize = 22;

        protected LogParser()
        {
        }

        public LogParser( string logFile )
            : this( logFile, StreamParsingType.Default )
        {
        }

        public LogParser( string logFile, StreamParsingType streamType )
        {
            this.logFile = logFile;
            this.readType = streamType;

            // Using FileStream as wrapper for StreamReader to open for reading
            var fileStream = new FileStream( logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            stream = new StreamReader( fileStream );
        }

        public IEnumerator<LogEntry> GetEnumerator()
        {
            switch ( readType ) {
                case StreamParsingType.Default:
                    //
                    // Default/one time parsing of the whole Stream
                    //
                    while ( !stream.EndOfStream ) {
                        var entry = ParseLine();
                        if ( entry != null )
                            yield return entry;
                    }
                    break;
                case StreamParsingType.Continous:
                    //
                    // Continous polling the Stream
                    //
                    while ( true ) {
                        // Keep polling the Stream
                        // XXX: is this even optimal?
                        if ( stream.EndOfStream ) {
                            System.Threading.Thread.Sleep( 500 );
                            continue;
                        }
                        var entry = ParseLine();
                        if ( entry != null )
                            yield return entry;
                    }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected LogEntry ParseLine()
        {
            var line = stream.ReadLine();
            LogEntry entry = null;

            // Try parse timestamp
            var timeEntry = ParseTimeEntry( line );

            // Invoking the functions
            foreach ( var parser in parsers ) {
                entry = parser( line );
                if ( entry != null ) {
                    // Update the time with the parsed timestamp
                    entry.Time = timeEntry.Time;
                    return entry;
                }
            }

            if ( entry == null ) {
                // houston problem!
                // invoke ParseError
                return null;
            }

            return null;
        }

        protected LogEntry ParseTimeEntry( string line )
        {
            // 07:09:14.925  CMBT   | 

            if ( line.Length < timestampSize ) {
                return null;
            }
            // Grab the first <timestampSize> bytes
            // They are at the beginning of the lines
            // Example: 07:09:14.925  CMBT
            string time = line.Substring( 0, timestampSize );
            DateTime parsed;

            // Try parsing the timestamp
            if ( !DateTime.TryParse( time, out parsed ) ) {
                return null;
            }
            var obj = new LogEntry {
                Time = parsed
            };
            return obj;
        }

        public void Dispose()
        {
            stream.Close();
        }
    }

    public enum StreamParsingType
    {
        Default = 0,
        Continous
    }
}
