CREATE function [Metadata].[fProperty](@MajorId int, @MinorId int) returns table as return
	select * from Metadata.vProperty p where
		p.MajorId = @MajorId and p.MinorId = @MinorId 
	