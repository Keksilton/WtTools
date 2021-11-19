namespace WtTools.Web.Data.Wrpl
{

    public class ReplaySettings
    {
        public string Level { get; set; }
        public string Type { get; set; }
        public string Environment { get; set; }
        public string Weather { get; set; }
        public string LocName { get; set; }
        public string LocDesc { get; set; }
        public int ScoreLimit { get; set; }
        public int TimeLimit { get; set; }
        public float DeathPenaltyMul { get; set; }
        public string Postfix { get; set; }
        public bool UseAlternativeMapCoord { get; set; }
        public bool AllowedKillStreaks { get; set; }
        public bool RandomSpawnTeams { get; set; }
        public bool RemapAiTankModels { get; set; }
        public string BattleAreaColorPreset { get; set; }
        public string Country_axis { get; set; }
        public string Country_allies { get; set; }
        public string RestoreType { get; set; }
        public bool OptionalTakeOff { get; set; }
        public bool ShowTacticalMapCellSize { get; set; }
        public AllowedUnitTypes AllowedUnitTypes { get; set; }
        public Mission[] Mission { get; set; }
        public Stars Stars { get; set; }
    }
}
