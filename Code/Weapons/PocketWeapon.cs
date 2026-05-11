using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

public sealed class PocketWeapon : BaseCarryable
{
	[Property] public int MaxSlots { get; set; } = 9;

	[Sync] public NetList<GameObject> StoredItems { get; set; } = new();

	protected override bool WantsPrimaryAttack() => Input.Pressed( "attack1" );
	protected override bool WantsSecondaryAttack() => Input.Pressed( "attack2" );

	public override void OnControl( Player player )
	{
		base.OnControl( player );

		if ( WantsPrimaryAttack() )
			PickupItem( player );

		if ( WantsSecondaryAttack() )
			DropItem( player );
	}

	[Rpc.Host]
	private void PickupItem( Player player )
	{
		if ( StoredItems.Count >= MaxSlots )
		{
			Notices.SendNotice( player.Network.Owner, "error", Color.Red, "Poche pleine !", 3 );
			return;
		}

		var tr = Scene.Trace.Ray( player.EyeTransform.ForwardRay, 150f )
			.IgnoreGameObjectHierarchy( player.GameObject )
			.Run();

		if ( tr.Hit && tr.GameObject.IsValid() )
		{
			// Verify if the object can be picked up
			var go = tr.GameObject;
			
			// We only allow picking up objects with the 'prop' tag or items
			if ( go.Tags.Has( "prop" ) || go.Components.Get<BaseCarryable>() != null || go.Components.Get<DroppedWeapon>() != null )
			{
				if ( go.Components.Get<Player>() != null ) return; // Never pickup players
				
				go.Enabled = false;
				go.SetParent( GameObject ); // Parent it to the pocket

				StoredItems.Add( go );
				Notices.SendNotice( player.Network.Owner, "info", Color.Green, $"{go.Name} ajout\u00E9 \u00E0 la poche.", 2 );
			}
		}
	}

	[Rpc.Host]
	private void DropItem( Player player )
	{
		if ( StoredItems.Count == 0 ) return;

		var go = StoredItems[StoredItems.Count - 1];
		StoredItems.RemoveAt( StoredItems.Count - 1 );

		if ( go.IsValid() )
		{
			go.SetParent( null );
			go.Enabled = true;

			var tr = Scene.Trace.Ray( player.EyeTransform.ForwardRay, 80f )
				.IgnoreGameObjectHierarchy( player.GameObject )
				.Run();

			go.WorldPosition = tr.EndPosition;
			
			if ( go.Components.TryGet<Rigidbody>( out var rb ) )
			{
				rb.Velocity = player.EyeTransform.Forward * 200f;
			}
		}
	}

	public override void DrawHud( HudPainter painter, Vector2 crosshair )
	{
		var len = 6;
		painter.SetBlendMode( BlendMode.Lighten );
		painter.DrawCircle( crosshair, len, Color.White );
	}
}
