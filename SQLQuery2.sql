CREATE TABLE [dbo].[DestinationTable] (
    [ID]       INT NOT NULL identity(1,1),
    [SourceID] INT NULL,
    [Sum]      INT NULL,
    CONSTRAINT [PK_DestinationTable] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_DestinationTable_SourceTable] FOREIGN KEY ([SourceID]) REFERENCES [dbo].[SourceTable] ([ID])
);


GO
