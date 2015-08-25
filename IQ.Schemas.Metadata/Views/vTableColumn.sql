CREATE VIEW [Metadata].[vTableColumn] as
select 
	c.*
from 
	Metadata.vTable x 
	cross apply Metadata.fGetColumns(x.TableId) c 




