// ---------------------------------------------------------------------------------------------------
// Modified version of CodeContracts: 
// Requires and Ensures only raise exceptions in DEBUG. Do nothing otherwise.
// ---------------------------------------------------------------------------------------------------
// CodeContracts
// Copyright(c) Microsoft Corporation
// All rights reserved.
// Licensed under the MIT license.
// ---------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace SeLoger.Contracts
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Delegate | AttributeTargets.Event
        | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false,
        Inherited = true)]
    public sealed class PureAttribute : Attribute
    {
    }

    public class ContractViolationException : Exception
    {
        public ContractViolationException(string message)
            : base(message)
        {
        }
    }

    public static class Contract
    {
        private const string REQUIRES_TYPE = "<REQUIRES>";
        private const string ENSURES_TYPE = "<ENSURES>";

        /// <summary>
        /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
        /// </summary>
        /// <param name="condition">Boolean expression representing the contract.</param>
        /// <remarks>
        /// This call must happen at the beginning of a method or property before any other code.
        /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
        /// </remarks>
        public static void Requires(
            Func<bool> condition, 
            string message = null, 
            [CallerMemberName] string callingMember = null, 
            [CallerLineNumber] int sourceLineNumber = 0)
        {
#if !DEBUG
            return;
#endif
            if (!condition())
            {
                throw new ContractViolationException(
                    FormatMessage(REQUIRES_TYPE, message, callingMember, sourceLineNumber));
            }
        }

        /// <summary>
        /// Specifies a public contract such that the expression <paramref name="condition"/> will be true when the enclosing method or property returns normally.
        /// </summary>
        /// <param name="condition">Boolean expression representing the contract.</param>
        /// <remarks>
        /// This call must happen at the end of a method or property after any other code.
        /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
        /// </remarks>
        public static void Ensures(
            Func<bool> condition, 
            string message = null, 
            [CallerMemberName] string callingMember = "", 
            [CallerLineNumber] int sourceLineNumber = 0)
        {
#if !DEBUG
            return;
#endif
            if (!condition())
            {
                throw new ContractViolationException(
                    FormatMessage(ENSURES_TYPE, message, callingMember, sourceLineNumber));
            }
        }

        /// <summary>
        /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
        /// for all integers starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.
        /// </summary>
        /// <param name="fromInclusive">First integer to pass to <paramref name="predicate"/>.</param>
        /// <param name="toExclusive">One greater than the last integer to pass to <paramref name="predicate"/>.</param>
        /// <param name="predicate">Function that is evaluated from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</param>
        /// <returns><c>true</c> if <paramref name="predicate"/> returns <c>true</c> for all integers 
        /// starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</returns>
        /// <seealso cref="System.Collections.Generic.List&lt;T&gt;.TrueForAll"/>      
        public static bool ForAll(int fromInclusive, int toExclusive, Predicate<int> predicate)
        {
#if !DEBUG
            return true;
#endif            
            if (fromInclusive > toExclusive)
                throw new ArgumentException("fromInclusive must be less than or equal to toExclusive");

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = fromInclusive; i < toExclusive; i++)
                if (!predicate(i)) return false;
            return true;
        }


        /// <summary>
        /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
        /// for all elements in the <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The collection from which elements will be drawn from to pass to <paramref name="predicate"/>.</param>
        /// <param name="predicate">Function that is evaluated on elements from <paramref name="collection"/>.</param>
        /// <returns><c>true</c> if and only if <paramref name="predicate"/> returns <c>true</c> for all elements in
        /// <paramref name="collection"/>.</returns>
        /// <seealso cref="System.Collections.Generic.List&lt;T&gt;.TrueForAll"/>
        public static bool ForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
#if !DEBUG
            return true;
#endif
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            
            foreach (T t in collection)
                if (!predicate(t)) return false;
            return true;
        }

        /// <summary>
        /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
        /// for any integer starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.
        /// </summary>
        /// <param name="fromInclusive">First integer to pass to <paramref name="predicate"/>.</param>
        /// <param name="toExclusive">One greater than the last integer to pass to <paramref name="predicate"/>.</param>
        /// <param name="predicate">Function that is evaluated from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</param>
        /// <returns><c>true</c> if <paramref name="predicate"/> returns <c>true</c> for any integer
        /// starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</returns>
        /// <seealso cref="System.Collections.Generic.List&lt;T&gt;.Exists"/>
        public static bool Exists(int fromInclusive, int toExclusive, Predicate<int> predicate)
        {
#if !DEBUG
            return true;
#endif
            if (fromInclusive > toExclusive)
                throw new ArgumentException("fromInclusive must be less than or equal to toExclusive");

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = fromInclusive; i < toExclusive; i++)
                if (predicate(i)) return true;
            return false;
        }

        /// <summary>
        /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
        /// for any element in the <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The collection from which elements will be drawn from to pass to <paramref name="predicate"/>.</param>
        /// <param name="predicate">Function that is evaluated on elements from <paramref name="collection"/>.</param>
        /// <returns><c>true</c> if and only if <paramref name="predicate"/> returns <c>true</c> for an element in
        /// <paramref name="collection"/>.</returns>
        /// <seealso cref="System.Collections.Generic.List&lt;T&gt;.Exists"/>
        public static bool Exists<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
#if !DEBUG
            return true;
#endif
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));            

            foreach (T t in collection)
                if (predicate(t)) return true;
            return false;
        }

        private static string FormatMessage(
            string contractType,
            string message,
            string callingMember,
            int sourceLineNumber)
        {
            StringBuilder sb = new StringBuilder($"{contractType} contract violation");
            if (callingMember != null)
            {
                sb.Append($" at {callingMember}");
            }

            if (sourceLineNumber != 0)
            {
                sb.Append($", line {sourceLineNumber}");
            }

            if (message != null)
            {
                sb.Append($": {message}");
            }

            return sb.ToString();
        }
    }
}
