﻿// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics.CodeAnalysis;

namespace Experimental.Agents.InternalUtilities.Type;

/// <summary>
/// Extensions methods for <see cref="Type"/>.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class TypeExtensions
{
    /// <summary>
    /// Tries to get the result type from a generic parameter.
    /// </summary>
    /// <param name="returnType">Return type.</param>
    /// <param name="resultType">The result type of the Nullable generic parameter.</param>
    /// <returns><c>true</c> if the result type was successfully retrieved; otherwise, <c>false</c>.</returns>
    /// TODO [@teresaqhoang]: Issue #4202 Cache Generic Types Extraction - Handlebars
    public static bool TryGetGenericResultType(this global::System.Type? returnType, out global::System.Type resultType)
    {
        resultType = typeof(object);
        if (returnType is null)
        {
            return false;
        }

        if (returnType.IsGenericType)
        {
            global::System.Type genericTypeDef = returnType.GetGenericTypeDefinition();

            if (genericTypeDef == typeof(Task<>)
                || genericTypeDef == typeof(Nullable<>)
                || genericTypeDef == typeof(ValueTask<>))
            {
                resultType = returnType.GetGenericArguments()[0];
            }
            else if (genericTypeDef == typeof(IEnumerable<>)
                || genericTypeDef == typeof(IList<>)
                || genericTypeDef == typeof(ICollection<>))
            {
                resultType = typeof(List<>).MakeGenericType(returnType.GetGenericArguments()[0]);
            }
            else if (genericTypeDef == typeof(IDictionary<,>))
            {
                global::System.Type[] genericArgs = returnType.GetGenericArguments();
                resultType = typeof(Dictionary<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
            }

            return true;
        }

        return false;
    }
}
