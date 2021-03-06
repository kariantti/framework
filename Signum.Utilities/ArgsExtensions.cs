﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Signum.Utilities
{
    public static class TypedArgsExtensions
    {
        public static T GetArg<T>(this object[] args)
        {
            return args.OfTypeOrEmpty<T>().SingleEx(() => "{0} in the argument list".Formato(typeof(T))); ;
        }

        public static T TryGetArgC<T>(this object[] args) where T : class
        {
            return args.OfTypeOrEmpty<T>().SingleOrDefaultEx(
                () => "There are more than one {0} in the argument list".Formato(typeof(T)));
        }

        public static T? TryGetArgS<T>(this object[] args) where T : struct
        {
            var casted = args.OfTypeOrEmpty<T>();

            if (casted.IsEmpty())
                return null;

            return casted.SingleEx(() => "{0} in the argument list".Formato(typeof(T)));
        }

        static IEnumerable<T> OfTypeOrEmpty<T>(this object[] args)
        {
            if (args == null)
                return Enumerable.Empty<T>();

            return args.OfType<T>();
        }
    }
}
