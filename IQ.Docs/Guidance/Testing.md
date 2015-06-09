#Testing Guidance

* Don't take direct dependencies on 3rd party unit testing libraries such as NUnit, xUnit or MSTest throughout 
your unit and/or integration test projects. Instead, create a library that isolates your tests from the 
the particular framework in use. This make changing unit test frameworks, should you need to do so, much
easier. 