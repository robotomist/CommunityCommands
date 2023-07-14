using CommunityCommands.Commands.Converters;
using VampireCommandFramework;
using ProjectM;

namespace CommunityCommands.Commands;

// 1557814269 = bone ring guid
public static class GivePlayerItemCommand
{
	[Command("giveplayer", description: "gives [player] [item]", adminOnly: true)]
	public static void GiveOther(ChatCommandContext ctx, FoundPlayer player, int guid)
	{
		var prefab = new PrefabGUID(guid);
		var playerEntity = player.Value.CharEntity;

		Helper.AddItemToInventory(playerEntity, prefab, 1);
	}
}
