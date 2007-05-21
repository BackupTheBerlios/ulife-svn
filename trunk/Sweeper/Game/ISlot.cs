namespace Sweeper
{
    public interface ISlot
    {
        bool Mine { get; }

        bool Hidden { get; }

        void Uncover();
    }
}