vstest.console /Logger:trx /TestAdapterPath:"packages\xunit.runner.visualstudio.2.0.0\build\_common" "IQ.Core.Data.Sql.Test\bin\Dx00\IQ.Core.Data.Sql.Test.dll" "IQ.Core.Data.Test\bin\Dx00\IQ.Core.Data.Test.dll"

REM vstest.console /TestCaseFilter:"TestCategory=Benchmark" /Logger:trx /TestAdapterPath:"packages\NUnitTestAdapter.2.0.0\lib" "IQ.Core.Framework.Test\bin\Debug\IQ.Core.Framework.Test.dll" "IQ.Core.Data.Sql.Test\bin\Debug\IQ.Core.Data.Sql.Test.dll" "IQ.Core.Data.Test\bin\Debug\IQ.Core.Data.Test.dll"

