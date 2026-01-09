IF DB_ID('BooksFavorites') IS NULL
BEGIN
    CREATE DATABASE BooksFavorites;
END
GO

USE BooksFavorites;
GO

-- Limpieza para re-ejecutar sin dolor
IF OBJECT_ID('dbo.Favorites', 'U') IS NOT NULL DROP TABLE dbo.Favorites;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE dbo.Favorites (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Favorites PRIMARY KEY,
    UserId INT NOT NULL,
    ExternalId NVARCHAR(200) NOT NULL,     -- Ej: /works/OL45804W
    Title NVARCHAR(500) NOT NULL,
    Authors NVARCHAR(2000) NOT NULL,       -- CSV o JSON simple
    FirstPublishYear INT NULL,
    CoverId INT NULL,                      -- cover_i (recomendado)
    CoverUrl NVARCHAR(500) NULL,           -- opcional (si prefieres guardar URL)
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Favorites_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Favorites_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

-- Anti-duplicados por usuario
CREATE UNIQUE INDEX UX_Favorites_User_External
ON dbo.Favorites(UserId, ExternalId);
GO

-- Seed: usuario fijo Id=1 (para simplificar la prueba)
INSERT INTO dbo.Users (Name) VALUES ('Default User');
GO
