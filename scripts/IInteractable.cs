namespace TheGoblinExam.scripts;

public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact();
    void Highlight(bool enabled);
}