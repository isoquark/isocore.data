CREATE VIEW [Metadata].[vViewColumn] as
select 
	c.*
from 
	Metadata.vView x 
	cross apply Metadata.fColumn(x.ViewId) c 





