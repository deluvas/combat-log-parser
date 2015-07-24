using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CombatLogParser
{
    public enum EventType
    {
        GameStart,
        PlayerSpawn
    }

    public enum DamageType
    {
        Emp,
        Thermal,
        Kinetic,
        PrimaryWeapon,
        Critical,
        Explosion,
        TrueDamage,
        Collision
    }

    public enum AuraEventType
    {
        Apply,
        Cancel
    }

    public class GameSessionEntry : LogEntry
    {
        public string SessionId { get; set; }
    }

    public class GameStartEntry : LogEntry
    {
        public string MapName { get; set; }
        public string Gamemode { get; set; }
        public int TeamId { get; set; }
    }

    public class GameEndEvent : LogEntry
    {
        public int WinnerTeam { get; set; }
        public string ReasonType { get; set; }
        public string Reason { get; set; }
        public float TotalTime { get; set; }
    }

    public class PlayerSpawnEvent : LogEntry
    {
        public string PlayerName { get; set; }
        public string ShipType { get; set; }
    }

    public class SpellEvent : LogEntry
    {
        public string SpellName { get; set; }
        public string PlayerName { get; set; }
        public string[] TargetNames { get; set; }
    }

    public class AuraEvent : LogEntry
    {
        public string AuraName { get; set; }
        public string AuraType { get; set; }
        public string TargetName { get; set; }
        public AuraEventType EventType { get; set; }
    }

    public class EntityDamageEvent : LogEntry
    {
        public string AttackerName { get; set; }
        public string VictimName { get; set; }
        public float Damage { get; set; }
        public string WeaponName { get; set; }
        public DamageType DamageType { get; set; }
    }

    public class MissileLaunchEvent : LogEntry
    {
        public string AttackerName { get; set; }
        public string MissileType { get; set; }
        public string VictimName { get; set; }
    }

    public class MissileDetonationEvent : LogEntry
    {
        public string OwnerName { get; set; }
        public string MissileType { get; set; }
        public string Reason { get; set; }
        public string VictimName { get; set; }
    }

    public class PlayerKilledEvent : LogEntry
    {
        public string AttackerName { get; set; }
        public string VictimName { get; set; }
        public string ShipName { get; set; }
    }

    public class EntityHealedEvent : LogEntry
    {
        public string HealerName { get; set; }
        public string TargetName { get; set; }
        public float Heal { get; set; }
        public string HealerModule { get; set; }
    }
}
