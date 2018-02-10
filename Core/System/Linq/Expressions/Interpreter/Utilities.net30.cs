#if NET20 || NET30

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using Theraot.Collections;
using Theraot.Core;

namespace System.Linq.Expressions.Interpreter
{
    internal static class DelegateHelpers
    {
        private const int _maximumArity = 17;

        internal static Type MakeDelegate(Type[] types)
        {
            Debug.Assert(types != null && types.Length > 0);

            // Can only used predefined delegates if we have no byref types and
            // the arity is small enough to fit in Func<...> or Action<...>
            if (types.Length > _maximumArity || types.Any(t => t.IsByRef))
            {
                throw Assert.Unreachable;
            }

            var returnType = types[types.Length - 1];
            if (returnType == typeof(void))
            {
                Array.Resize(ref types, types.Length - 1);
                switch (types.Length)
                {
                    case 0:
                        return typeof(Action);

                    case 1:
                        return typeof(Action<>).MakeGenericType(types);

                    case 2:
                        return typeof(Action<,>).MakeGenericType(types);

                    case 3:
                        return typeof(Action<,,>).MakeGenericType(types);

                    case 4:
                        return typeof(Action<,,,>).MakeGenericType(types);

                    case 5:
                        return typeof(Action<,,,,>).MakeGenericType(types);

                    case 6:
                        return typeof(Action<,,,,,>).MakeGenericType(types);

                    case 7:
                        return typeof(Action<,,,,,,>).MakeGenericType(types);

                    case 8:
                        return typeof(Action<,,,,,,,>).MakeGenericType(types);

                    case 9:
                        return typeof(Action<,,,,,,,,>).MakeGenericType(types);

                    case 10:
                        return typeof(Action<,,,,,,,,,>).MakeGenericType(types);

                    case 11:
                        return typeof(Action<,,,,,,,,,,>).MakeGenericType(types);

                    case 12:
                        return typeof(Action<,,,,,,,,,,,>).MakeGenericType(types);

                    case 13:
                        return typeof(Action<,,,,,,,,,,,,>).MakeGenericType(types);

                    case 14:
                        return typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(types);

                    case 15:
                        return typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(types);

                    case 16:
                        return typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                }
            }
            else
            {
                switch (types.Length)
                {
                    case 1:
                        return typeof(Func<>).MakeGenericType(types);

                    case 2:
                        return typeof(Func<,>).MakeGenericType(types);

                    case 3:
                        return typeof(Func<,,>).MakeGenericType(types);

                    case 4:
                        return typeof(Func<,,,>).MakeGenericType(types);

                    case 5:
                        return typeof(Func<,,,,>).MakeGenericType(types);

                    case 6:
                        return typeof(Func<,,,,,>).MakeGenericType(types);

                    case 7:
                        return typeof(Func<,,,,,,>).MakeGenericType(types);

                    case 8:
                        return typeof(Func<,,,,,,,>).MakeGenericType(types);

                    case 9:
                        return typeof(Func<,,,,,,,,>).MakeGenericType(types);

                    case 10:
                        return typeof(Func<,,,,,,,,,>).MakeGenericType(types);

                    case 11:
                        return typeof(Func<,,,,,,,,,,>).MakeGenericType(types);

                    case 12:
                        return typeof(Func<,,,,,,,,,,,>).MakeGenericType(types);

                    case 13:
                        return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(types);

                    case 14:
                        return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(types);

                    case 15:
                        return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(types);

                    case 16:
                        return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(types);

                    case 17:
                        return typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                }
            }
            throw Assert.Unreachable;
        }
    }

    internal static class ScriptingRuntimeHelpers
    {
        public static object Int32ToObject(int i)
        {
            return i;
        }

        public static object BooleanToObject(bool b)
        {
            return b ? True : False;
        }

        internal static object True = true;
        internal static object False = false;

        internal static object GetPrimitiveDefaultValue(Type type)
        {
            switch (type.GetTypeCode())
            {
                case TypeCode.Boolean:
                    return False;

                case TypeCode.SByte:
                    return default(sbyte);

                case TypeCode.Byte:
                    return default(byte);

                case TypeCode.Char:
                    return default(char);

                case TypeCode.Int16:
                    return default(short);

                case TypeCode.Int32:
                    return Int32ToObject(0);

                case TypeCode.Int64:
                    return default(long);

                case TypeCode.UInt16:
                    return default(ushort);

                case TypeCode.UInt32:
                    return default(uint);

                case TypeCode.UInt64:
                    return default(ulong);

                case TypeCode.Single:
                    return default(float);

                case TypeCode.Double:
                    return default(double);
                //            case TypeCode.DBNull: return default(DBNull);
                case TypeCode.DateTime:
                    return default(DateTime);

                case TypeCode.Decimal:
                    return default(decimal);

                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Wraps all arguments passed to a dynamic site with more arguments than can be accepted by a Func/Action delegate.
    /// The binder generating a rule for such a site should unwrap the arguments first and then perform a binding to them.
    /// </summary>
    internal sealed class ArgumentArray
    {
        private readonly object[] _arguments;

        // the index of the first item _arguments that represents an argument:
        private readonly int _first;

        // the number of items in _arguments that represent the arguments:
        private readonly int _count;

        internal ArgumentArray(object[] arguments, int first, int count)
        {
            _arguments = arguments;
            _first = first;
            _count = count;
        }

        public int Count
        {
            get { return _count; }
        }

        public object GetArgument(int index)
        {
            return _arguments[_first + index];
        }

        public static object GetArg(ArgumentArray array, int index)
        {
            return array._arguments[array._first + index];
        }
    }

    internal static class ExceptionHelpers
    {
        private const string _prevStackTraces = "PreviousStackTraces"; // TODO not used

        public static Exception UpdateForRethrow(Exception rethrow)
        {
#if FEATURE_STACK_TRACES
            List<StackTrace> prev;

            // we don't have any dynamic stack trace data, capture the data we can
            // from the raw exception object.
            StackTrace st = new StackTrace(rethrow, true);

            if (!TryGetAssociatedStackTraces(rethrow, out prev))
            {
                prev = new List<StackTrace>();
                AssociateStackTraces(rethrow, prev);
            }

            prev.Add(st);

#endif // FEATURE_STACK_TRACES
            return rethrow;
        }

#if FEATURE_STACK_TRACES
        /// <summary>
        /// Returns all the stack traces associates with an exception
        /// </summary>
        public static IList<StackTrace> GetExceptionStackTraces(Exception rethrow)
        {
            List<StackTrace> result;
            return TryGetAssociatedStackTraces(rethrow, out result) ? result : null;
        }

        private static void AssociateStackTraces(Exception e, List<StackTrace> traces)
        {
            e.Data[prevStackTraces] = traces;
        }

        private static bool TryGetAssociatedStackTraces(Exception e, out List<StackTrace> traces)
        {
            traces = e.Data[prevStackTraces] as List<StackTrace>;
            return traces != null;
        }
#endif // FEATURE_STACK_TRACES
    }

    /// <summary>
    /// A hybrid dictionary which compares based upon object identity.
    /// </summary>
    internal class HybridReferenceDictionary<TKey, TValue> where TKey : class
    {
        private KeyValuePair<TKey, TValue>[] _keysAndValues;
        private Dictionary<TKey, TValue> _dict;
        private int _count;
        private const int _arraySize = 10;

        public HybridReferenceDictionary()
        {
        }

        public HybridReferenceDictionary(int initialCapicity)
        {
            if (initialCapicity > _arraySize)
            {
                _dict = new Dictionary<TKey, TValue>(initialCapicity);
            }
            else
            {
                _keysAndValues = new KeyValuePair<TKey, TValue>[initialCapicity];
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Debug.Assert(key != null);

            if (_dict != null)
            {
                return _dict.TryGetValue(key, out value);
            }
            if (_keysAndValues != null)
            {
                for (var i = 0; i < _keysAndValues.Length; i++)
                {
                    if (_keysAndValues[i].Key == key)
                    {
                        value = _keysAndValues[i].Value;
                        return true;
                    }
                }
            }
            value = default(TValue);
            return false;
        }

        public bool Remove(TKey key)
        {
            Debug.Assert(key != null);

            if (_dict != null)
            {
                return _dict.Remove(key);
            }
            if (_keysAndValues != null)
            {
                for (var i = 0; i < _keysAndValues.Length; i++)
                {
                    if (_keysAndValues[i].Key == key)
                    {
                        _keysAndValues[i] = new KeyValuePair<TKey, TValue>();
                        _count--;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ContainsKey(TKey key)
        {
            Debug.Assert(key != null);

            if (_dict != null)
            {
                return _dict.ContainsKey(key);
            }
            if (_keysAndValues != null)
            {
                for (var i = 0; i < _keysAndValues.Length; i++)
                {
                    if (_keysAndValues[i].Key == key)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int Count
        {
            get
            {
                if (_dict != null)
                {
                    return _dict.Count;
                }
                return _count;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (_dict != null)
            {
                return _dict.GetEnumerator();
            }

            return GetEnumeratorWorker();
        }

        private IEnumerator<KeyValuePair<TKey, TValue>> GetEnumeratorWorker()
        {
            if (_keysAndValues != null)
            {
                for (var i = 0; i < _keysAndValues.Length; i++)
                {
                    if (_keysAndValues[i].Key != null)
                    {
                        yield return _keysAndValues[i];
                    }
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                Debug.Assert(key != null);

                TValue res;
                if (TryGetValue(key, out res))
                {
                    return res;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                Debug.Assert(key != null);

                if (_dict != null)
                {
                    _dict[key] = value;
                }
                else
                {
                    int index;
                    if (_keysAndValues != null)
                    {
                        index = -1;
                        for (var i = 0; i < _keysAndValues.Length; i++)
                        {
                            if (_keysAndValues[i].Key == key)
                            {
                                _keysAndValues[i] = new KeyValuePair<TKey, TValue>(key, value);
                                return;
                            }
                            if (_keysAndValues[i].Key == null)
                            {
                                index = i;
                            }
                        }
                    }
                    else
                    {
                        _keysAndValues = new KeyValuePair<TKey, TValue>[_arraySize];
                        index = 0;
                    }

                    if (index != -1)
                    {
                        _count++;
                        _keysAndValues[index] = new KeyValuePair<TKey, TValue>(key, value);
                    }
                    else
                    {
                        _dict = new Dictionary<TKey, TValue>();
                        for (var i = 0; i < _keysAndValues.Length; i++)
                        {
                            _dict[_keysAndValues[i].Key] = _keysAndValues[i].Value;
                        }
                        _keysAndValues = null;

                        _dict[key] = value;
                    }
                }
            }
        }
    }

    internal static class Assert
    {
        internal static Exception Unreachable
        {
            get
            {
                Debug.Assert(false, "Unreachable");
                return new InvalidOperationException("Code supposed to be unreachable");
            }
        }

        [Conditional("DEBUG")]
        public static void NotNull(object var)
        {
            Debug.Assert(var != null);
        }

        [Conditional("DEBUG")]
        public static void NotNull(object var1, object var2)
        {
            Debug.Assert(var1 != null && var2 != null);
        }

        [Conditional("DEBUG")]
        public static void NotNull(object var1, object var2, object var3)
        {
            Debug.Assert(var1 != null && var2 != null && var3 != null);
        }

        [Conditional("DEBUG")]
        public static void NotNullItems<T>(IEnumerable<T> items) where T : class
        {
            Debug.Assert(items != null);
            foreach (object item in items)
            {
                Debug.Assert(item != null);
            }
        }

        [Conditional("DEBUG")]
        public static void NotEmpty(string str)
        {
            Debug.Assert(!string.IsNullOrEmpty(str));
        }
    }

    [Flags]
    internal enum ExpressionAccess
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write,
    }

    internal static class Utils
    {
        private static readonly DefaultExpression _voidInstance = Expression.Empty();

        public static DefaultExpression Empty()
        {
            return _voidInstance;
        }
    }

    internal sealed class ListEqualityComparer<T> : EqualityComparer<ICollection<T>>
    {
        internal static readonly ListEqualityComparer<T> Instance = new ListEqualityComparer<T>();

        private ListEqualityComparer()
        {
        }

        // EqualityComparer<T> handles null and object identity for us
        public override bool Equals(ICollection<T> x, ICollection<T> y)
        {
            return x.ListEquals(y);
        }

        public override int GetHashCode(ICollection<T> obj)
        {
            return ListHashCode(obj);
        }

        // We could probably improve the hashing here
        private static int ListHashCode(IEnumerable<T> list)
        {
            var cmp = EqualityComparer<T>.Default;
            var h = 6551;
            foreach (var t in list)
            {
                h ^= (h << 5) ^ cmp.GetHashCode(t);
            }
            return h;
        }
    }
}

#endif