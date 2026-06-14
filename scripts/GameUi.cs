using Godot;

namespace TheGoblinExam.Scripts;

public partial class GameUi : CanvasLayer
{
    private TextureProgressBar _staminaProgressBar;
    private HBoxContainer _eKeyContainer;
    private Label _actionText;

    public override void _Ready()
    {
        _staminaProgressBar = GetNode<TextureProgressBar>("%StaminaProgressBar");
        _eKeyContainer = GetNode<HBoxContainer>("%EKeyContainer");
        _actionText = GetNode<Label>("%ActionText");
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