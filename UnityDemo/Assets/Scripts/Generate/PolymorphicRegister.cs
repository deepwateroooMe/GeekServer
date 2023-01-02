using MessagePack;
using MessagePack.Resolvers;
using PolymorphicMessagePack;
using Resolvers;

namespace Geek.Server.Proto
{
	public partial class PolymorphicRegister // 这里暂时不明白:这些程序集是从哪里来的　？ 还是说，这里是定义了某些类的自动生成方法呢　？
	{

		static bool serializerRegistered = false;
		private static void Init()
		{
			if (!serializerRegistered)
			{
				PolymorphicResolver.AddInnerResolver(ConfigDataResolver.Instance);
				PolymorphicResolver.AddInnerResolver(MessagePack.Resolvers.GeneratedResolver.Instance);
				PolymorphicTypeMapper.Register<Geek.Server.Message>();
				PolymorphicResolver.Init();
				serializerRegistered = true;
			}
		}

		public static void Load() { Init(); }
	}
}
