-- Script Date: 09.08.2019 23:55  - ErikEJ.SqlCeScripting version 3.5.2.81
CREATE TABLE [Clients] (
  [Id] uniqueidentifier DEFAULT (((NEWID()))) NOT NULL
, [Name] nvarchar(1024) NOT NULL
, [Uri] nvarchar(4000) NULL
, [Secret] nvarchar(1024) NOT NULL
, [IsBlocked] bit DEFAULT (((0))) NOT NULL
);
GO
ALTER TABLE [Clients] ADD CONSTRAINT [PK_Clients] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Clients] ADD CONSTRAINT [UQ__Clients__000000000000006B] UNIQUE ([Name]);
GO

-- Script Date: 09.08.2019 23:55  - ErikEJ.SqlCeScripting version 3.5.2.81
CREATE TABLE [Users] (
  [Id] uniqueidentifier DEFAULT ((((NEWID())))) NOT NULL
, [UserName] nvarchar(256) NOT NULL
, [PasswordHash] nvarchar(1024) NULL
, [PasswordSalt] nvarchar(1024) NULL
, [IsBlocked] bit DEFAULT (0) NOT NULL
);
GO
ALTER TABLE [Users] ADD CONSTRAINT [PK__Users__000000000000005A] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Users] ADD CONSTRAINT [UQ__Users__000000000000006B] UNIQUE ([UserName]);
GO
