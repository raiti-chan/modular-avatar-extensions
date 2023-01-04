using System;
using System.Linq.Expressions;
using System.Reflection;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper {
	public static class ExpressionTreeUtils {
		public static Action CreateStaticMethodCallAction(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			Expression<Action> expression = Expression.Lambda<Action>(
				Expression.Call(methodInfo)
			);
			return expression.Compile();
		}

		public static Action<TArg0> CreateStaticMethodCallAction<TArg0>(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");
			Expression<Action<TArg0>> expression = Expression.Lambda<Action<TArg0>>(
				Expression.Call(methodInfo, arg0Parameter),
				arg0Parameter);
			return expression.Compile();
		}


		public static Func<TResult> CreateStaticMethodCallFunction<TResult>(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			Expression<Func<TResult>> expression = Expression.Lambda<Func<TResult>>(
				Expression.Call(methodInfo)
			);
			return expression.Compile();
		}

		public static Func<TArg0, TResult> CreateStaticMethodCallFunction<TArg0, TResult>(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");
			Expression<Func<TArg0, TResult>> expression = Expression.Lambda<Func<TArg0, TResult>>(
				Expression.Call(methodInfo, arg0Parameter),
				arg0Parameter);
			return expression.Compile();
		}

		public static Func<TInstance, TResult> CreateInstanceValueGetFunction<TInstance, TResult>(PropertyInfo fieldInfo) {
			if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

			ParameterExpression instanceParameter = Expression.Parameter(typeof(TInstance), "instance");
			Expression<Func<TInstance, TResult>> expression = Expression.Lambda<Func<TInstance, TResult>>(
				Expression.Property(instanceParameter, fieldInfo),
				instanceParameter);
			return expression.Compile();
		}
	}
}