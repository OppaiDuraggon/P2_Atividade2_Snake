using Godot;
using System.Collections.Generic;

public enum Direction { LEFT, RIGHT, UP, DOWN };

public partial class SnakeController : Sprite2D
{
	public bool HasEatenConsumable = false;

	private double timeElapsed = 0;
	private float startingTickRate = 0.5f;
	private float minimumTickRate = 0.1f;
	private float tickRate;

	private const int SNAKE_RECT_SIZE = 36;
	private Direction direction;
	private List<Rect2> snakeBodyList;
	private Area2D snakeHeadArea;
	private bool hasHitItself = false;
	private bool canReceiveInput = true;

	// Called when the node is added to the scene.
	public override void _Ready()
	{
		direction = Direction.RIGHT;
		tickRate = startingTickRate;

		snakeBodyList = new List<Rect2>
			{
				new Rect2(40, 0, 40, 40),
				new Rect2(0, 0, 40, 40)
			};

		snakeHeadArea = GetNode<Area2D>("SnakeHead");
	}

	// Called every frame, delta is the elapsed time since the last frame.
	public override void _Process(double delta)
	{
		if (Game.CurrentState != GameState.PLAYING) { return; }

		timeElapsed += delta;
		if (timeElapsed >= tickRate)
		{
			Vector2 translation;
			switch (direction)
			{
				case Direction.LEFT: translation = new Vector2(-40, 0); break;
				case Direction.RIGHT: translation = new Vector2(40, 0); break;
				case Direction.UP: translation = new Vector2(0, -40); break;
				case Direction.DOWN: translation = new Vector2(0, 40); break;
				default: translation = new Vector2(40, 0); break;
			}

			UpdateHeadPosition(translation);

			if (!HasEatenConsumable)
			{
				//Removes the tail(last body part) from the list to Draw, so as to keep it's size
				snakeBodyList.RemoveAt(snakeBodyList.Count - 1);
			}
			else
			{
				tickRate = Mathf.Clamp(tickRate - 0.1f, minimumTickRate, startingTickRate);
				GD.Print($"Current TICK RATE: {tickRate}");
			}

			if (HasHitItself())
			{
				Game.instance.OnGameOver();
			}

			if (!hasHitItself)
			{
				QueueRedraw();
			}
			snakeHeadArea.Position = snakeBodyList[0].Position;
			HasEatenConsumable = false;
			timeElapsed = 0;
		}
	}

	// Draw the snake on the screen.
	public override void _Draw()
	{
		DrawRect(new Rect2(snakeBodyList[0].Position.X - SNAKE_RECT_SIZE / 2, snakeBodyList[0].Position.Y - SNAKE_RECT_SIZE / 2, SNAKE_RECT_SIZE, SNAKE_RECT_SIZE), new Color(0, 0.7f, 0));

		var bodyColor = new Color(0, 1f, 0);
		for (int i = 1; i < snakeBodyList.Count; i++)
		{
			DrawRect(new Rect2(snakeBodyList[i].Position.X - SNAKE_RECT_SIZE / 2, snakeBodyList[i].Position.Y - SNAKE_RECT_SIZE / 2, SNAKE_RECT_SIZE, SNAKE_RECT_SIZE), bodyColor);
		}
		canReceiveInput = true;
	}

	// Handles input events for changing the snake's direction
	public override void _Input(InputEvent @event)
	{
		if (Game.CurrentState != GameState.PLAYING || !canReceiveInput) { return; }

		if (@event.IsAction("ui_left") && direction != Direction.RIGHT)
		{
			direction = Direction.LEFT;
			canReceiveInput = false;
			return;
		}
		if (@event.IsAction("ui_right") && direction != Direction.LEFT)
		{
			direction = Direction.RIGHT;
			canReceiveInput = false;
			return;
		}
		if (@event.IsAction("ui_up") && direction != Direction.DOWN)
		{
			direction = Direction.UP;
			canReceiveInput = false;
			return;
		}
		if (@event.IsAction("ui_down") && direction != Direction.UP)
		{
			direction = Direction.DOWN;
			canReceiveInput = false;
			return;
		}
	}

	public void ResetController()
	{
		direction = Direction.RIGHT;
		tickRate = startingTickRate;
		hasHitItself = false;

		snakeBodyList = new List<Rect2>
			{
				new Rect2(0, 0, 40, 40),
				new Rect2(40, 0, 40, 40)
			};
	}

	private void UpdateHeadPosition(Vector2 translation)
	{
		var newHeadRect = new Rect2(snakeBodyList[0].Position, snakeBodyList[0].Size);
		newHeadRect.Position += translation;

		Vector2 offset = Game.instance.SnakeStartingPosition;
		Vector2 limitOffset = offset + snakeBodyList[0].Size / 2;

		if (newHeadRect.Position.X < 0 - limitOffset.X)
		{
			newHeadRect.Position = new Vector2(GetViewportRect().Size.X - offset.X, newHeadRect.Position.Y);
		}
		if (newHeadRect.Position.X > GetViewportRect().Size.X + limitOffset.X)
		{
			newHeadRect.Position = new Vector2(0 - offset.X, newHeadRect.Position.Y);
		}
		if (newHeadRect.Position.Y < 0 - limitOffset.Y)
		{
			newHeadRect.Position = new Vector2(newHeadRect.Position.X, GetViewportRect().Size.Y - offset.Y);
		}
		if (newHeadRect.Position.Y > GetViewportRect().Size.Y + limitOffset.Y)
		{
			newHeadRect.Position = new Vector2(newHeadRect.Position.X, 0 - offset.Y);
		}
		GD.Print($"New HEAD: {newHeadRect.Position}");
		snakeBodyList.Insert(0, newHeadRect);
	}


	// Check if the snake has collided with itself
	private bool HasHitItself()
	{
		for (int i = 1; i < snakeBodyList.Count; i++)
		{
			if (snakeBodyList[i].Position.X == snakeBodyList[0].Position.X && snakeBodyList[i].Position.Y == snakeBodyList[0].Position.Y)
			{
				GD.Print("Has hit itself!");
				hasHitItself = true;
				return true;
			}
		}
		return false;
	}
}
