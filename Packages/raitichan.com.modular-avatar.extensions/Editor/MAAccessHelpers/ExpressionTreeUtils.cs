using System;
using System.Linq.Expressions;
using System.Reflection;

namespace raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers {
	public static class ExpressionTreeUtils {
		public static Action CreateMethodCallAction(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			Expression<Action> expression = Expression.Lambda<Action>(
				Expression.Call(methodInfo)
			);
			return expression.Compile();
		}

		public static Action<TArg0> CreateMethodCallAction<TArg0>(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");
			Expression<Action<TArg0>> expression = Expression.Lambda<Action<TArg0>>(
				Expression.Call(methodInfo, arg0Parameter),
				arg0Parameter);
			return expression.Compile();
		}
		

		public static Func<TResult> CreateMethodCallFunction<TResult>(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			Expression<Func<TResult>> expression = Expression.Lambda<Func<TResult>>(
				Expression.Call(methodInfo)
			);
			return expression.Compile();
		}

		public static Func<TArg0, TResult> CreateMethodCallFunction<TArg0, TResult>(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");
			Expression<Func<TArg0, TResult>> expression = Expression.Lambda<Func<TArg0, TResult>>(
				Expression.Call(methodInfo, arg0Parameter),
				arg0Parameter);
			return expression.Compile();
		}
	}
}