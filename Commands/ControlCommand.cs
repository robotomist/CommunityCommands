using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityCommands.Commands.Converters;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Entities;
using VampireCommandFramework;

using Bloodstone.API;
using Unity.Collections;
using Unity.Transforms;
using CommunityCommands.Models;

namespace CommunityCommands.Commands;
internal class ControlCommands
{
	public static int z = 0;
	public static PlayerData savedPlayer;

	//Command gets entety closest to curor and prints the name
	[Command("m", adminOnly: true)]
	public void CloseToMouse(ChatCommandContext ctx){

	} 

	[Command("z", adminOnly: true)]
	public void TReplyCommand(ChatCommandContext ctx){
		z = z + 1;
		ctx.Reply($"z: {z}");
		ctx.Reply($"savedPlayer: {savedPlayer.CharacterName}");
		ctx.Reply($"savedPlayer: {savedPlayer.SteamID}");
		ctx.Reply($"savedPlayer: {savedPlayer.IsOnline}");
		ctx.Reply($"savedPlayer: {savedPlayer.UserEntity}");
		ctx.Reply($"savedPlayer: {savedPlayer.CharEntity}");

	}

	[Command("r", adminOnly: true)]
	public void ResetCharacterCommand(ChatCommandContext ctx){
		// playerChar.User = ctx.Event.SenderUserEntity;
		// playerChar.Character = ctx.Event.SenderCharacterEntity;

		FromCharacter fromCharacter = default(FromCharacter);
		fromCharacter.User = savedPlayer.UserEntity;
		fromCharacter.Character =  savedPlayer.CharEntity;

		ControlDebugEvent controlDebugEvent = default(ControlDebugEvent);
		ControlDebugEvent clientEvent = controlDebugEvent;

		// controlDebugEvent.EntityTarget = ctx.Event.SenderUserEntity;
		controlDebugEvent.EntityTarget = savedPlayer.UserEntity;
		Core.Server.GetExistingSystem<DebugEventsSystem>().JumpToNextBloodMoon();
			Core.Server.GetExistingSystem<DebugEventsSystem>().ControlUnit(fromCharacter, clientEvent);
		ctx.Reply("reset");
	}
	
	// 
	[Command("t", "control", adminOnly: true)]
	public void ControlCommand(ChatCommandContext ctx, string name, bool controlPlayer = false)
	{
		// make sure the user is an admin:
		if (!ctx.IsAdmin)
		{
			ctx.Reply("You must be an admin to use this command.");
			return;
		}

		string playerName = ctx.Name;
		Entity senderUserEntity = ctx.Event.SenderUserEntity;
		Entity playerCharacter = ctx.Event.SenderCharacterEntity;

		FromCharacter fromCharacter = default(FromCharacter);
		fromCharacter.User = senderUserEntity;
		fromCharacter.Character = playerCharacter;

		// save playr before attaching to mob
		// DO WE NEED THE STEAM ID???? setting it to 999 for now
		savedPlayer = new PlayerData(playerName, 999, true, senderUserEntity, playerCharacter);	
		
		FromCharacter fromCharacter2 = fromCharacter;

		var existingSystem = Core.Server.GetExistingSystem<DebugEventsSystem>();

		var characterPos = Core.EntityManager.GetComponentData<LocalToWorld>(playerCharacter).Position;

		var q = new EntityQueryDesc()
		{
			All = new[]
					{
				ComponentType.ReadOnly<LocalToWorld>(),
				ComponentType.ReadOnly<Team>()
			},
			None = new[] { ComponentType.ReadOnly<Dead>(), ComponentType.ReadOnly<DestroyTag>() }
		};

		var mobQuery = Core.EntityManager.CreateEntityQuery(q);

		var mobs = mobQuery.ToEntityArray(Allocator.Temp);
		// ctx.Reply($"mobs count: {mobs.Length}");
		var results = new List<Entity>();


		var prefabCollectionSystem = Core.Server.GetExistingSystem<PrefabCollectionSystem>();


		// string name = deerId.ToString(); 
		var wantedGuid = !name.Equals("None") ? prefabCollectionSystem.NameToPrefabGuidDictionary[name] : new PrefabGUID();

		var getGuid = Core.EntityManager.GetComponentDataFromEntity<PrefabGUID>();

		int range = 100;

		// go through mobs and if they are deer, add them to results
		foreach (var mob in mobs)
		{
			var mobPos = Core.EntityManager.GetComponentData<LocalToWorld>(mob).Position;
            var distance = UnityEngine.Vector3.Distance(characterPos, mobPos); 
			
			var mobGUID = getGuid[mob];
			if (distance < range && mobGUID == wantedGuid)
			{
				ctx.Reply($"found mob: {mobGUID}");
				results.Add(mob);
			}	
		}
		ctx.Reply($"results count: {results.Count}");

		if (results.Count <= 0)
		{
			ctx.Reply($"no {name} found");
			return;
		}
		else if (!controlPlayer && results.Count > 1)
		{
			ctx.Reply($"multiple results found for {name}: {results.Count}");
		}
		else 
		{
			var firstMob = controlPlayer && results.Count>1 ? results[1] : results[0];
			ctx.Reply($"found {results.Count} {name}");
			ControlDebugEvent controlDebugEvent = default(ControlDebugEvent);
			controlDebugEvent.EntityTarget = firstMob;
			ControlDebugEvent clientEvent = controlDebugEvent;
			Core.Server.GetExistingSystem<DebugEventsSystem>().ControlUnit(fromCharacter2, clientEvent);
		}


		ctx.Reply("no hover point found");
	}
	
	[Command("cp", adminOnly: true)]
	public void ControlPlayer(ChatCommandContext ctx)
	{
		ControlCommand(ctx, "CHAR_VampireMale", true);
	}
}

