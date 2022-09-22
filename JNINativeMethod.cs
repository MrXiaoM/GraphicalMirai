using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GraphicalMirai
{
    /// <summary>
    /// https://github.com/NPavie/jnet/blob/master/src/JNINativeMethod.cs
    /// </summary>
    public struct JNINativeMethod : IDisposable
    {
        public IntPtr name; //char* 
        public IntPtr signature; //char* 
        public IntPtr fnPtr; //void* 

        public static JNINativeMethod CreateNativeMethod(string javaName, string javaSignature, IntPtr funcPointer)
        {
            JNINativeMethod meth;

            meth.fnPtr = funcPointer;
            meth.name = Marshal.StringToHGlobalAnsi(javaName);
            meth.signature = Marshal.StringToHGlobalAnsi(javaSignature);
            return meth;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (name != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(name);
                name = IntPtr.Zero;
            }
            if (signature != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(signature);
                signature = IntPtr.Zero;
            }
            //if (fnPtr != IntPtr.Zero)
            //{
            //    Marshal.FreeHGlobal(fnPtr);
            //    fnPtr = IntPtr.Zero;
            //}
        }
    }

    public class JavaNativeMethod
    {
        public static List<JNINativeMethod> Entries = new List<JNINativeMethod>();

        public static JNINativeMethod CreateNativeMethod(Type type, string javaName, string clrName, string javaSignature)
        {
            MethodInfo? methodInfo = type.GetMethod(clrName, BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
            {
                throw new Exception("Can't find type " + type.Name + "." + clrName);
            }
            return JNINativeMethod.CreateNativeMethod(javaName, javaSignature, Marshal.GetFunctionPointerForDelegate(Delegate.CreateDelegate(GetDelegateType(methodInfo), methodInfo)));
        }


        public static void CreateType(Type type)
        {
            MethodInfo[] methodInfoArray = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            for (int i = 0; i < methodInfoArray.Length; i++)
            {
                MethodInfo methodInfo = methodInfoArray[i];
                {
                    string signature = GetDelegateSignature(methodInfo);
                    Entries.Add(JNINativeMethod.CreateNativeMethod(methodInfo.Name, signature,
                                        Marshal.GetFunctionPointerForDelegate(Delegate.CreateDelegate(GetDelegateType(methodInfo), methodInfo))));
                }
            }
        }


        private static string GetDelegateSignature(MethodInfo method)
        {
            string ParamSig = "";
            var MethodParam = from MethodName in method.GetParameters() select MethodName;
            foreach (ParameterInfo pi in MethodParam)
            {
                ParamSig = ParamSig + ", " + pi.ParameterType + " " + pi.Name;
            }
            return ParamSig.Substring(1);
        }

        private static Type GetDelegateType(MethodInfo method)
        {
            List<Type> args = new List<Type>(method.GetParameters().Select(p => p.ParameterType));

            Type delegateType;
            if (method.ReturnType == typeof(void))
            {
                delegateType = Expression.GetActionType(args.ToArray());
            }
            else
            {
                args.Add(method.ReturnType);
                delegateType = Expression.GetFuncType(args.ToArray());
            }
            return delegateType;
        }
    }
}
