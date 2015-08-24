create function Metadata.fGetParameters(@ParentId int) returns table as return
	select 
		x.*
	from 
		Metadata.vParameter x
	where
		x.ParentId = @ParentId
