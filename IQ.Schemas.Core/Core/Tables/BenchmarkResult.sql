CREATE TABLE [Core].[BenchmarkResult]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [BenchmarkName] NVARCHAR(150) not null, 
    [MachineName] NCHAR(50) not null, 
    [StartTime] DATETIME2 not null, 
    [EndTime] DATETIME2 not null, 
    [Duration] INT not null

)
