using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.ReflectionHelper {
	public static class ExpressionTreeUtils {
		public static Action CreateStaticMethodCallAction(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			Expression<Action> expression = Expression.Lambda<Action>(
				Expression.Call(methodInfo)
			);
			return expression.Compile();
		}

		public static Action<TArg0> CreateStaticMethodCallAction<TArg0>(MethodInfo methodInfo, Type tArg0 = null) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");

			Expression arg0TypeAs = tArg0 != null ? Expression.TypeAs(arg0Parameter, tArg0) : null;

			Expression<Action<TArg0>> expression = Expression.Lambda<Action<TArg0>>(
				Expression.Call(methodInfo, arg0TypeAs ?? arg0Parameter),
				arg0Parameter);
			return expression.Compile();
		}

		public static Action<TArg0, TArg1, TArg2> CreateStaticMethodCallAction<TArg0, TArg1, TArg2>(MethodInfo methodInfo, Type tArg0 = null, Type tArg1 = null, Type tArg2 = null) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");
			ParameterExpression arg1Parameter = Expression.Parameter(typeof(TArg1), "arg_1");
			ParameterExpression arg2Parameter = Expression.Parameter(typeof(TArg2), "arg_2");

			Expression arg0TypeAs = tArg0 != null ? Expression.TypeAs(arg0Parameter, tArg0) : null;
			Expression arg1TypeAs = tArg1 != null ? Expression.TypeAs(arg1Parameter, tArg1) : null;
			Expression arg2TypeAs = tArg2 != null ? Expression.TypeAs(arg2Parameter, tArg2) : null;

			Expression<Action<TArg0, TArg1, TArg2>> expression = Expression.Lambda<Action<TArg0, TArg1, TArg2>>(
				Expression.Call(methodInfo, arg0TypeAs ?? arg0Parameter, arg1TypeAs ?? arg1Parameter, arg2TypeAs ?? arg2Parameter),
				arg0Parameter, arg1Parameter, arg2Parameter);
			return expression.Compile();
		}


		public static Func<TResult> CreateStaticMethodCallFunction<TResult>(MethodInfo methodInfo) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			Expression<Func<TResult>> expression = Expression.Lambda<Func<TResult>>(
				Expression.Call(methodInfo)
			);
			return expression.Compile();
		}

		public static Func<TArg0, TResult> CreateStaticMethodCallFunction<TArg0, TResult>(MethodInfo methodInfo, Type tArg0 = null) {
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");

			Expression arg0TypeAs = tArg0 != null ? Expression.TypeAs(arg0Parameter, tArg0) : null;

			Expression<Func<TArg0, TResult>> expression = Expression.Lambda<Func<TArg0, TResult>>(
				Expression.Call(methodInfo, arg0TypeAs ?? arg0Parameter),
				arg0Parameter);
			return expression.Compile();
		}

		public static Func<TArg0, TResult> CreateConstructor<TArg0, TResult>(ConstructorInfo constructorInfo) {
			if (constructorInfo == null) throw new ArgumentNullException(nameof(constructorInfo));

			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");
			Expression<Func<TArg0, TResult>> expression = Expression.Lambda<Func<TArg0, TResult>>(
				Expression.New(constructorInfo, arg0Parameter),
				arg0Parameter);
			return expression.Compile();
		}

		public static Action<TInstance> CreatePublicInstanceMethodCallAction<TInstance>(string methodName, Type tInstance = null) {
			Type type = To<TInstance>(tInstance);
			MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
			if (methodInfo == null) {
				throw new NullReferenceException($"Not Found : {type.Name}.{methodName}()");
			}

			ParameterExpression instanceParameter = Expression.Parameter(typeof(TInstance), "instance");
			Expression instanceTypeAs = Convert(instanceParameter, tInstance);

			Expression<Action<TInstance>> expression = Expression.Lambda<Action<TInstance>>(
				Expression.Call(instanceTypeAs, methodInfo),
				instanceParameter);
			return expression.Compile();
		}

		public static Action<TInstance, TArg0> CreateNonPublicInstanceMethodCallAction<TInstance, TArg0>(string methodName, Type tInstance = null, Type tArg0 = null) {
			Type type = tInstance ?? typeof(TInstance);
			MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (methodInfo == null) {
				throw new NullReferenceException($"Not Found : {type.Name}.{methodName}({tArg0?.Name ?? typeof(TArg0).Name})");
			}

			ParameterExpression instanceParameter = Expression.Parameter(typeof(TInstance), "instance");
			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");

			Expression instanceTypeAs = tInstance != null ? Expression.TypeAs(instanceParameter, tInstance) : null;
			Expression arg0TypeAs = tArg0 != null ? Expression.TypeAs(arg0Parameter, tArg0) : null;

			Expression<Action<TInstance, TArg0>> expression = Expression.Lambda<Action<TInstance, TArg0>>(
				Expression.Call(instanceTypeAs ?? instanceParameter, methodInfo, arg0TypeAs ?? arg0Parameter),
				instanceParameter, arg0Parameter);
			return expression.Compile();
		}

		public static Func<TInstance, TResult> CreateInstanceValueGetFunction<TInstance, TResult>(PropertyInfo propertyInfo) {
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

			ParameterExpression instanceParameter = Expression.Parameter(typeof(TInstance), "instance");
			Expression<Func<TInstance, TResult>> expression = Expression.Lambda<Func<TInstance, TResult>>(
				Expression.Property(instanceParameter, propertyInfo),
				instanceParameter);
			return expression.Compile();
		}

		public static Func<TInstance, TResult> CreateInstanceValueGetFunction<TInstance, TResult>(PropertyInfo propertyInfo, Type instanceType) {
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
			if (instanceType == null) throw new ArgumentNullException(nameof(instanceType));

			ParameterExpression instanceParameter = Expression.Parameter(typeof(TInstance), "instance");
			Expression<Func<TInstance, TResult>> expression = Expression.Lambda<Func<TInstance, TResult>>(
				Expression.Property(Expression.Convert(instanceParameter, instanceType), propertyInfo),
				instanceParameter);
			return expression.Compile();
		}

		public static Action<TInstance, TArg0> CreateNonPublicInstancePropertyGetFunction<TInstance, TArg0>(string propertyName, Type tInstance = null, Type tArg0 = null) {
			Type type = tInstance ?? typeof(TInstance);
			Type valueType = tArg0 ?? typeof(TArg0);

			PropertyInfo propertyInfo = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (propertyInfo == null) {
				throw new NullReferenceException($"Not Found : {type.Name}.{propertyName}.set({valueType.Name})");
			}

			ParameterExpression instanceParameter = Expression.Parameter(typeof(TInstance), "instance");
			ParameterExpression arg0Parameter = Expression.Parameter(typeof(TArg0), "arg_0");

			Expression instanceTypeAs = Convert(instanceParameter, tInstance);
			Expression arg0TypeAs = Convert(arg0Parameter, tArg0);


			Expression<Action<TInstance, TArg0>> expression = Expression.Lambda<Action<TInstance, TArg0>>(
				Expression.Assign(Expression.Property(instanceTypeAs ?? instanceParameter, propertyInfo), arg0TypeAs ?? arg0Parameter),
				instanceParameter, arg0Parameter);
			return expression.Compile();
		}

		private static Type To<T>(Type type) {
			return type ?? typeof(T);
		}

		private static Expression Convert(Expression parameter, Type to) {
			if (to == null) return parameter;
			return to.IsClass ? Expression.TypeAs(parameter, to) : Expression.Convert(parameter, to);
		}
	}
}