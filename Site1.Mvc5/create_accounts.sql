-- Script Date: 09.08.2019 23:58  - ErikEJ.SqlCeScripting version 3.5.2.81
CREATE TABLE [UserProfiles] (
  [UserProfileId] int IDENTITY (1,1) NOT NULL
, [IdSrvId] nvarchar(100) NOT NULL
, [Login] nvarchar(256) NOT NULL
);
GO
ALTER TABLE [UserProfiles] ADD CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([UserProfileId]);
GO
ALTER TABLE [UserProfiles] ADD CONSTRAINT [UQ__UserProfiles__000000000000006B] UNIQUE ([IdSrvId]);
GO
