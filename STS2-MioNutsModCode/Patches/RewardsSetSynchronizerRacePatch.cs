using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.Runs;

namespace STS2_MioNutsMod.STS2_MioNutsModCode.Patches;

[HarmonyPatch(typeof(RewardsSetSynchronizer))]
public static class RewardsSetSynchronizerRacePatch
{
    private static readonly AccessTools.FieldRef<RewardsSetSynchronizer, IPlayerCollection> PlayerCollectionRef =
        AccessTools.FieldRefAccess<RewardsSetSynchronizer, IPlayerCollection>("_playerCollection");

    [HarmonyPatch(nameof(RewardsSetSynchronizer.HandleRewardSelectedMessage))]
    [HarmonyPrefix]
    private static bool HandleRewardSelectedMessagePrefix(
        RewardsSetSynchronizer __instance,
        RewardSelectedMessage message,
        ulong senderId)
    {
        return ShouldHandle(__instance, senderId, message.setId);
    }

    [HarmonyPatch(nameof(RewardsSetSynchronizer.HandleRewardSetSkippedMessage))]
    [HarmonyPrefix]
    private static bool HandleRewardSetSkippedMessagePrefix(
        RewardsSetSynchronizer __instance,
        RewardSetSkippedMessage message,
        ulong senderId)
    {
        return ShouldHandle(__instance, senderId, message.setId);
    }

    private static bool ShouldHandle(RewardsSetSynchronizer synchronizer, ulong senderId, int setId)
    {
        try
        {
            var playerCollection = PlayerCollectionRef(synchronizer);
            var player = playerCollection.GetPlayer(senderId);
            if (player is null)
                return true;
            return !synchronizer.IsRewardsSetCompleted(player, setId);
        }
        catch
        {
            return true;
        }
    }
}
