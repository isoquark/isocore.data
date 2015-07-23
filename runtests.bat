vstest.console /Logger:trx /TestAdapterPath:"packages\xunit.runner.visualstudio.2.0.1\build\_common" "build\targets\x86\IQ.Core.Framework.Test.dll"

REM vstest.console /TestCaseFilter:"TestCategory=Benchmark" /Logger:trx /TestAdapterPath:"packages\NUnitTestAdapter.2.0.0\lib" "IQ.Core.Framework.Test\bin\Debug\IQ.Core.Framework.Test.dll" "IQ.Core.Data.Sql.Test\bin\Debug\IQ.Core.Data.Sql.Test.dll" "IQ.Core.Data.Test\bin\Debug\IQ.Core.Data.Test.dll"

