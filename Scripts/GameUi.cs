using Godot;

namespace TheGoblinExam.Scripts;

public partial class GameUi : CanvasLayer
{
    [Export] private TextureProgressBar _staminaProgressBar;

    public void OnStaminaChanged(float currentValue, float maxValue)
    {
        _staminaProgressBar.MaxValue = maxValue;
        _staminaProgressBar.Value = currentValue;
    }
}