using Godot;

public partial class LabyrinthCollision : Node2D
{
	public void OnWallEntered(Area2D area)
	{
		if (area.IsInGroup("Player"))
		{
			Game.instance.OnGameOver();
		}
	}

	public void OnGoalEntered(Area2D area)
	{
		if (area.IsInGroup("Player"))
		{
			Game.instance.OnGameWin();
		}
	}
}
