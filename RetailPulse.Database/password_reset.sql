USE RetailPulseDB;
GO

ALTER TABLE Users ADD 
    ResetToken     NVARCHAR(100) NULL,
    ResetTokenExpiry DATETIME2   NULL;
GO