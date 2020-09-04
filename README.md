# GitHub Dependents

![.NET Core](https://github.com/erri120/GitHubDependents/workflows/.NET%20Core/badge.svg)

GitHub has this nice overview of all repositories that depend on your package, eg: [dotnet/roslyn](https://github.com/dotnet/roslyn/network/dependents?package_id=UGFja2FnZS0xNTY3MzEzNTc%3D). Only problem is that this data can not be querried using the API at the moment. This makeshift lib uses [Html Agility Pack](https://html-agility-pack.net/) to scrap the site for the information we need.
