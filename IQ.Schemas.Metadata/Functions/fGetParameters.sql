create function Metadata.fParameters(@ParentId int) returns table as return
	select 
		x.*
	from 
		Metadata.vParameter x
	where
		x.ParentId = @ParentId
