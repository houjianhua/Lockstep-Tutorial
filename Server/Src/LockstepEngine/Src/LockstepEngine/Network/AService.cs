using System;
using System.Net;
using System.Threading.Tasks;

namespace Lockstep.Network
{
	public enum NetworkProtocol
	{
		TCP,
	}

	public abstract class AService :NetBase
	{
		public abstract AChannel GetChannel(long id);

		/// <summary>
		/// 每个客户的链接进来都会创建一个Channel
		/// </summary>
		/// <returns></returns>
		public abstract Task<AChannel> AcceptChannel();

		public abstract AChannel ConnectChannel(IPEndPoint ipEndPoint);

		public abstract void Remove(long channelId);

		public abstract void Update();
	}
}