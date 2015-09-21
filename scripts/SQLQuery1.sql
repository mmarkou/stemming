
UPDATE paper
SET AbstractWithoutQuotes=LEFT(AbstractWithoutQuotes, CHARINDEX('©',AbstractWithoutQuotes)-2) 
WHERE AbstractWithoutQuotes LIKE '%©%'

update dbo.paper
SET StemmedAbstract =LTRIM(RTRIM(StemmedAbstract))
 
