// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Resolvers
{
    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        private GeneratedResolver()
        {
        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            internal static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    Formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        private static readonly global::System.Collections.Generic.Dictionary<global::System.Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(12)
            {
                { typeof(global::System.Collections.Generic.Dictionary<int, long>), 0 },
                { typeof(global::Geek.Server.Proto.HearBeat), 1 },
                { typeof(global::Geek.Server.Proto.ReqBagInfo), 2 },
                { typeof(global::Geek.Server.Proto.ReqLogin), 3 },
                { typeof(global::Geek.Server.Proto.ReqSellItem), 4 },
                { typeof(global::Geek.Server.Proto.ReqUseItem), 5 },
                { typeof(global::Geek.Server.Proto.ResBagInfo), 6 },
                { typeof(global::Geek.Server.Proto.ResErrorCode), 7 },
                { typeof(global::Geek.Server.Proto.ResItemChange), 8 },
                { typeof(global::Geek.Server.Proto.ResLevelUp), 9 },
                { typeof(global::Geek.Server.Proto.ResLogin), 10 },
                { typeof(global::Geek.Server.Proto.UserInfo), 11 },
            };
        }

        internal static object GetFormatter(global::System.Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.DictionaryFormatter<int, long>();
                case 1: return new MessagePack.Formatters.Geek.Server.Proto.HearBeatFormatter();
                case 2: return new MessagePack.Formatters.Geek.Server.Proto.ReqBagInfoFormatter();
                case 3: return new MessagePack.Formatters.Geek.Server.Proto.ReqLoginFormatter();
                case 4: return new MessagePack.Formatters.Geek.Server.Proto.ReqSellItemFormatter();
                case 5: return new MessagePack.Formatters.Geek.Server.Proto.ReqUseItemFormatter();
                case 6: return new MessagePack.Formatters.Geek.Server.Proto.ResBagInfoFormatter();
                case 7: return new MessagePack.Formatters.Geek.Server.Proto.ResErrorCodeFormatter();
                case 8: return new MessagePack.Formatters.Geek.Server.Proto.ResItemChangeFormatter();
                case 9: return new MessagePack.Formatters.Geek.Server.Proto.ResLevelUpFormatter();
                case 10: return new MessagePack.Formatters.Geek.Server.Proto.ResLoginFormatter();
                case 11: return new MessagePack.Formatters.Geek.Server.Proto.UserInfoFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1649 // File name should match first type name