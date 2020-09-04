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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JetBrains.Annotations;

namespace GitHubDependents
{
    [PublicAPI]
    public class Dependent
    {
        /// <summary>
        /// Link to the avatar of the user/organization
        /// </summary>
        public string? AvatarURL { get; set; }
        /// <summary>
        /// Name of the user/organization
        /// </summary>
        public string? User { get; set; }
        /// <summary>
        /// Name of the repository
        /// </summary>
        public string? Repository { get; set; }
    }
    
    [PublicAPI]
    public static class GitHubDependents
    {
        /// <summary>
        /// Scrapes GitHub for Dependents of a specific repository.
        /// </summary>
        /// <param name="user">GitHub user or organization, eg: dotnet</param>
        /// <param name="repository">GitHub repository name, eg: roslyn</param>
        /// <param name="packageID">Optional package ID to use, eg: UGFja2FnZS0xNTY3MzEzNTc%3D</param>
        /// <returns>List of all Dependents using the repository</returns>
        /// <exception cref="HtmlWebException">Thrown when unable to load the HTML Document</exception>
        /// <exception cref="NodeNotFoundException">Thrown when a node was not found</exception>
        public static async Task<List<Dependent>> GetDependents(string user, string repository, string? packageID = null)
        {
            var list = new List<Dependent>();

            var web = new HtmlWeb();
            var url = $"https://github.com/{user}/{repository}/network/dependents";
            if (packageID != null)
                url = $"{url}?package_id={packageID}";
            
            var document = await web.LoadFromWebAsync(url);
            if(document == null)
                throw new HtmlWebException($"Unable to load from {url}");

            var node = document.DocumentNode;

            var boxNode = node.SelectSingleNode("//div[@class='repository-content ']/div[@class='gutter-condensed gutter-lg d-flex']/div[@class='flex-shrink-0 col-9']/div[@id='dependents']/div[@class='Box']");
            if(boxNode == null)
                throw new NodeNotFoundException("Unable to find Box Node!");

            var dependentNodes = boxNode.SelectNodes(boxNode.XPath + "/div[@class='Box-row d-flex flex-items-center']");
            if (dependentNodes == null || dependentNodes.Count == 0)
                return list;
            
            list.AddRange(dependentNodes.Select(x =>
            {
                var dependent = new Dependent();

                var imgNode = x.SelectSingleNode(x.XPath + "/img");
                if (imgNode != null)
                {
                    var imgSrc = imgNode.GetValue("src");
                    dependent.AvatarURL = imgSrc;
                }

                var spanNode = x.SelectSingleNode(x.XPath + "/span");

                var linkNodes = spanNode?.SelectNodes(spanNode.XPath + "/a");
                if (linkNodes == null || linkNodes.Count == 0) return null;
                if (linkNodes.Count != 2)
                    throw new Exception($"Span contains {linkNodes.Count} link nodes instead of 2!");

                var userNode = linkNodes[0];
                var repositoryNode = linkNodes[1];

                dependent.User = userNode.DecodeInnerText();
                dependent.Repository = repositoryNode.DecodeInnerText();

                return dependent;
            }).NotNull());
            
            return list;
        }
    }
}
