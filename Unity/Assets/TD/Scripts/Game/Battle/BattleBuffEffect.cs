// using System;
// using System.Collections.Generic;
// using TDFramework;

// namespace TD
// {
//     public class BattleBuffEffect : ILife
//     {
//         public BuffType buffType;
//         public BattleActor actor;

//         public List<BattleBuff> buffs;

//         public void Init()
//         {
//             buffs = new List<BattleBuff>();
//             //添加效果
//             switch (buffType)
//             {
//                 case BuffType.Dizz:
//                     actor.PlayEffect(effect);
//                     actor.AddTag(BattleTagType.DontMove, GameConst.BuffTagStartId + (int)buffType);
//                     break;
//                 case BuffType.AddHp:
//                     break;
//                 case BuffType.AddMp:
//                     break;
//                 default:
//                     break;
//             }
//         }

//         public void Release()
//         {
//             buffs.Clear();
//             buffs = null;
//             actor = null;
//         }

//         public void Tick(int delta)
//         {
//         }

//         public void AddBuff(BattleBuff buff)
//         {
//             buffs.Add(buff);
//         }

//         public void RemoveBuff(BattleBuff buff)
//         {
//             buffs.Remove(buff);
//             if (this.buffs.Count == 0)
//             {
//                 //移除效果
//             }
//         }
//     }
// }