
using CommunityCommands.Commands.Converters;
using VampireCommandFramework;
using ProjectM;

namespace CommunityCommands.Commands;

public static class GivePlayerKitCommand
{
	[Command("kit", description: "give kit to player calling", adminOnly: true)]
	public static void GiveKit(ChatCommandContext ctx, string kitName)
	{
		if (Plugin.availableKits.ContainsKey(kitName))
		{
			var kit = Plugin.availableKits[kitName];
			foreach (var itemGuid in kit)
			{
				var prefab = new PrefabGUID(itemGuid);
				Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, prefab, 1);
			}
		}
		else
		{
			ctx.Reply($"kit {kitName} not found");
		}
	}

	[Command("givekit", description: "give kit to a player", adminOnly: true)]
	public static void GiveKitToPlayer(ChatCommandContext ctx, FoundPlayer player, string kitName)
	{
		if (player == null)
		{
			ctx.Reply("player not found");
			return;
		}
		if (Plugin.availableKits.ContainsKey(kitName))
		{
			var kit = Plugin.availableKits[kitName];
			foreach (var itemGuid in kit)
			{
				GivePlayerItemCommand.GiveOther(ctx, player, itemGuid);
			}
			ctx.Reply($"giving player {player.Value.CharacterName} {kitName}");
		}
		else
		{
			ctx.Reply($"kit {kitName} not found");
		}
	}
}
