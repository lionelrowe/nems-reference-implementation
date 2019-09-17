using System;
using System.Collections.Generic;

namespace NEMS_API.Utilities
{
    public class TypeHelper
    {
        public static Type GetListType(Type type)
        {
            if (IsListType(type))
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        public static bool IsStringListType(Type type)
        {
            return GetListType(type) == typeof(string);
        }

        public static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }
    }
}
