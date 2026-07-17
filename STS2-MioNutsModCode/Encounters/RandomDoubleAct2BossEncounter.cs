using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Encounters;

public sealed class RandomDoubleAct2BossEncounter : ModEncounterTemplate
{
    public override RoomType RoomType => RoomType.Boss;
    public override string? CustomRunHistoryIconPath => null;
    public override string? CustomRunHistoryIconOutlinePath => null;

    public override IReadOnlyList<MonsterModel> AllPossibleMonsters => GetAct2BossEncounters()
        .SelectMany(encounter => encounter.AllPossibleMonsters)
        .DistinctBy(monster => monster.Id)
        .ToList();

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var candidates = GetAct2BossEncounters().ToList();
        if (candidates.Count < 2)
            throw new InvalidOperationException("Insufficient Act 2 boss encounters for random double encounter.");

        var firstIndex = Rng.NextInt(candidates.Count);
        var secondIndex = Rng.NextInt(candidates.Count - 1);
        if (secondIndex >= firstIndex)
            secondIndex++;

        return
        [
            (GetEncounterMonster(candidates[firstIndex]), null),
            (GetEncounterMonster(candidates[secondIndex]), null)
        ];
    }

    private static IReadOnlyList<EncounterModel> GetAct2BossEncounters()
    {
        // Hive is the base game's Act 2 (Index = 1).
        return ModelDb.Act<Hive>().AllBossEncounters
            .DistinctBy(encounter => encounter.Id)
            .ToList();
    }

    private static MonsterModel GetEncounterMonster(EncounterModel encounter)
    {
        var monster = encounter.AllPossibleMonsters.FirstOrDefault();
        if (monster is null)
            throw new InvalidOperationException($"Boss encounter '{encounter.Id}' has no available monsters.");

        return monster.ToMutable();
    }
}
