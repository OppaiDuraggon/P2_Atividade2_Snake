using Godot;
using System;
using System.Collections.Generic;

public enum GameState { PLAYING, GAME_WIN, GAME_OVER }

public partial class Game : Node2D
{
	public static Game instance;
	public static GameState CurrentState;
	public Vector2 SnakeStartingPosition;

	private PackedScene consumableScene = GD.Load<PackedScene>("res://Scenes/Consumable.tscn");
	private SnakeController snakeController;
	private List<Consumable> consumablesSpawnedList = new List<Consumable>();

	private AudioStreamPlayer audioGameOver, audioWin;

	private int snakeBodySize = 40; // Size of each snake segment
	private Vector2 gameSize = new Vector2(30, 16); // Game grid sizes

	private Label scoreLabel, timerLabel;
	private UIBannerController uiBanner;

	private int currentScore;
	private double timeElapsed;
	private float maxLevelTime = 50f;
	private double timeSinceLastConsumableSpawn;
	private float timeBetweenConsumableSpawn = 4f;

	// Called when the node is added to the scene.
	public override void _Ready()
	{
		if (instance == null) { instance = this; }

		scoreLabel = GetNode<Label>("ScoreLabel");
		scoreLabel.Text = $"Score: {currentScore}";

		timerLabel = GetNode<Label>("TimerLabel");
		timerLabel.Text = $"Time: {(maxLevelTime - timeElapsed).ToString("00")}";

		uiBanner = GetNode<UIBannerController>("UIBanner");
		audioGameOver = GetNode<AudioStreamPlayer>("AudioGameOver");
		audioWin = GetNode<AudioStreamPlayer>("AudioWin");

		// Get the SnakeBody node and set its initial position
		snakeController = GetNode<SnakeController>("SnakeController");
		SnakeStartingPosition = snakeController.Position;

		uiBanner.Visible = false;
		CurrentState = GameState.PLAYING;

		SpawnNewConsumable();
		timeSinceLastConsumableSpawn = 0f;
	}

	// Called every frame, delta is the elapsed time since the last frame.
	public override void _Process(double delta)
	{
		if (CurrentState != GameState.PLAYING) { return; }

		timeElapsed += delta;
		if (timeElapsed >= maxLevelTime) { OnGameOver(); }
		else { timerLabel.Text = $"Time: {(maxLevelTime - timeElapsed).ToString("00")}"; }

		timeSinceLastConsumableSpawn += delta;
		if (timeSinceLastConsumableSpawn >= timeBetweenConsumableSpawn)
		{
			SpawnNewConsumable();
			timeSinceLastConsumableSpawn = 0f;
		}
	}

	// Called when the game is over
	public void OnGameOver()
	{
		uiBanner.Visible = true;
		uiBanner.OnGameOver();
		audioGameOver.Play();
		CurrentState = GameState.GAME_OVER;
	}

	public void OnGameWin()
	{
		uiBanner.Visible = true;
		uiBanner.OnGameWin();
		audioWin.Play();
		CurrentState = GameState.GAME_WIN;
	}

	public void OnConsumableEaten(Consumable eatenConsumable)
	{
		snakeController.HasEatenConsumable = true;

		currentScore++;
		scoreLabel.Text = $"Score: {currentScore}";

		consumablesSpawnedList.Remove(eatenConsumable);
	}

	// Creates a new consumable at a random position
	public void SpawnNewConsumable()
	{
		Node consumableNode = consumableScene.Instantiate();
		AddChild(consumableNode);
		Consumable consumable = consumableNode.GetNode<Consumable>(consumableNode.GetPath());
		consumablesSpawnedList.Add(consumable);

		Random rng = new();

		for (int i = 0; i <= 5; i++)
		{
			Vector2 spawnPosition = new Vector2(
					rng.Next((int)gameSize.X) * snakeBodySize,
					rng.Next((int)gameSize.Y) * snakeBodySize);

			if (consumable.CanSpawnAt(spawnPosition))
			{
				consumable.Position = spawnPosition;
				GD.Print($"New Food Position: {consumable.Position}");
				return;
			}
		}

		//if it reaches here that means it couldn't spawn, so remove the consumable reference
		consumablesSpawnedList.Remove(consumable);
		consumable.QueueFree();
	}

	public void ResetGame()
	{
		currentScore = 0;
		scoreLabel.Text = $"Score: {currentScore}";

		timeElapsed = 0f;
		timerLabel.Text = $"Time: {(maxLevelTime - timeElapsed).ToString("00")}";

		uiBanner.Visible = false;

		snakeController.Position = SnakeStartingPosition;
		snakeController.ResetController();

		CurrentState = GameState.PLAYING;

		foreach (var consumable in consumablesSpawnedList)
		{
			consumable.QueueFree();
		}

		consumablesSpawnedList = new List<Consumable>();

		SpawnNewConsumable();
		timeSinceLastConsumableSpawn = 0f;
	}
}