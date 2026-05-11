public sealed class ChoppableRock : Component, Component.IDamageable
{
	[Property]
	public OreType RockType { get; set; } = OreType.Coal;

	[Property]
	public float MaxHealth { get; set; } = 100f;

	public float Health { get; set; }

	[Property]
	public float RespawnTime { get; set; } = 120f;

	private TimeSince _timeSinceMined;
	private bool _isMined = false;
	private ModelRenderer _renderer;
	private Collider _collider;

	protected override void OnStart()
	{
		Health = MaxHealth;
		_renderer = Components.Get<ModelRenderer>();
		_collider = Components.Get<Collider>();
		
		UpdateColor();
	}

	protected override void OnUpdate()
	{
		if ( _isMined && _timeSinceMined > RespawnTime )
		{
			Respawn();
		}
	}

	private void UpdateColor()
	{
		if ( !_renderer.IsValid() ) return;
		
		switch ( RockType )
		{
			case OreType.Coal:
				_renderer.Tint = Color.Black;
				break;
			case OreType.Gold:
				_renderer.Tint = Color.Yellow;
				break;
			case OreType.Diamond:
				_renderer.Tint = Color.Cyan;
				break;
		}
	}

	public void OnDamage( in DamageInfo damage )
	{
		if ( _isMined || !Networking.IsHost ) return;
		if ( damage.Damage <= 0 ) return;
		
		// Only take damage from PickaxeWeapon (optional: or we can just accept all melee damage)
		if ( damage.Weapon.IsValid() && damage.Weapon.Components.Get<PickaxeWeapon>() == null )
		{
			// Not a pickaxe, deal drastically reduced damage or no damage
			Health -= damage.Damage * 0.1f;
		}
		else
		{
			Health -= damage.Damage;
		}

		if ( Health <= 0 )
		{
			Mine();
		}
	}

	private void Mine()
	{
		if ( _isMined ) return;
		_isMined = true;
		_timeSinceMined = 0;

		if ( _renderer.IsValid() ) _renderer.Enabled = false;
		if ( _collider.IsValid() ) _collider.Enabled = false;

		SpawnOre();
	}

	private void Respawn()
	{
		_isMined = false;
		Health = MaxHealth;

		if ( _renderer.IsValid() ) _renderer.Enabled = true;
		if ( _collider.IsValid() ) _collider.Enabled = true;
	}

	private void SpawnOre()
	{
		if ( !Networking.IsHost ) return;

		var go = new GameObject( true, "raw_ore" );
		go.WorldPosition = WorldPosition + Vector3.Up * 20f;
		
		var rawOre = go.Components.Create<RawOre>();
		rawOre.Type = RockType;
		
		var modelRenderer = go.Components.Create<ModelRenderer>();
		modelRenderer.Model = Model.Load( "models/dev/box.vmdl" ); // Placeholder
		modelRenderer.Tint = _renderer.IsValid() ? _renderer.Tint : Color.White;
		go.WorldScale = new Vector3( 0.3f, 0.3f, 0.3f );

		var collider = go.Components.Create<BoxCollider>();
		var rb = go.Components.Create<Rigidbody>();
		
		go.Tags.Add( "solid" );
		go.Tags.Add( "prop" );
		
		go.NetworkSpawn();
	}
}
