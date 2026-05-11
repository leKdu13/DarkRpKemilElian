public sealed class RawOre : Component
{
	[Property, Sync]
	public OreType Type { get; set; } = OreType.Coal;

	protected override void OnStart()
	{
		base.OnStart();
		
		var renderer = Components.Get<ModelRenderer>();
		if ( renderer.IsValid() )
		{
			switch ( Type )
			{
				case OreType.Coal: renderer.Tint = Color.Black; break;
				case OreType.Gold: renderer.Tint = Color.Yellow; break;
				case OreType.Diamond: renderer.Tint = Color.Cyan; break;
			}
		}
	}
}
