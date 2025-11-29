namespace PhotoBookRenamer.Services
{
    public interface IUndoRedoService
    {
        bool CanUndo { get; }
        bool CanRedo { get; }
        void Execute(ICommand command);
        void Undo();
        void Redo();
        void Clear();
    }

    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
