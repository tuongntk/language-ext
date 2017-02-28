﻿using System;
using System.Diagnostics.Contracts;
using LanguageExt;
using static LanguageExt.Prelude;
using LanguageExt.ClassInstances;

namespace LanguageExt
{
    /// <summary>
    /// Represents the result of an operation:
    /// 
    ///     A | Exception
    /// 
    /// </summary>
    /// <typeparam name="A">Bound value type</typeparam>
    public struct Result<A> : IEquatable<Result<A>>
    {
        readonly bool IsValid;
        internal readonly A Value;
        internal Exception Exception;

        /// <summary>
        /// Constructor of a concrete value
        /// </summary>
        /// <param name="value"></param>
        [Pure]
        public Result(A value)
        {
            IsValid = true;
            Value = value;
            Exception = null;
        }

        /// <summary>
        /// Constructor of an error value
        /// </summary>
        /// <param name="e"></param>
        [Pure]
        public Result(Exception e)
        {
            IsValid = true;
            Exception = e;
            Value = default(A);
        }

        /// <summary>
        /// Implicit conversion operator from A to Result<A>
        /// </summary>
        /// <param name="value">Value</param>
        [Pure]
        public static implicit operator Result<A>(A value) =>
            new Result<A>(value);

        /// <summary>
        /// True if the result is faulted
        /// </summary>
        [Pure]
        public bool IsFaulted => Exception != null || IsBottom;

        /// <summary>
        /// True if the struct is in an invalid state
        /// </summary>
        [Pure]
        public bool IsBottom => !IsValid;

        /// <summary>
        /// Convert the value to a showable string
        /// </summary>
        [Pure]
        public override string ToString() =>
            IsFaulted
                ? Exception.ToString()
                : Value.ToString();

        /// <summary>
        /// Equality check
        /// </summary>
        public bool Equals(Result<A> other) =>
            IsBottom == other.IsBottom &&
            IsFaulted
                ? Exception == other.Exception
                : EqDefault<A>.Inst.Equals(Value, other.Value);

        /// <summary>
        /// Equality check
        /// </summary>
        public override bool Equals(object obj) =>
            obj is Result<A> && Equals((Result<A>)obj);

        /// <summary>
        /// Get hash code for bound value
        /// </summary>
        public override int GetHashCode()
        {
            if (IsBottom) return -1;
            if (IsFaulted) return -2;
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator==(Result<A> a, Result<A> b) =>
            a.Equals(b);

        public static bool operator !=(Result<A> a, Result<A> b) =>
            !(a==b);

        public readonly static Result<A> Bottom =
            new Result<A>(BottomException.Default);

        public A IfFail(A defaultValue) =>
            IsFaulted
                ? defaultValue
                : Value;

        public A IfFail(Func<Exception, A> f) =>
            IsFaulted
                ? f(Exception)
                : Value;

        public Unit IfFail(Action<Exception> f)
        {
            if (IsFaulted) f(Exception);
            return unit;
        }

        public Unit IfSucc(Action<A> f)
        {
            if (!IsFaulted) f(Value);
            return unit;
        }

        public R Match<R>(Func<A, R> Succ, Func<Exception, R> Fail) =>
            IsFaulted
                ? Fail(Exception)
                : Succ(Value);
    }
}