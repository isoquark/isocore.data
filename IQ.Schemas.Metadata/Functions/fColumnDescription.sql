CREATE function [Metadata].[fColumnDescription](@MajorId int, @MinorId int) returns table as return
	select
		 p.* 
	from 
		Metadata.fColumnProperty(@MajorId, @MinorId) p 
	where 
		p.PropertyName = 'MS_Description'
