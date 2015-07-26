using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace CombatLogParser
{
    public class CombatLogParser : LogParser
    {
        public CombatLogParser( string logFile )
            : this( logFile, StreamParsingType.Default )
        {
        }

        public CombatLogParser( string logFile, StreamParsingType streamType )
            : base( logFile, streamType )
        {
            // List of functions to try/parse the line
            parsers = new Func<string, LogEntry>[] {
                ParseGameSession, ParseGameStart, ParsePlayerSpawn, ParseAuraEvent, ParseSpellEvent, ParseDamageEvent, ParseEntityHealed, 
                ParseMissileDetonation, ParseMissileLaunch, ParsePlayerKilled, ParseGameEnd
            };
        }

        protected GameSessionEntry ParseGameSession( string line )
        {
            if ( !line.Contains( "Connect to game session" ) )
                return null;

            // | ======= Connect to game session 12402065 =======

            var pattern = @"Connect to game session (\d+)";
            var regex = Regex.Match( line, pattern );

            var e = new GameSessionEntry {
                SessionId = regex.Groups[1].Value
            };
            return e;
        }

        protected GameStartEntry ParseGameStart( string line )
        {
            if ( !line.Contains( "Start gameplay" ) )
                return null;

            // | ======= Start gameplay 'TeamDeathMatch' map 's8256_thar_ancientcomplex', local client team 2 =======

            var pattern = @"Start gameplay '([^']+)' map '([^']+)', local client team (\d+)";
            var regex = Regex.Match( line, pattern );

            var e = new GameStartEntry {
                Gamemode = regex.Groups[1].Value,
                MapName = regex.Groups[2].Value,
                TeamId = int.Parse( regex.Groups[3].Value )
            };
            return e;
        }

        protected PlayerSpawnEvent ParsePlayerSpawn( string line )
        {
            if ( !line.Contains( "| Spawn SpaceShip" ) )
                return null;

            // 04:44:39.180  CMBT   | Spawn SpaceShip for player0 (GoblinPoilu, #000EC1CB). 'Ship_Race1_S_T5_Faction1'

            var regex = Regex.Match( line, @"\(([^,]+), #([^)]+)\). '([^']+)'" );
            var obj = new PlayerSpawnEvent {
                PlayerName = regex.Groups[1].Value,
                ShipType = regex.Groups[3].Value
            };
            return obj;
        }

        protected SpellEvent ParseSpellEvent( string line )
        {
            if ( !line.Contains( "| Spell" ) )
                return null;

            // 04:44:39.180  CMBT   | Spell 'MaxSpeedPlus42' by Sandiego(Module_AdaptiveForceField_T5_Epic) targets(1): Sandiego
            // 04:44:39.170  CMBT   | Spell 'SpawnWarpInvulnerability' by BrenoAlberto targets(1): BrenoAlberto
            // Spell 'Spell_Nanobots_Cloud_T5_Epic' by General(Weapon_Nanobots_Cloud_T5_Epic) targets(2): Pestizid Pestizid
            // 02:57:50.140  CMBT   | Spell 'FrigateDrone_T5_Base' by (bot)Sarah(Module_FrigateDrone_T5_Base) targets(2): (bot)Nicholas (bot)Sarah

            var regex = Regex.Match( line, @"Spell '([^']+)' by ([^ ]+) targets\((\d+)\): (.+)" );

            // The targets(n) section can have one or more
            // elements; split these by ' '
            // e.g. targets(2): (bot)Nicholas (bot)Sarah
            var targets = regex.Groups[4].Value
                .Split( ' ' );

            var obj = new SpellEvent {
                PlayerName = regex.Groups[2].Value,
                SpellName = regex.Groups[1].Value,
                TargetNames = targets
            };

            // Sometimes, the PlayerName is accompanied by the module name
            // in brackets eg. (bot)Sarah(Module_FrigateDrone_T5_Base)
            if ( obj.PlayerName.LastIndexOf( ')' ) == obj.PlayerName.Length - 1 ) {
                var playerModuleRegex = Regex.Match( regex.Groups[2].Value, @"^(.+)\((.+)\)$" );
                obj.PlayerName = playerModuleRegex.Groups[1].Value;
            }
            return obj;
        }

        protected AuraEvent ParseAuraEvent( string line )
        {
            if ( !( line.Contains( "| Apply aura" ) || line.Contains( "| Cancel aura" ) ) )
                return null;

            // Apply aura 'CommandArmorResist_T5_Mk1' id 865 type AURA_HULL_RESIST_ALL to '(bot)Alexis'
            // Cancel aura 'MaxSpeedPlus42' id 821 type AURA_RESIST_ALL from 'Catbox'
            // Cancel aura 'SpawnInvulnerabilityFreeSpace' id 11624 type AURA_INVULNERABILITY from ''

            var pattern = @" ([^ ]+) aura '([^']+)' id [^ ]+ type ([^ ]+) (?:to|from) '([^']*)'";
            var regex = Regex.Match( line, pattern );

            var obj = new AuraEvent {
                EventType = (AuraEventType)Enum.Parse( 
                    typeof( AuraEventType ), 
                    regex.Groups[1].Value, 
                    true 
                ),
                AuraName = regex.Groups[2].Value,
                AuraType = regex.Groups[3].Value,
                TargetName = regex.Groups[4].Value,
            };

            return obj;
        }

        protected EntityDamageEvent ParseDamageEvent( string line )
        {
            if ( !line.Contains( "| Damage" ) )
                return null;

            // Damage              n/a ->         Pestizid 139.68 (crash) TRUE_DAMAGE|COLLISION 
            // Damage           Catbox ->          General 601.62 Weapon_Plasmagun_Basic_T5_Rel EMP|PRIMARY_WEAPON|CRIT 
            // Damage      galiver1971 ->         Sandiego 168.68 Weapon_Radiation_Beam_T5_Epic THERMAL 
            // Damage     BrenoAlberto -> Expendable_BasicGuidedDron_T5_Mk3(General) 577.46 Weapon_Plasmagun_Basic_T5_Rel EMP|PRIMARY_WEAPON "

            var regex = Regex.Match( line, @"Damage\s+(\S+) ->\s+(\S+)\s+([\d.,]+) (.*) ([\w\|]+)" );
            var obj = new EntityDamageEvent {
                AttackerName = regex.Groups[1].Value,
                VictimName = regex.Groups[2].Value,
                Damage = float.Parse( regex.Groups[3].Value ),
                WeaponName = regex.Groups[4].Value
            };
            return obj;
        }

        protected PlayerKilledEvent ParsePlayerKilled( string line )
        {
            if ( !line.Contains( "| Killed" ) )
                return null;

            // Killed Sandiego	 Ship_Race2_S_T5_Faction2;	 killer Pestizid 
            // Killed HealBot_Armor(General);	 killer BrenoAlberto 
            // Killed Expendable_BasicGuidedDron_T5_Mk3(General);	 killer BrenoAlberto 

            var regex = Regex.Match( line, @"Killed (\S+)\s+.+?killer (\S+)" );
            var obj = new PlayerKilledEvent {
                VictimName = regex.Groups[1].Value,
                AttackerName = regex.Groups[2].Value
            };
            return obj;
        }

        protected GameEndEvent ParseGameEnd( string line )
        {
            if ( !line.Contains( "| Gameplay finished" ) ) 
                return null;

            // Gameplay finished. Winner team: 1(TDM_TARGET_SCORE_REACHED). Finish reason: 'Max score earned'. Actual game time 577.1 sec

            var regex = Regex.Match( line, @"Gameplay finished\. Winner team: (\d+)\((\S+)\)\. Finish reason: '(.+)'\. Actual game time ([\d.,]+) sec" );
            var obj = new GameEndEvent {
                WinnerTeam = int.Parse( regex.Groups[1].Value ),
                ReasonType = regex.Groups[2].Value,
                Reason = regex.Groups[3].Value,
                TotalTime = float.Parse( regex.Groups[4].Value )
            };
            return obj;
        }

        protected MissileLaunchEvent ParseMissileLaunch( string line )
        {
            if ( !line.Contains( "| Rocket launch" ) )
                return null;

            // Rocket launch. owner 'syndicats88', def 'SpaceMissile_AAMu_T4_Mk3'
            // Rocket launch. owner 'sogoth', def 'SpaceMissile_Volley_T4_Mk3', target 'HWDesigner'

            string pattern = @"Rocket launch. owner '([^']+)', def '([^']+)'(?:, target '([^']+)')?";
            var regex = Regex.Match( line, pattern );

            var obj = new MissileLaunchEvent {
                AttackerName = regex.Groups[1].Value,
                MissileType = regex.Groups[2].Value,
                VictimName = regex.Groups[3].Value
            };

            return obj;
        }

        protected MissileDetonationEvent ParseMissileDetonation( string line )
        {
            if ( !line.Contains( "| Rocket detonation" ) )
                return null;

            // Rocket detonation. owner 'ClanShip_SideRocketTurret_t3', def 'ClanShip_SideRocketTurret_t3', reason 'ttl'
            // Rocket detonation. owner 'ClanShip_SideRocketTurret_t3', def 'ClanShip_SideRocketTurret_t3', reason 'auto_detonate', directHit 'NPC13'
            // Rocket detonation. owner 'ClanShip_SideRocketTurret_t3', def 'ClanShip_SideRocketTurret_t3', reason 'hit', directHit 'Movement_Asteroid6'

            string pattern = @"Rocket detonation. owner '([^']+)', def '([^']+)', " +
                "reason '([^']+)'(?:, directHit '([^']+)')?";

            var regex = Regex.Match( line, pattern );
            var obj = new MissileDetonationEvent {
                OwnerName = regex.Groups[1].Value,
                MissileType = regex.Groups[2].Value,
                Reason = regex.Groups[3].Value, 
                VictimName = regex.Groups[4].Value
            };

            return obj;
        }

        protected EntityHealedEvent ParseEntityHealed( string line )
        {
            if ( !line.Contains( "| Heal" ) )
                return null;

            // Heal        syndicats88 ->      syndicats88 750.75 Module_RepairDrones_s_T4_Mk1
            // Heal                n/a ->              n/a   2.00 ClanShip_Angar_t3

            string pattern = @"Heal\s+([^\s]+) ->\s+([^\s]+)\s+([\d.,]+) ([^\s]+)";

            var regex = Regex.Match( line, pattern );
            var obj = new EntityHealedEvent {
                HealerName = regex.Groups[1].Value,
                TargetName = regex.Groups[2].Value,
                Heal = float.Parse( regex.Groups[3].Value ),
                HealerModule = regex.Groups[4].Value
            };

            return obj;
        }
    }
}
