public class MsgTags {
    public const ushort JoinMessage = 1;
    public const ushort PlayerReady = 10;
    public const ushort PlayerUpdate = 2;
    public const ushort StartGameCountdown = 3;
    public const ushort GameStart = 4;
    public const ushort GameComplete = 5;   // Called when game is successfully completed
    public const ushort GameCanceled = 6;   // Called when active player leaves during gameplay
    public const ushort GameStatsUpdate = 7;

    public const ushort TurretSpawn = 10;
    public const ushort TurretUpdate = 11;
    public const ushort TurretFire = 12;
    public const ushort TurretDespawn = 13;

    public const ushort ProjectileSpawn = 20;
    public const ushort ProjectileUpdate = 21;

    public const ushort EnemyDespawn = 31;
    public const ushort EnemyUpdate = 32;
}
