CREATE VIEW [Metadata].[vTableFunctionParameter] as
select 
	p.*
from 
	Metadata.vTableFunction x 
	cross apply Metadata.fGetParameters(x.FunctionId) p 




