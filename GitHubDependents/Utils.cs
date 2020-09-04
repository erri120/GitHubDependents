// /*
//     Copyright (C) 2020  erri120
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// */

using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;

namespace GitHubDependents
{
    internal static class Utils
    {
        internal static bool IsEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }
        
        internal static IEnumerable<T> NotNull<T>(this IEnumerable<T?> col) where T : class
        {
            return col.Where(x => x != null).Select(x => x!);
        }
        
        internal static string? GetValue(this HtmlNode node, string attr)
        {
            if (!node.HasAttributes)
                return null;
            var value = node.GetAttributeValue(attr, string.Empty);

            return value.IsEmpty() ? null : value;
        }

        internal static string DecodeInnerText(this HtmlNode? node)
        {
            return HttpUtility.HtmlDecode(node?.InnerText) ?? string.Empty;
        }
    }
}
