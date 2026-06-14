using Godot;

public partial class DeathScreen : CanvasLayer
{
    [Export] private Button _restartButton;

    public override void _Ready()
    {
        Visible = false;
        ProcessMode = ProcessModeEnum.WhenPaused;

        if (_restartButton != null)
            _restartButton.Pressed += OnRestartPressed;
    }

    public void ShowGameOver()
    {
        Visible = true;
    }

    private void OnRestartPressed()
    {
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }
}