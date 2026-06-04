USE RetailPulseDB;
GO

CREATE TABLE AuditLogs (
    AuditId      INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT            NULL REFERENCES Users(UserId),
    Username     NVARCHAR(100)  NOT NULL,
    Module       NVARCHAR(50)   NOT NULL,
    Action       NVARCHAR(50)   NOT NULL,
    EntityId     INT            NULL,
    EntityName   NVARCHAR(200)  NULL,
    OldValues    NVARCHAR(MAX)  NULL,
    NewValues    NVARCHAR(MAX)  NULL,
    IpAddress    NVARCHAR(50)   NULL,
    CreatedAt    DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt DESC);
CREATE INDEX IX_AuditLogs_Module    ON AuditLogs(Module);
CREATE INDEX IX_AuditLogs_UserId    ON AuditLogs(UserId);
GO