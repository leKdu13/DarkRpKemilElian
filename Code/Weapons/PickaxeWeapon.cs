using Sandbox.Citizen;

public sealed class PickaxeWeapon : MeleeWeapon
{
	protected override bool WantsPrimaryAttack() => Input.Pressed( "attack1" );

	public override void OnAdded( Player player )
	{
		base.OnAdded( player );
		
		// Setup reasonable defaults for a pickaxe
		Damage = 25f;
		Range = 80f;
		SwingDelay = 1.0f;
		MissSwingDelay = 1.0f;

		if ( Enum.TryParse<CitizenAnimationHelper.HoldTypes>( "Melee", true, out var melee ) )
		{
			HoldType = melee;
		}
	}
}
