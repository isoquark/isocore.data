create function Metadata.fParameterDescription(@MajorId int, @MinorId int) returns table as return
	select 
		d.* 
	from 
		Metadata.fParameterProperty(@MajorId, @MinorId) p inner join  
		Metadata.fDescription(@MajorId, @MinorId) d on p.MajorId = d.MajorId and p.MinorId = p.MinorId
