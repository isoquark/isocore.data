using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit.Abstractions;

using IQ.Core.Data;
using IQ.Core.Data.Sql;
using IQ.Core.Framework;

using static IQ.Core.Data.DataAttributes;
using static IQ.Core.Data.Test.TestProxies;
using Xunit;

namespace IQ.Core.Data.Sql.Test.CSharp
{


    public class RoutineTest : UnitTest        
    {
        public RoutineTest(AppTestContext ctx, ITestOutputHelper log)
            : base(ctx, log)
        {

        }

        [Fact]
        private void TestSqlData()
        {
            var store = Context.DataStore;
            var routines = store.GetContract<ISqlTestRoutines>();
            var result = routines.pTable03Insert(5, 10, 15, 20);
        }

    }
}
