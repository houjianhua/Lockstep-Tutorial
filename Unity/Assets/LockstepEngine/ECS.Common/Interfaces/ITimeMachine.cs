using System.Collections;
using System.Collections.Generic;

namespace Lockstep.Game {
    public interface ITimeMachine {
        int CurTick { get;}
        ///Rollback to tick , so all cmd between [tick,~)(Include tick) should undo
        void RollbackTo(int tick);
        /// <summary>
        /// 所有实现了ITimeMachine的Service都将执行备份
        /// </summary>
        /// <param name="tick"></param>
        void Backup(int tick);
        ///Discard all cmd between [0,maxVerifiedTick] (Include maxVerifiedTick)
        void Clean(int maxVerifiedTick);
    }
}