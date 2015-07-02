CREATE PROCEDURE Core.pBenchmarkResultPut(
	@BenchmarkName nvarchar(150),
	@MachineName nvarchar(50),
	@StartTime datetime2(7),
	@EndTime datetime2(7),
	@Duration int,
	@Id int output
	)
	
 as
	set @Id = next value for Core.BenchmarkResultSequence
	insert into Core.BenchmarkResult(Id,BenchmarkName, MachineName, StartTime, EndTime, Duration)
		values(@Id, @BenchmarkName, @MachineName, @StartTime, @EndTime, @Duration)			