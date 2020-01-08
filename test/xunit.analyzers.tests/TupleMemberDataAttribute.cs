using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.Analyzers
{
	[DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class TupleMemberDataAttribute : MemberDataAttributeBase
	{
		private static readonly Type[] TupleOpenGenericTypes = { typeof(Tuple<>), typeof(Tuple<,>), typeof(Tuple<,,>), typeof(Tuple<,,,>), typeof(Tuple<,,,,>), typeof(Tuple<,,,,,>), typeof(Tuple<,,,,,,>), };
		private static readonly ConcurrentDictionary<Type, Func<object, object[]>> ConverterCache = new ConcurrentDictionary<Type, Func<object, object[]>>();

		public TupleMemberDataAttribute(string memberName, params object[] parameters)
			: base(memberName, parameters)
		{ }

		protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
		{
			if (item == null)
				return null;

			var converter = GetConverter(item);
			if (converter == null)
				throw new ArgumentException($"Property {MemberName} on {MemberType ?? testMethod.DeclaringType} yielded an item that is not an Tuple");

			return converter(item);
		}

		static Func<object, object[]> GetConverter(object item)
		{
			if (!item.GetType().IsGenericType || !TupleOpenGenericTypes.Contains(item.GetType().GetGenericTypeDefinition()))
				return null;

			return ConverterCache.GetOrAdd(item.GetType(), valueFactory: t =>
			{
				var methodName = "Convert" + t.GenericTypeArguments.Length;

				// (object item) => Convert<TN>((Tuple<TN>)item)
				var param = Expression.Parameter(typeof(object), "item");
				var cast = Expression.Convert(param, t);
				var call = Expression.Call(typeof(TupleMemberDataAttribute), methodName, t.GenericTypeArguments, cast);
				return Expression.Lambda<Func<object, object[]>>(call, param).Compile();
			});
		}

		static object[] Convert1<T1>(Tuple<T1> t)
		{
			return new object[] { t.Item1 };
		}

		static object[] Convert2<T1, T2>(Tuple<T1, T2> t)
		{
			return new object[] { t.Item1, t.Item2 };
		}

		static object[] Convert3<T1, T2, T3>(Tuple<T1, T2, T3> t)
		{
			return new object[] { t.Item1, t.Item2, t.Item3 };
		}

		static object[] Convert4<T1, T2, T3, T4>(Tuple<T1, T2, T3, T4> t)
		{
			return new object[] { t.Item1, t.Item2, t.Item3, t.Item4 };
		}

		static object[] Convert5<T1, T2, T3, T4, T5>(Tuple<T1, T2, T3, T4, T5> t)
		{
			return new object[] { t.Item1, t.Item2, t.Item3, t.Item4, t.Item5 };
		}

		static object[] Convert6<T1, T2, T3, T4, T5, T6>(Tuple<T1, T2, T3, T4, T5, T6> t)
		{
			return new object[] { t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6 };
		}

		static object[] Convert7<T1, T2, T3, T4, T5, T6, T7>(Tuple<T1, T2, T3, T4, T5, T6, T7> t)
		{
			return new object[] { t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7 };
		}
	}
}
