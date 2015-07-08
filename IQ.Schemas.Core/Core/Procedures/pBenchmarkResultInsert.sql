CREATE PROCEDURE Core.pBenchmarkResultPut(
	@BenchmarkName nvarchar(150),
	@DeclaringType nvarchar(250),
	@OpCount int,
	@MachineName nvarchar(50),
	@StartTime datetime2(7),
	@EndTime datetime2(7),
	@Duration int,
	@Id int output
	)
	
 as
	set @Id = next value for Core.BenchmarkResultSequence
	insert into Core.BenchmarkResult(Id, DeclaringType, OpCount, BenchmarkName, MachineName, StartTime, EndTime, Duration)
		values(@Id, @DeclaringType, @OpCount, @BenchmarkName, @MachineName, @StartTime, @EndTime, @Duration)			