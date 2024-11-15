namespace Lockstep.Network {
    public interface IMessageDispatcher {
        /// <summary>
        /// 接到消息后 都会通过此接口派发消息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="packet"></param>
        void Dispatch(Session session, Packet packet);
    }
}