CREATE function [Metadata].[fColumnProperty](@MajorId int, @MinorId int) returns table as return
	select * from Metadata.vProperty p where
		--We use the fact that non-column objects, such as contraints, have a minor id of 0 
		p.MajorId = @MajorId and p.MinorId = @MinorId and p.ClassName = 'OBJECT_OR_COLUMN' and p.MinorId <> 0
	