using System;
using System.Collections.Generic;
using System.Linq;

namespace raitichan.com.modular_avatar.extensions {
	public static class LinqExtensions {
		public static T MinBy<T, U>(this IEnumerable<T> xs, Func<T, U> key) where U : IComparable<U> {
			return xs.Aggregate((a, b) => key(a).CompareTo(key(b)) < 0 ? a : b);
		}
		
		public static T MaxBy<T, U>(this IEnumerable<T> xs, Func<T, U> key) where U : IComparable<U> {
			return xs.Aggregate((a, b) => key(a).CompareTo(key(b)) > 0 ? a : b);
		}
	}
}