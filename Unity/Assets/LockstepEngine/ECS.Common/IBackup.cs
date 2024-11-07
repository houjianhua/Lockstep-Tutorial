using System.Text;
using Lockstep.Serialization;

namespace Lockstep.Game {
    public interface IBackup : IDumpStr {
        /// <summary>
        /// 写入备份
        /// </summary>
        /// <param name="writer"></param>
        void WriteBackup(Serializer writer);
        void ReadBackup(Deserializer reader);
    }

    public interface IHashCode {
        int GetHash(ref int idx);
    }

    public interface IDumpStr {
        void DumpStr(StringBuilder sb, string prefix);
    }

    public interface IAfterBackup {
        void OnAfterDeserialize(); //反序列化后重构
    }
}