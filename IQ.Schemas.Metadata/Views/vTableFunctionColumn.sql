create view Metadata.vTableFunctionColumn as
select 
	c.*
from 
	Metadata.vTableFunction x 
	cross apply Metadata.fColumn(x.FunctionId) c 


