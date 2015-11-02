using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IQ.Core.Framework;
using IQ.Core.Contracts;

using IQ.Core.Data;
using IQ.Core.Data.Contracts;

using IQ.Core.TestFramework;


using static IQ.Core.TestFramework.TestVocabulary;
using Assembly = System.Reflection.Assembly;

using Xunit;
using Xunit.Sdk;
using Xunit.Abstractions;

namespace IQ.Core.Data.Sql.Test.CSharp
{
    public interface IAppTestContext : IAppContext
    {
        IConfigurationManager ConfigurationManager { get; }
        ISqlDataStore DataStore { get; }
    }

    public class AppTestContext : IAppTestContext
    {

        private static void RegisterDependencies(ICompositionRegistry registry)
        {
            var provider = DataStoreProvider.Create(SqlDataStore.getProvider(), ExcelDataStore.getProvider(), CsvDataStore.getProvider());
            registry.RegisterInstance<IDataStoreProvider>(provider);
            
        }

        private readonly ICompositionRoot root;
        private readonly IAppContext appContext;
        private readonly IConfigurationManager configurationManager;
        private readonly ISqlDataStore dataStore;
        private readonly string cs;


        public AppTestContext(ICompositionRoot root)
        {
            this.root = root;
            this.appContext = root.CreateContext();
            this.configurationManager = appContext.Resolve<IConfigurationManager>();
            this.cs = configurationManager.GetValue("csSqlDataStore");
            var dsProvider = appContext.Resolve<IDataStoreProvider>();
            this.dataStore = dsProvider.GetDataStore<ISqlDataStore>(cs);
        }

        public AppTestContext()
            : this(CoreRegistration.composeWithAction(Assembly.GetExecutingAssembly(), RegisterDependencies))
        {

        }

        public string ConnectionString => cs;

        IConfigurationManager IAppTestContext.ConfigurationManager => configurationManager;

        ISqlDataStore IAppTestContext.DataStore => dataStore;

        T IAppContext.Resolve<T>() => appContext.Resolve<T>();

        T IAppContext.Resolve<T>(string key, object value) => appContext.Resolve<T>(key, value);

        I IAppContext.Resolve<C, I>(C config) => appContext.Resolve<C, I>(config);

        void IDisposable.Dispose()
        {
            appContext?.Dispose();
            root?.Dispose();
        }
    }


    [CollectionDefinition("Core SQL Tests - C#")]
    public abstract class TestCollectionMarker : TestCollection<AppTestContext>
    {

    }

    [Collection("Core SQL Tests - C#")]
    public abstract class UnitTest
    {
        private readonly IAppTestContext ctx;
        private readonly ITestOutputHelper log;
        public UnitTest(AppTestContext ctx, ITestOutputHelper log)
        {
            this.ctx = ctx;
            this.log = log;
        }

        public IAppTestContext Context => ctx;
        public ITestOutputHelper Log => log;

    }
}
