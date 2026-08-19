USE pizzatowndha;

-- =============================================
-- CREATE UNITS TABLE
-- =============================================
CREATE TABLE Units (
    Id VARCHAR(36) PRIMARY KEY NOT NULL,
    UnitSymbol VARCHAR(20) NOT NULL,
    UnitName VARCHAR(50) NOT NULL UNIQUE,
    Category VARCHAR(30) NOT NULL,
    IsBaseUnit TINYINT NOT NULL DEFAULT 0,
    ConversionFactor DECIMAL(20,10) NOT NULL,
    DisplayOrder INT NOT NULL,
    CreatedBy VARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy VARCHAR(50) DEFAULT NULL,
    UpdatedAt DATETIME DEFAULT NULL,
    IsDeleted TINYINT DEFAULT 0
);