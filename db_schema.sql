IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [MOM_Department] (
    [DepartmentID] int NOT NULL IDENTITY,
    [DepartmentName] nvarchar(100) NOT NULL,
    [Created] datetime2 NOT NULL,
    [Modified] datetime2 NOT NULL,
    CONSTRAINT [PK_MOM_Department] PRIMARY KEY ([DepartmentID])
);

CREATE TABLE [MOM_MeetingType] (
    [MeetingTypeID] int NOT NULL IDENTITY,
    [MeetingTypeName] nvarchar(100) NOT NULL,
    [Remarks] nvarchar(max) NULL,
    [Created] datetime2 NOT NULL,
    [Modified] datetime2 NOT NULL,
    CONSTRAINT [PK_MOM_MeetingType] PRIMARY KEY ([MeetingTypeID])
);

CREATE TABLE [MOM_MeetingVenue] (
    [MeetingVenueID] int NOT NULL IDENTITY,
    [MeetingVenueName] nvarchar(100) NOT NULL,
    [Created] datetime2 NOT NULL,
    [Modified] datetime2 NOT NULL,
    CONSTRAINT [PK_MOM_MeetingVenue] PRIMARY KEY ([MeetingVenueID])
);

CREATE TABLE [MOM_Staff] (
    [StaffID] int NOT NULL IDENTITY,
    [DepartmentID] int NOT NULL,
    [StaffName] nvarchar(50) NOT NULL,
    [MobileNo] nvarchar(20) NOT NULL,
    [EmailAddress] nvarchar(50) NOT NULL,
    [Remarks] nvarchar(250) NULL,
    [Created] datetime2 NOT NULL,
    [Modified] datetime2 NOT NULL,
    CONSTRAINT [PK_MOM_Staff] PRIMARY KEY ([StaffID]),
    CONSTRAINT [FK_MOM_Staff_MOM_Department_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [MOM_Department] ([DepartmentID]) ON DELETE CASCADE
);

CREATE TABLE [MOM_Meetings] (
    [MeetingID] int NOT NULL IDENTITY,
    [MeetingDate] datetime2 NOT NULL,
    [MeetingVenueID] int NOT NULL,
    [MeetingTypeID] int NOT NULL,
    [DepartmentID] int NOT NULL,
    [MeetingDescription] nvarchar(250) NULL,
    [DocumentPath] nvarchar(250) NULL,
    [Created] datetime2 NOT NULL,
    [Modified] datetime2 NOT NULL,
    [IsCancelled] bit NULL,
    [CancellationDateTime] datetime2 NULL,
    [CancellationReason] nvarchar(250) NULL,
    CONSTRAINT [PK_MOM_Meetings] PRIMARY KEY ([MeetingID]),
    CONSTRAINT [FK_MOM_Meetings_MOM_Department_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [MOM_Department] ([DepartmentID]) ON DELETE CASCADE,
    CONSTRAINT [FK_MOM_Meetings_MOM_MeetingType_MeetingTypeID] FOREIGN KEY ([MeetingTypeID]) REFERENCES [MOM_MeetingType] ([MeetingTypeID]) ON DELETE CASCADE,
    CONSTRAINT [FK_MOM_Meetings_MOM_MeetingVenue_MeetingVenueID] FOREIGN KEY ([MeetingVenueID]) REFERENCES [MOM_MeetingVenue] ([MeetingVenueID]) ON DELETE CASCADE
);

CREATE TABLE [MOM_MeetingMember] (
    [MeetingMemberID] int NOT NULL IDENTITY,
    [MeetingID] int NOT NULL,
    [StaffID] int NOT NULL,
    [IsPresent] bit NOT NULL,
    [Remarks] nvarchar(250) NULL,
    [Created] datetime2 NOT NULL,
    [Modified] datetime2 NOT NULL,
    CONSTRAINT [PK_MOM_MeetingMember] PRIMARY KEY ([MeetingMemberID]),
    CONSTRAINT [FK_MOM_MeetingMember_MOM_Meetings_MeetingID] FOREIGN KEY ([MeetingID]) REFERENCES [MOM_Meetings] ([MeetingID]) ON DELETE CASCADE,
    CONSTRAINT [FK_MOM_MeetingMember_MOM_Staff_StaffID] FOREIGN KEY ([StaffID]) REFERENCES [MOM_Staff] ([StaffID]) ON DELETE CASCADE
);

CREATE INDEX [IX_MOM_MeetingMember_MeetingID] ON [MOM_MeetingMember] ([MeetingID]);

CREATE INDEX [IX_MOM_MeetingMember_StaffID] ON [MOM_MeetingMember] ([StaffID]);

CREATE INDEX [IX_MOM_Meetings_DepartmentID] ON [MOM_Meetings] ([DepartmentID]);

CREATE INDEX [IX_MOM_Meetings_MeetingTypeID] ON [MOM_Meetings] ([MeetingTypeID]);

CREATE INDEX [IX_MOM_Meetings_MeetingVenueID] ON [MOM_Meetings] ([MeetingVenueID]);

CREATE INDEX [IX_MOM_Staff_DepartmentID] ON [MOM_Staff] ([DepartmentID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260120072443_InitialCreate', N'10.0.1');

COMMIT;
GO

