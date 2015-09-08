CREATE function [Metadata].[fDescription](@MajorId int, @MinorId int) returns table as return
	select
		 p.* 
	from 
		Metadata.fProperty(@MajorId, @MinorId) p 
	where 
		p.PropertyName = 'MS_Description'
