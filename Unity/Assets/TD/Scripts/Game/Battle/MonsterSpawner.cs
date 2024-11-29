// using Lockstep.Collision2D;
// using Lockstep.Math;
// using TDFramework;

// namespace TD
// {
//     public class MonsterSpawner : ILife
//     {
//         public BattleScene scene;
//         private int spawnTimer;
//         private int spawnIdx;
//         private LVector2 spawnPosition;
//         public void Init()
//         {
//             spawnTimer = scene.levelConfig.SpawnStart;
//             spawnIdx = 0;
//             spawnPosition = new LVector2();
//         }

//         public void Release()
//         {
//             throw new System.NotImplementedException();
//         }

//         public void Tick(int delta)
//         {
//             spawnTimer -= delta;
//             if (spawnTimer <= 0)
//             {
//                 spawnTimer += scene.levelConfig.SpawnInterval;
//                 SpawnMonster(scene.levelConfig.Monsters[spawnIdx]);
//                 spawnIdx++;
//             }
//         }

//         private void SpawnMonster(int monsterId)
//         {
//             var entity = new BattleActor();
//             entity.compType = BattleType.Monster;
//             entity.configId = monsterId;
//             entity.transform = new CTransform2D(spawnPosition);
//             var move = entity.AddComponent<PathMoveComponent>();
//             move.path = scene.monsterPath;
//             scene.AddBattleEntity(entity);
//         }
//     }
// }