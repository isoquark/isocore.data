create function Metadata.fColumn(@ParentId int) returns table as return
	select 
		c.*
	from 
		Metadata.vColumn c 
	where 
		c.ParentId = @ParentId
