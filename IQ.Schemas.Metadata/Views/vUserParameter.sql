CREATE VIEW [Metadata].[vUserParameter] as
	select 
		x.*
	from 
		Metadata.vParameter x 
		inner join Metadata.vUserObject o on o.ObjectId = x.ParentId
