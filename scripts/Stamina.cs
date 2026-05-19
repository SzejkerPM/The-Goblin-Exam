using Godot;

namespace TheGoblinExam.Scripts;

public partial class Stamina : Node
{
    [Signal]
    public delegate void StaminaChangedEventHandler(float currentValue, float maxValue);

    [ExportGroup("Stamina Values")] [Export]
    private float _maxValue = 100f;

    [Export] private float _regenRatePerSecond = 20f;
    [Export] private float _dropRatePerSecond = 40f;
    [Export] private float _regenDelaySeconds = 2f;
    [Export] private float _exhaustionDelaySeconds = 4f;

    private float _currentValue;
    private float _currentDelayTimer;

    private bool _isExhausted;

    public override void _Ready()
    {
        _currentValue = _maxValue;
        _currentDelayTimer = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CanRest())
        {
            Rest(delta);
        }

        if (CanRegenerate())
        {
            Regenerate(delta);
        }
    }

    public bool CanUseStamina()
    {
        return _currentValue > 0 && !_isExhausted;
    }

    public void ConsumeStamina(double delta)
    {
        if (!_isExhausted)
        {
            _currentValue = Mathf.Clamp(_currentValue - _dropRatePerSecond * (float)delta, 0, _maxValue);
            _currentDelayTimer = _regenDelaySeconds;

            if (_currentValue <= 0)
            {
                _isExhausted = true;
                _currentDelayTimer = _exhaustionDelaySeconds;
            }

            NotifyStaminaChanged();
        }
    }

    private bool CanRest()
    {
        return _currentDelayTimer > 0;
    }

    private void Rest(double delta)
    {
        _currentDelayTimer -= (float)delta;

        if (_currentDelayTimer < 0)
        {
            _isExhausted = false;
            _currentDelayTimer = 0;
        }
    }

    private bool CanRegenerate()
    {
        return _currentDelayTimer <= 0 && _currentValue < _maxValue;
    }

    private void Regenerate(double delta)
    {
        _currentValue = Mathf.Clamp(_currentValue + _regenRatePerSecond * (float)delta, 0, _maxValue);
        NotifyStaminaChanged();
    }

    private void NotifyStaminaChanged()
    {
        EmitSignal(SignalName.StaminaChanged, _currentValue, _maxValue);
    }
}