
LogLig uses Database first principle of EntityFramework
Please have a read: https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/database-first-development/changing-the-database


!!!I M P O R T A N T!!!
DO NOT MAKE ANY MANUAL CHANGES INSIDE THIS PROJECT
Please.


IF YOU ARE CREATING NEW TABLE:
1. Make sure to set Id column as "Identity column"(in properties of table designer). 
	Please do not name Id columns like "ActivityId" or "AnythingId", it should be just "Id"
2. Make sure to set Id column as Primary key - Right mouse button on column in table designer -> "Set Primary Key"
3. Make sure to set all possible relations of columns:
	For example:
		You created table "Blablabla"
		You added column "UnionId"
		This column should have relationship with Unions table
		To create relation, right click anywhere in table designer -> "Relationships..."
		Click "Add"
		On the right side: click on "Tables And Columns Specification", then click small "..." button
		In new window:
			On the left side select in dropdown: "Unions"
			In a cell below that dropdown select Id column of Unions table: "UnionId"
			On a right side, in same row with "UnionId" select also "UnionId" of your "Blablabla" table
		Click OK
		Click Close
		Save your table to database

		
HOW TO UPDATE APPLICATION MODELS FROM DATABASE:
1. First of all open "App.config", find connection string "DataEntities". 
	Scroll a bit to the right, find "data source=something\something". Set data source to you local database, for example "data source=localhost"
	Save the file
2. Open "DataModel.edmx"
3. Right click anywhere in designer, select "Update Model from Database"
4. In new window:
	If you added new table to the database, expand "Tables" list and select that table
	If you just added new columns to existing table, you don't need to select anything, just proceed
5. Click "Finish"
	VisualStudio can hang up for some time 
6. After that, left click anywhere in designer and save the file
	Two warning windows will appear, click OK in both
7. Close "DataModel.edmx" file tab
8. Just in case, in visual studio go to "File->Save All""

If everything is OK, the code classes should be created/updated and ready to use in code.