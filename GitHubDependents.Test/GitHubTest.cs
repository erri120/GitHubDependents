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
using Xunit;
using System.Linq;

namespace GitHubDependents.Test
{
    public class GitHubTest
    {
        [Fact]
        public async void TestGetDependents()
        {
            List<Dependent> list = await GitHubDependents.GetDependents("dotnet", "roslyn", "UGFja2FnZS0xNTY3NTE0NTM%3D", 2).ToListAsync();

            Assert.NotEmpty(list);
            //loading 2 pages but not every page has exactly 30 repos...
            Assert.True(list.Count > 45);
        }
    }
}
