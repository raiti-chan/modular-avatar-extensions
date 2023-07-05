using System;

namespace raitichan.com.modular_avatar.extensions.ReflectionHelper {
	public class ErrorMessageCreator {
		private const string NOT_FOUND = "Not Found";
		private readonly string _className;

		public ErrorMessageCreator(string className) {
			this._className = className;
		}

		public ErrorMessageCreator(Type type) {
			this._className = type.FullName;
		}

		public string TypeMessage() {
			return $"{NOT_FOUND} : {this._className}";
		}

		public string ConstructorMessage() {
			return $"{NOT_FOUND} : {this._className}.ctor()";
		}
		
		public string ConstructorMessage(string arg0) {
			return $"{NOT_FOUND} : {this._className}.ctor({arg0})";
		}

		public string MethodMessage(string methodName, string retType) {
			return $"{NOT_FOUND} : {this._className}.{methodName}() : {retType ?? "void"}";
		}

		public string MethodMessage(string methodName, string arg0, string retType) {
			return $"{NOT_FOUND} : {this._className}.{methodName}({arg0}) : {retType ?? "void"}";
		}

		public string MethodMessage(string methodName, string arg0, string arg1, string retType) {
			return $"{NOT_FOUND} : {this._className}.{methodName}({arg0}, {arg1}) : {retType ?? "void"}";
		}

		public string MethodMessage(string methodName, string arg0, string arg1, string arg2, string retType) {
			return $"{NOT_FOUND} : {this._className}.{methodName}({arg0}, {arg1}, {arg2}) : {retType ?? "void"}";
		}
	}
}