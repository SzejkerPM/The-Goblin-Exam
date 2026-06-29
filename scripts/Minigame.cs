using Godot;
using System;
using System.Collections.Generic;

namespace TheGoblinExam.Scripts;

public partial class Minigame : Control
{
    [Signal]
    public delegate void MinigameCompleteEventHandler();

    private Label[] _stepLabels;
    
    private const string Left = "left";
    private const string Right = "right";
    private const string LeftDisplay = "L";
    private const string RightDisplay = "R";
    private const string EmptyDisplay = ".";

    private List<string> _sequence = new List<string>();
    private int _currentStep = 0;
    private bool _isActive = false;

    public override void _Ready()
    {
        var container = GetNode<Control>("VBoxContainer/LabelContainer");
        var labels = new List<Label>();
        foreach (var child in container.GetChildren())
        {
            if (child is Label label)
            {
                labels.Add(label);
            }
        }
        _stepLabels = labels.ToArray();

        ResetMinigame();
    }

    public void StartMinigame()
    {
        ResetMinigame();
        _isActive = true;
        Visible = true;
    }

    public void ResetMinigame()
    {
        _isActive = false;
        _currentStep = 0;
        _sequence.Clear();

        Random random = new Random();
        for (int i = 0; i < _stepLabels.Length; i++)
        {
            _sequence.Add(random.Next(2) == 0 ? Left : Right);
        }

        if (_stepLabels != null)
        {
            foreach (var label in _stepLabels)
            {
                if (label != null) label.Text = EmptyDisplay;
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isActive) return;

        if (@event.IsActionPressed("ui_left"))
        {
            HandleGuess(Left);
        }
        else if (@event.IsActionPressed("ui_right"))
        {
            HandleGuess(Right);
        }
    }

    private void HandleGuess(string guess)
    {
        if (guess == _sequence[_currentStep])
        {
            _stepLabels[_currentStep].Text = guess == Left ? LeftDisplay : RightDisplay;
            _currentStep++;

            if (_currentStep >= _sequence.Count)
            {
                _isActive = false;
                EmitSignal(SignalName.MinigameComplete);
                GD.Print("Minigame Complete!");
            }
        }
        else
        {
            ResetMinigame();
            _isActive = true;
        }
    }
}
