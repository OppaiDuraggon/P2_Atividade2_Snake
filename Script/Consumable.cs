using Godot;

public partial class Consumable : Sprite2D
{
	private Area2D areaReference;

	public override void _Ready()
	{
		areaReference = GetNode<Area2D>("Consumable_Area");
	}

	public void OnConsumableEntered(Area2D area)
	{
		if (area.IsInGroup("Player"))
		{
			Game.instance.OnConsumableEaten(this);
			QueueFree();
		}
	}

	public bool CanSpawnAt(Vector2 position)
	{
		areaReference.Position = position;

		if (areaReference.HasOverlappingAreas()
			|| position.X <= 0 || position.X >= 1152
			|| position.Y <= 0 || position.Y >= 648)
		{
			areaReference.Position = this.Position;

			return false;
		}

		areaReference.Position = this.Position;

		return true;
	}
}
