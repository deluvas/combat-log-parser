using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CombatLogParser
{
    public class PlayerAddedEntry : LogEntry
    {
        public string PlayerName { get; set; }
        public string PlayerCorp { get; set; }
        public string PlayerId { get; set; }
        public int TeamId { get; set; }        
    }

    public class LevelStartedEntry : LogEntry
    {
        public string LevelName { get; set; }
    }

    public class LevelStartingEntry : LevelStartedEntry
    {
        public string Gamemode { get; set; }
    }

}
