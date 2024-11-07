using Lockstep.Game;

namespace Lockstep.Game {
    public interface IGameResourceService : IService {
        //用于加载Entity Prefab的
        object LoadPrefab(int id);
    }
}