using Godot;

namespace TheGoblinExam.scripts;

public partial class Shopkeeper : Area2D, IInteractable
{
    [Signal]
    public delegate void MinigameRequestedEventHandler();

    public string InteractionPrompt => "Sell items";

    [Export] public int Gold { get; private set; } = 1000;

    private AnimatedSprite2D _animatedSprite2D;
    private TextureRect _textBubble;
    private Label _text;
    private int _goldNeededForNextLevel;
    private int _playerGold;

    private string TextMissingGold => $"You still need {_goldNeededForNextLevel - _playerGold} more gold to leave!";
    private const string TextEnoughGold = "You have enough gold. You can leave now!";

    public override void _Ready()
    {
        _animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _textBubble = GetNode<TextureRect>("%TextBubble");
        _text = GetNode<Label>("%ShopkeeperText");
    }

    public void Initialize(int targetGold)
    {
        _goldNeededForNextLevel = targetGold;
    }

    public void Interact()
    {
        if (_playerGold < _goldNeededForNextLevel)
        {
            _text.Text = TextMissingGold;
        }
        else
        {
            _text.Text = TextEnoughGold;
        }

        _textBubble.SetVisible(true);

        GetTree().CreateTimer(5.0).Timeout += () => { _textBubble.SetVisible(false); };
    }

    public void ReduceGold(int goldToReduce)
    {
        Gold -= goldToReduce;
    }

    public void UpdatePlayerGold(int gold)
    {
        _playerGold = gold;
    }

    public void Highlight(bool enabled)
    {
        if (enabled)
        {
            _animatedSprite2D.SelfModulate = new Color(1.5f, 1.5f, 1.5f);
        }
        else
        {
            _animatedSprite2D.SelfModulate = Colors.White;
        }
    }
}