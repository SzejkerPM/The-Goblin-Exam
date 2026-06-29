using Godot;

namespace TheGoblinExam.Scripts;

public partial class GameUi : CanvasLayer
{
    [Export] private DeathScreen _deathScreen;
    [Export] private Minigame _minigame;
    [Export] private Player _player;

    private TextureProgressBar _staminaProgressBar;
    private HBoxContainer _eKeyContainer;
    private Label _actionText;
    private Openable _currentRequestingOpenable;

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public override void _Ready()
    {
        _staminaProgressBar = GetNode<TextureProgressBar>("%StaminaProgressBar");
        _eKeyContainer = GetNode<HBoxContainer>("%EKeyContainer");
        _actionText = GetNode<Label>("%ActionText");

        if (_deathScreen != null)
            _deathScreen.Visible = false;
        else
            GD.PrintErr("GameUi.cs: DeathScreen is not assigned.");

        if (_minigame != null)
        {
            _minigame.Visible = false;
            _minigame.MinigameComplete += OnMinigameComplete;
        }
    }

    private void OnMinigameComplete()
    {
        _minigame.Visible = false;
        _player?.SetMovementBlocked(false);
        GD.Print("GameUi: Minigame completed event received.");
        
        if (_currentRequestingOpenable != null)
        {
            _currentRequestingOpenable.CompleteInteraction();
            _currentRequestingOpenable = null;
        }
    }

    public void ShowMinigame()
    {
        _currentRequestingOpenable = null;
        _player?.SetMovementBlocked(true);
        _minigame?.StartMinigame();
    }

    public void ShowMinigameForOpenable(Openable openable)
    {
        _currentRequestingOpenable = openable;
        _player?.SetMovementBlocked(true);
        _minigame?.StartMinigame();
    }

    public void ShowDeathScreen()
    {
        if (_deathScreen == null)
        {
            GD.PrintErr("GameUi.cs: Cannot show death screen because reference is null.");
            return;
        }

        _deathScreen.ShowGameOver();
    }

    public void OnStaminaChanged(float currentValue, float maxValue)
    {
        _staminaProgressBar.MaxValue = maxValue;
        _staminaProgressBar.Value = currentValue;
    }

    public void HideEKeyContainer()
    {
        _eKeyContainer.Visible = false;
    }

    public void ShowEKeyContainer(string interactionPrompt)
    {
        _actionText.Text = interactionPrompt;
        _eKeyContainer.Visible = true;
    }
}