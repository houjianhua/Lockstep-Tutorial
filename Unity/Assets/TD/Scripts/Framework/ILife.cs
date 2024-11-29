namespace TDFramework
{
    public interface ILife
    {
        void Init();
        void Tick(int delta);
        void Release();
    }
}