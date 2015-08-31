using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

using static IQ.Core.Data.Test.TestProxies;
using IQ.Core.Data.Contracts;


namespace IQ.Core.Data.Sql.Test.CSharp
{


    public static class SqlDataStoreExtensions
    {
        public static TableDescription DescribeNamedTable(
            this ISqlMetadataProvider subject, string schemaName, string tableName) =>
                subject.DescribeTable(DataObjectName.NewDataObjectName(schemaName, tableName));
    }


    public class SqlMetadataProviderTest : UnitTest
    {
        public SqlMetadataProviderTest(AppTestContext ctx, ITestOutputHelper log)
            : base(ctx, log)
        {
        }

        [Fact(DisplayName = "Discovered tables - C#")]
        private void DiscoveredTables()
        {
            var store = Context.DataStore;
            var metadata = store.MetadataProvider;
            var t0 = metadata.DescribeNamedTable(SchemaNames.SqlTest, TableNames.Table01);
        }

    }
}
