create function SqlTest.fTable04Before (@StartDate date) returns table as return
	select 
		[t].[Id], [t].[Code], [t].[StartDate], [t].[EndDate] 
	from 
		SqlTest.Table04 t where t.StartDate <= @StartDate