public sealed class OreRefinery : Component, Component.ITriggerListener
{
	[Property]
	public float ProcessingTime { get; set; } = 3f;

	private bool _isProcessing = false;

	public async void OnTriggerEnter( Collider other )
	{
		if ( !Networking.IsHost || _isProcessing ) return;

		var rawOre = other.GameObject.Components.Get<RawOre>();
		if ( rawOre.IsValid() )
		{
			_isProcessing = true;
			var oreType = rawOre.Type;
			
			// Play sound or effect here if wanted
			if ( Application.IsDedicatedServer == false )
			{
				Sound.Play( "sounds/ui/ui.button.press.sound", WorldPosition );
			}

			// Destroy the raw ore
			rawOre.GameObject.Destroy();

			// Wait for processing
			await global::System.Threading.Tasks.Task.Delay( (int)(ProcessingTime * 1000) );

			if ( !GameObject.IsValid() ) return;

			// Spawn finished ore
			SpawnFinishedOre( oreType );
			_isProcessing = false;
		}
	}

	public void OnTriggerExit( Collider other ) { }

	private void SpawnFinishedOre( OreType type )
	{
		var go = new GameObject( true, "finished_ore" );
		go.WorldPosition = WorldPosition + Vector3.Up * 30f + WorldRotation.Forward * 30f;
		
		var finishedOre = go.Components.Create<FinishedOre>();
		finishedOre.Type = type;
		
		var modelRenderer = go.Components.Create<ModelRenderer>();
		modelRenderer.Model = Model.Load( "models/dev/box.vmdl" ); // Placeholder
		
		switch ( type )
		{
			case OreType.Coal: modelRenderer.Tint = Color.Black; break;
			case OreType.Gold: modelRenderer.Tint = Color.Yellow; break;
			case OreType.Diamond: modelRenderer.Tint = Color.Cyan; break;
		}
		
		go.WorldScale = new Vector3( 0.2f, 0.2f, 0.2f );

		go.Components.Create<BoxCollider>();
		go.Components.Create<Rigidbody>();
		
		go.Tags.Add( "solid" );
		go.Tags.Add( "prop" );
		
		go.NetworkSpawn();
	}
}
