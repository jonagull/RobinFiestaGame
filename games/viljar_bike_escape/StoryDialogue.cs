using System;
using Godot;
using Godot.Collections;

public partial class StoryDialogue : Control
{
    [Signal] public delegate void ScriptFinishedEventHandler();

    private TextureRect   _portrait;
    private Label         _nameLabel;
    private RichTextLabel _bodyText;
    private Label         _continueLabel;

    private Array<Dictionary> _pages;
    private int   _pageIndex    = 0;
    private float _charsPerSec  = 40f;
    private float _elapsed      = 0f;
    private float _inputLockout = 0f;   // prevents double-advance right after page change
    private bool  _crawling     = false;
    private bool  _waitingInput = false;

    public override void _Ready()
    {
        _portrait      = GetNode<TextureRect>("Panel/HBox/PortraitRect");
        _nameLabel     = GetNode<Label>("Panel/HBox/VBox/NameLabel");
        _bodyText      = GetNode<RichTextLabel>("Panel/HBox/VBox/BodyText");
        _continueLabel = GetNode<Label>("Panel/ContinueLabel");
        _continueLabel.Visible = false;
        Visible = false;
    }

    // JSON format: [{ "speaker": "Robin", "text": "Hello!" }, ...]
    public void PlayScript(string jsonPath, float charsPerSec = 40f)
    {
        _charsPerSec = charsPerSec;
        using var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError($"StoryDialogue: cannot open {jsonPath}");
            return;
        }
        _pages     = Json.ParseString(file.GetAsText()).AsGodotArray<Dictionary>();
        _pageIndex = 0;
        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        var page    = _pages[_pageIndex];
        var speaker = page.ContainsKey("speaker") ? page["speaker"].AsString() : "";
        var text    = page.ContainsKey("text")    ? page["text"].AsString()    : "";

        GD.Print($"[StoryDialogue] Page {_pageIndex}: speaker='{speaker}'");

        _nameLabel.Text    = speaker;
        _nameLabel.Visible = speaker.Length > 0;
        GD.Print($"[StoryDialogue] Name label set.");

        var portrait = LoadPortrait(speaker);
        _portrait.Texture = portrait;
        _portrait.Visible = portrait != null;
        GD.Print($"[StoryDialogue] Portrait: {(portrait != null ? "loaded" : "none")}");

        _elapsed       = 0f;
        _inputLockout  = 0.25f;
        _bodyText.Text = text;
        _bodyText.VisibleCharacters = 0;
        _crawling      = true;
        _waitingInput  = false;
        _continueLabel.Visible = false;
        Visible = true;
        GD.Print($"[StoryDialogue] Page {_pageIndex} ready, crawling.");
    }

    public override void _Process(double delta)
    {
        if (_inputLockout > 0f)
            _inputLockout -= (float)delta;

        if (!_crawling) return;
        _elapsed += (float)delta;
        int shown = Mathf.Min((int)(_elapsed * _charsPerSec), _bodyText.Text.Length);
        _bodyText.VisibleCharacters = shown;
        if (shown >= _bodyText.Text.Length)
            FinishCrawl();
    }

    public override void _Input(InputEvent e)
    {
        if (!Visible || _inputLockout > 0f) return;

        // Ignore key-repeat echo events
        if (e is InputEventKey k && k.Echo) return;

        bool advance = e.IsActionPressed("ui_accept")
                    || (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left);
        if (!advance) return;
        GetViewport().SetInputAsHandled();

        if (_crawling)
            FinishCrawl();
        else if (_waitingInput)
            AdvanceOrFinish();
    }

    private Texture2D LoadPortrait(string speaker)
    {
        if (speaker.Length == 0) return null;
        try
        {
            string folder = $"res://games/viljar_bike_escape/assets/portraits/{speaker.ToLower()}";
            var dir = DirAccess.Open(folder);
            if (dir == null) return null;

            var files = new Array<string>();
            dir.ListDirBegin();
            for (string f = dir.GetNext(); f != ""; f = dir.GetNext())
                if (!dir.CurrentIsDir() && (f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".jpeg") || f.EndsWith(".webp")))
                    files.Add($"{folder}/{f}");
            dir.ListDirEnd();

            if (files.Count == 0) return null;
            string chosen = files[(int)GD.RandRange(0, files.Count - 1)];
            GD.Print($"[StoryDialogue] Loading portrait: {chosen}");
            return ResourceLoader.Load<Texture2D>(chosen);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[StoryDialogue] Portrait error for '{speaker}': {ex.Message}");
            return null;
        }
    }

    private void FinishCrawl()
    {
        GD.Print($"[StoryDialogue] Page {_pageIndex} crawl done, waiting for input.");
        _crawling = false;
        _bodyText.VisibleCharacters = -1;
        _inputLockout  = 0.1f;
        _waitingInput  = true;
        _continueLabel.Visible = true;
    }

    private void AdvanceOrFinish()
    {
        GD.Print($"[StoryDialogue] Advancing from page {_pageIndex}.");
        _waitingInput          = false;
        _continueLabel.Visible = false;
        _pageIndex++;
        if (_pageIndex < _pages.Count)
            ShowCurrentPage();
        else
        {
            GD.Print($"[StoryDialogue] Script finished.");
            Visible = false;
            EmitSignal(SignalName.ScriptFinished);
        }
    }
}
