-- =============================================
-- Script: Seed Master Data for Service Requests
-- Description: Insert VehicleType and VehicleName Master Data
-- =============================================

USE ASC;
GO

-- Insert Master Keys
IF NOT EXISTS (SELECT 1 FROM MasterDataKeys WHERE PartitionKey = 'VehicleType')
BEGIN
    INSERT INTO MasterDataKeys (RowKey, PartitionKey, Name, IsActive, IsDeleted, CreatedBy, UpdatedBy, CreatedDate, UpdatedDate)
    VALUES 
    (NEWID(), 'VehicleType', 'VehicleType', 1, 0, 'System', 'System', GETUTCDATE(), GETUTCDATE());
    PRINT '✅ Master Key "VehicleType" created';
END
ELSE
BEGIN
    PRINT '⚠️ Master Key "VehicleType" already exists';
END

IF NOT EXISTS (SELECT 1 FROM MasterDataKeys WHERE PartitionKey = 'VehicleName')
BEGIN
    INSERT INTO MasterDataKeys (RowKey, PartitionKey, Name, IsActive, IsDeleted, CreatedBy, UpdatedBy, CreatedDate, UpdatedDate)
    VALUES 
    (NEWID(), 'VehicleName', 'VehicleName', 1, 0, 'System', 'System', GETUTCDATE(), GETUTCDATE());
    PRINT '✅ Master Key "VehicleName" created';
END
ELSE
BEGIN
    PRINT '⚠️ Master Key "VehicleName" already exists';
END

-- Insert Master Values for VehicleType
PRINT '';
PRINT '--- Inserting VehicleType Values ---';

DECLARE @VehicleTypes TABLE (Name NVARCHAR(100));
INSERT INTO @VehicleTypes VALUES ('Car'), ('Truck'), ('SUV'), ('Motorcycle'), ('Van'), ('Bus');

DECLARE @TypeName NVARCHAR(100);
DECLARE type_cursor CURSOR FOR SELECT Name FROM @VehicleTypes;
OPEN type_cursor;
FETCH NEXT FROM type_cursor INTO @TypeName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM MasterDataValues WHERE PartitionKey = 'VehicleType' AND Name = @TypeName)
    BEGIN
        INSERT INTO MasterDataValues (RowKey, PartitionKey, Name, IsActive, IsDeleted, CreatedBy, UpdatedBy, CreatedDate, UpdatedDate)
        VALUES 
        (NEWID(), 'VehicleType', @TypeName, 1, 0, 'System', 'System', GETUTCDATE(), GETUTCDATE());
        PRINT '  ✅ VehicleType: ' + @TypeName;
    END
    ELSE
    BEGIN
        PRINT '  ⚠️ VehicleType already exists: ' + @TypeName;
    END
    
    FETCH NEXT FROM type_cursor INTO @TypeName;
END

CLOSE type_cursor;
DEALLOCATE type_cursor;

-- Insert Master Values for VehicleName
PRINT '';
PRINT '--- Inserting VehicleName Values ---';

DECLARE @VehicleNames TABLE (Name NVARCHAR(100));
INSERT INTO @VehicleNames VALUES 
    ('Toyota'), ('Honda'), ('Ford'), ('BMW'), ('Mercedes-Benz'), 
    ('Audi'), ('Volkswagen'), ('Nissan'), ('Hyundai'), ('Kia'),
    ('Mazda'), ('Chevrolet'), ('Tesla'), ('Lexus'), ('Subaru');

DECLARE @VehicleName NVARCHAR(100);
DECLARE name_cursor CURSOR FOR SELECT Name FROM @VehicleNames;
OPEN name_cursor;
FETCH NEXT FROM name_cursor INTO @VehicleName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM MasterDataValues WHERE PartitionKey = 'VehicleName' AND Name = @VehicleName)
    BEGIN
        INSERT INTO MasterDataValues (RowKey, PartitionKey, Name, IsActive, IsDeleted, CreatedBy, UpdatedBy, CreatedDate, UpdatedDate)
        VALUES 
        (NEWID(), 'VehicleName', @VehicleName, 1, 0, 'System', 'System', GETUTCDATE(), GETUTCDATE());
        PRINT '  ✅ VehicleName: ' + @VehicleName;
    END
    ELSE
    BEGIN
        PRINT '  ⚠️ VehicleName already exists: ' + @VehicleName;
    END
    
    FETCH NEXT FROM name_cursor INTO @VehicleName;
END

CLOSE name_cursor;
DEALLOCATE name_cursor;

PRINT '';
PRINT '========================================';
PRINT '✅ Master Data Seeding Completed!';
PRINT '========================================';
PRINT '';

-- Verify the data
PRINT '--- Verification ---';
SELECT 'Master Keys' AS [Type], COUNT(*) AS [Count] FROM MasterDataKeys WHERE IsDeleted = 0;
SELECT 'Master Values' AS [Type], COUNT(*) AS [Count] FROM MasterDataValues WHERE IsDeleted = 0;
PRINT '';
SELECT 'VehicleType Values' AS [Category], COUNT(*) AS [Count] FROM MasterDataValues WHERE PartitionKey = 'VehicleType' AND IsDeleted = 0;
SELECT 'VehicleName Values' AS [Category], COUNT(*) AS [Count] FROM MasterDataValues WHERE PartitionKey = 'VehicleName' AND IsDeleted = 0;

GO
