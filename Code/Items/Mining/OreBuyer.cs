using Sandbox.UI;
using System.Linq;

public sealed class OreBuyer : Component, Component.IPressable
{
	[Property]
	public int CoalPrice { get; set; } = 100;
	
	[Property]
	public int GoldPrice { get; set; } = 500;
	
	[Property]
	public int DiamondPrice { get; set; } = 1000;

	[Property]
	public float SellRadius { get; set; } = 150f;

	IPressable.Tooltip? IPressable.GetTooltip( IPressable.Event e )
	{
		return new IPressable.Tooltip( "Vendre Minerais", "$", "Vendez vos minerais raffin\u00E9s ici." );
	}

	bool IPressable.CanPress( IPressable.Event e ) => true;

	bool IPressable.Press( IPressable.Event e )
	{
		SellOres( e.Source.GameObject );
		return true;
	}

	[Rpc.Host]
	private void SellOres( GameObject presserObject )
	{
		if ( !presserObject.IsValid() ) return;

		var player = presserObject.Root.GetComponent<Player>();
		if ( !player.IsValid() ) return;

		// Find all FinishedOre components within SellRadius
		var ores = Scene.GetAllComponents<FinishedOre>()
			.Where( x => x.IsValid() && x.WorldPosition.Distance( WorldPosition ) <= SellRadius )
			.ToList();

		if ( ores.Count == 0 )
		{
			if ( player.Network.Owner is { } emptyOwner )
			{
				Notices.SendNotice( emptyOwner, "info", Color.Orange, "Aucun minerai \u00E0 vendre \u00E0 proximit\u00E9.", 3 );
			}
			return;
		}

		int totalEarned = 0;

		foreach ( var ore in ores )
		{
			switch ( ore.Type )
			{
				case OreType.Coal: totalEarned += CoalPrice; break;
				case OreType.Gold: totalEarned += GoldPrice; break;
				case OreType.Diamond: totalEarned += DiamondPrice; break;
			}
			ore.GameObject.Destroy();
		}

		player.GiveMoney( totalEarned );

		if ( player.Network.Owner is { } owner )
		{
			Notices.SendNotice( owner, "$", Color.Green, $"Vous avez vendu vos minerais pour ${totalEarned:n0} !", 3 );
		}

		if ( Application.IsDedicatedServer == false )
		{
			Sound.Play( "sounds/ui/ui.spawn.sound", WorldPosition );
		}
	}
}
