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
using Xunit;

namespace IQ.Core.Data.Sql.Test.CSharp
{

    [Schema("SqlTest")]
    public interface ISqlTestRoutines
    {
        [Procedure]
        int pTable03Insert(Byte Col01, Int16 Col02, Int32 Col03, Int64 Col04);

    }


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
