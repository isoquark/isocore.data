create function Metadata.fParameterProperty(@MajorId int, @MinorId int) returns table as return
	select 
		p.* 
	from 
		Metadata.fProperty(@MajorId, @MinorId) p 
	where 
		p.ClassName = 'PARAMETER'