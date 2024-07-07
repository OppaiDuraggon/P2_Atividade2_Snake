using Godot;

public partial class UIBannerController : Sprite2D
{
	private Label bannerLabel;
	private Color gameOverColor = new Color(0.82f, 0.28f, 0.28f);
	private Color winColor = new Color(0.28f, 0.82f, 0.28f);

	public override void _Ready()
	{
		bannerLabel = GetNode<Label>("GameStateLabel");
	}

	public void OnGameOver()
	{
		bannerLabel.Text = "Game Lost!";
		SelfModulate = gameOverColor;
	}

	public void OnGameWin()
	{
		bannerLabel.Text = "Game Won!";
		SelfModulate = winColor;
	}
}
