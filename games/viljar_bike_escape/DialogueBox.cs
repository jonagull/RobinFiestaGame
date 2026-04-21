using Godot;

/// <summary>
/// Star Fox-style portrait dialogue box. Lives on the HUD.
/// Call Show() from any chaser to display a transmission.
/// </summary>
public partial class DialogueBox : Control
{
	private TextureRect _portrait;
	private Label       _nameLabel;
	private Label       _textLabel;

	private const float MinDisplayTime = 2f;

	private float _timer       = 0f;
	private float _displayedFor = 0f;
	private bool  _showing     = false;

	public override void _Ready()
	{
		_portrait  = GetNode<TextureRect>("Panel/PortraitRect");
		_nameLabel = GetNode<Label>("Panel/NameLabel");
		_textLabel = GetNode<Label>("Panel/TextLabel");
		Visible = false;
	}

	public void Show(string characterName, Texture2D portrait, string text, float duration = 3.5f)
	{
		// Don't cut off a message that just started
		if (_showing && _displayedFor < MinDisplayTime) return;

		_nameLabel.Text   = characterName;
		_portrait.Texture = portrait;
		_textLabel.Text   = text;
		_timer       = duration;
		_displayedFor = 0f;
		_showing     = true;
		Visible      = true;
	}

	public override void _Process(double delta)
	{
		if (!_showing) return;
		_timer        -= (float)delta;
		_displayedFor += (float)delta;
		if (_timer <= 0f)
		{
			_showing = false;
			Visible  = false;
		}
	}
}
