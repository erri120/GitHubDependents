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
        /// Link to the avatar of the user/organization.
        /// </summary>
        public string? AvatarURL { get; set; }
        /// <summary>
        /// Name of the user/organization.
        /// </summary>
        public string? User { get; set; }
        /// <summary>
        /// Name of the repository.
        /// </summary>
        public string? Repository { get; set; }
        /// <summary>
        /// Amount of Stars the repository has.
        /// </summary>
        public int Stars { get; set; }
        /// <summary>
        /// Amount of Forks the repository has.
        /// </summary>
        public int Forks { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{User}/{Repository}";
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <param name="withDetails">Whether to append extra details (Amount of Stars and Forks) to the string.</param>
        /// <returns>String representation of the current object.</returns>
        public string ToString(bool withDetails)
        {
            return withDetails
                ? $"{ToString()}: {Stars} Stars, {Forks} forks"
                : ToString();
        }
    }

    [PublicAPI]
    public static class GitHubDependents
    {
        private static List<Dependent> FindDependents(HtmlNode boxNode)
        {
            var list = new List<Dependent>();

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

                var detailsNode = x.SelectSingleNode(x.XPath + "/div");
                var spanNodes = detailsNode?.SelectNodes(detailsNode.XPath + "/span");
                if (spanNodes != null && spanNodes.Count == 2)
                {
                    var starNode = spanNodes[0];
                    var forkNode = spanNodes[1];

                    var sStars = starNode.DecodeInnerText().Replace("\n", "").Trim().Replace(",", "");
                    var sForks = forkNode.DecodeInnerText().Replace("\n", "").Trim().Replace(",", "");

                    if (int.TryParse(sStars, out var stars))
                        dependent.Stars = stars;
                    if (int.TryParse(sForks, out var forks))
                        dependent.Forks = forks;
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

        /// <summary>
        /// Scrapes GitHub for Dependents of a specific repository.
        /// </summary>
        /// <param name="user">GitHub user or organization, eg: dotnet</param>
        /// <param name="repository">GitHub repository name, eg: roslyn</param>
        /// <param name="packageID">Optional package ID to use, eg: UGFja2FnZS0xNTY3MzEzNTc%3D</param>
        /// <param name="pages">Optional amount of pages to load.
        /// The function will not throw an exception if the provided amount of pages is higher than the actual amount of available pages.
        /// If you don't know the exact amount of pages you want to load but want to load all, simply pass 255.</param>
        /// <returns>List of all Dependents using the repository</returns>
        /// <exception cref="HtmlWebException">Thrown when unable to load the HTML Document</exception>
        /// <exception cref="NodeNotFoundException">Thrown when a node was not found</exception>
        public static async IAsyncEnumerable<Dependent> GetDependents(string user, string repository, string? packageID = null, byte pages = 1)
        {
            if (pages == 0)
                throw new ArgumentException("Can not load 0 pages!", nameof(pages));

            var web = new HtmlWeb();

            var url = $"https://github.com/{user}/{repository}/network/dependents";
            if (packageID != null)
                url = $"{url}?package_id={packageID}";

            var availablePages = 1;

            for (var i = 0; i < availablePages; i++)
            {
                var document = await web.LoadFromWebAsync(url);
                if (document == null)
                    throw new HtmlWebException($"Unable to load from {url}");

                var node = document.DocumentNode;

                var boxNode = node.SelectSingleNode("//div[@class='repository-content ']/div[@class='gutter-condensed gutter-lg d-flex']/div[@class='flex-shrink-0 col-9']/div[@id='dependents']/div[@class='Box']");
                if (boxNode == null)
                    throw new NodeNotFoundException("Unable to find Box Node!");

                if (pages != 1)
                {
                    var boxHeaderNode = boxNode.SelectSingleNode(boxNode.XPath + "/div[@class='Box-header clearfix']/div/div");
                    var repoLinkNode = boxHeaderNode?.SelectSingleNode(boxHeaderNode.XPath + "/a");
                    if (repoLinkNode != null)
                    {
                        var sRepositories = repoLinkNode.DecodeInnerText();
                        if (!sRepositories.IsEmpty())
                        {
                            sRepositories = sRepositories.Replace("\n", "").Trim().Replace("Repositories", "").Trim().Replace(",", "");
                            if (int.TryParse(sRepositories, out var repoCount))
                            {
                                availablePages = repoCount / 30;
                                if (availablePages > pages)
                                    availablePages = pages;
                            }
                        }
                    }
                }

                foreach (var dep in FindDependents(boxNode))
                {
                    yield return dep;
                }

                if (i + 1 == pages) break;

                var buttonGroupNode = node.SelectSingleNode("//div[@class='paginate-container']/div[@class='BtnGroup']");
                if (buttonGroupNode == null) break;

                var buttonNodes = buttonGroupNode.SelectNodes(buttonGroupNode.XPath + "/button");
                var buttonLinkNodes = buttonGroupNode.SelectNodes(buttonGroupNode.XPath + "/a");

                if (buttonLinkNodes == null || buttonLinkNodes.Count == 0) break;
                if (buttonNodes != null && buttonNodes.Count != 0)
                {
                    if (buttonNodes.Any(x => x.DecodeInnerText().Equals("Next", StringComparison.OrdinalIgnoreCase)))
                        break;
                }

                var nextLinkNode = buttonLinkNodes.FirstOrDefault(x => x.DecodeInnerText().Equals("Next", StringComparison.OrdinalIgnoreCase));
                var nextLink = nextLinkNode?.GetValue("href");
                if (nextLink == null) break;

                url = nextLink;
            }
        }
    }
}
