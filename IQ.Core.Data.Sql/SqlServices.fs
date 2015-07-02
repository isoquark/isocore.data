namespace IQ.Core.Data.Sql

open IQ.Core.Framework

module SqlServices =
    let register(registry : ICompositionRegistry) =
        registry.RegisterInterfaces<SqlDataStore.Realization>()

