CREATE VIEW [Metadata].[vTableColumn] as
select 
	c.*
from 
	Metadata.vTable x 
	cross apply Metadata.fColumn(x.TableId) c 




