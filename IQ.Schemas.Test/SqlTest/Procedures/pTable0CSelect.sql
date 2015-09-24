create procedure [SqlTest].[pTable0CSelect](@TopCount int) as
	select top(@TopCount) 
		t.Col01,
		t.Col02,
		t.Col03
	from [SqlTest].Table0C t