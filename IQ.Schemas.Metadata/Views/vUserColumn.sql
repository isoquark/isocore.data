﻿create view Metadata.vUserColumn as
	select 
		x.*
	from 
		Metadata.vColumn x 
		inner join Metadata.vUserObject o on o.ObjectId = x.ParentId
		

