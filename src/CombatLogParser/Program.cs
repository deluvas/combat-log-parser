using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CombatLogParser
{
    public class Program
    {
        static string logsPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) 
            + @"\My Games\StarConflict\logs\";
        const string localLog = "combat.log";
        const string dreadLog = @"dread-combat.log";

        static void Main( string[] args )
        {
            var logDir = new DirectoryInfo( logsPath );
            var latestLogDir = logDir.GetDirectories()
                .OrderByDescending( r => r.LastWriteTime )
                .First();
            var latestCombatLog = latestLogDir.FullName + "/combat.log";

            //--------------//

            var list = new List<LogEntry>();

            var logParser = new CombatLogParser( dreadLog, StreamParsingType.Continous );

            SpellEvent lastTorp = null;

            foreach ( var entry in logParser ) {
                if ( entry is SpellEvent ) {
                    var spell = (SpellEvent)entry;
                    //Console.WriteLine( "{0} {1} -> {2}", spell.PlayerName, spell.SpellName, string.Join( ", ", spell.TargetNames ) );
                    if ( spell.SpellName == "Spell_ClanShipTorpedo" && spell.PlayerName == "n/a" ) {
                        lastTorp = spell;
                        Console.WriteLine( "{3}: {0} {1} -> {2}", spell.PlayerName, spell.SpellName, string.Join( ", ", spell.TargetNames ), spell.Time.ToString( "hh-mm-ss" ) );
                    }
                    

                }

                if ( lastTorp != null ) {
                    var nextTorpTime = lastTorp.Time.AddSeconds( 65 );
                    var diff = entry.Time - lastTorp.Time;
                    var diff2 = nextTorpTime - entry.Time;

                    if ( diff2.TotalSeconds > 5 && diff2.TotalSeconds < 6 ) {
                        Console.WriteLine( "{0} left", diff2.TotalSeconds );
                    }
                }

                //Console.WriteLine( entry.GetType().Name );
                list.Add( entry );
            }

            logParser.Dispose();


            //var groups = list.OfType<EntityDamageEvent>().GroupBy( k => k.AttackerName );
            ////.ToDictionary( s => s.Key, s => s.Sum( r => r.Damage ) ).OrderBy( r => r.Value );
            //foreach ( var group in groups ) {
            //    Console.WriteLine( group.Key );
            //    foreach ( var g in group ) {
            //        Console.WriteLine( "    -> " + g.Damage + " / " + g.VictimName + " .. " + g.WeaponName );
            //    }
            //}
        }
    }
}
