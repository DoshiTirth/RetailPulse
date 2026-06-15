USE RetailPulseDB;
GO

CREATE TABLE DashboardWidgets (
    WidgetId    INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT           NOT NULL REFERENCES Users(UserId),
    WidgetKey   NVARCHAR(50)  NOT NULL,
    IsVisible   BIT           NOT NULL DEFAULT 1,
    SortOrder   INT           NOT NULL DEFAULT 0,
    UNIQUE(UserId, WidgetKey)
);
GO