USE [Requisition]
GO
/****** Object:  StoredProcedure [dbo].[sp_InsertRequisition]    Script Date: 7/29/2025 6:02:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[sp_InsertRequisition]
    @RequisitionNumber NVARCHAR(50),
    @RequestedBy NVARCHAR(255),
    @Department NVARCHAR(255),
    @Purpose NVARCHAR(500),
    @Type INT,
    @Remarks NVARCHAR(1000) = NULL,
    @Items NVARCHAR(MAX),
    @RequisitionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Input validation
        IF @RequisitionNumber IS NULL OR LEN(TRIM(@RequisitionNumber)) = 0
        BEGIN
            RAISERROR('Requisition number is required', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        IF @RequestedBy IS NULL OR LEN(TRIM(@RequestedBy)) = 0
        BEGIN
            RAISERROR('Requested by is required', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @Department IS NULL OR LEN(TRIM(@Department)) = 0
        BEGIN
            RAISERROR('Department is required', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @Purpose IS NULL OR LEN(TRIM(@Purpose)) = 0
        BEGIN
            RAISERROR('Purpose is required', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @Items IS NULL OR LEN(TRIM(@Items)) = 0 OR @Items = '[]'
        BEGIN
            RAISERROR('At least one item is required', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF ISJSON(@Items) = 0
        BEGIN
            RAISERROR('Invalid JSON format for items', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert into Requisitions table
        INSERT INTO [dbo].[Requisitions] (
            RequisitionNumber, RequestDate, RequestedBy,
            Department, Purpose, Type, Status, Remarks
        )
        VALUES (
            @RequisitionNumber, GETDATE(), @RequestedBy,
            @Department, @Purpose, @Type, 1, @Remarks
        );

        SET @RequisitionId = SCOPE_IDENTITY();

        -- Validate item count
        DECLARE @ItemCount INT;
        SELECT @ItemCount = COUNT(*)
        FROM OPENJSON(@Items)
        WHERE JSON_VALUE(value, '$.ProductId') IS NOT NULL 
          AND CAST(JSON_VALUE(value, '$.ProductId') AS INT) > 0
          AND JSON_VALUE(value, '$.Quantity') IS NOT NULL 
          AND CAST(JSON_VALUE(value, '$.Quantity') AS INT) > 0;

        IF @ItemCount = 0
        BEGIN
            RAISERROR('No valid items found in the request', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert items
        INSERT INTO [dbo].[RequisitionItem] (
            RequisitionId, ProductId, Quantity, Purpose, Remarks
        )
        SELECT 
            @RequisitionId,
            CAST(JSON_VALUE(value, '$.ProductId') AS INT),
            CAST(JSON_VALUE(value, '$.Quantity') AS INT),
            ISNULL(JSON_VALUE(value, '$.Purpose'), ''),
            ISNULL(JSON_VALUE(value, '$.Remarks'), '')
        FROM OPENJSON(@Items)
        WHERE JSON_VALUE(value, '$.ProductId') IS NOT NULL 
          AND CAST(JSON_VALUE(value, '$.ProductId') AS INT) > 0
          AND JSON_VALUE(value, '$.Quantity') IS NOT NULL 
          AND CAST(JSON_VALUE(value, '$.Quantity') AS INT) > 0
          AND EXISTS (
              SELECT 1 
              FROM [dbo].[Product]
              WHERE ProductId = CAST(JSON_VALUE(value, '$.ProductId') AS INT)
                AND IsActive = 1
          );

        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('No items were inserted. Please check product IDs and quantities.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Insert approval steps
        INSERT INTO [dbo].[RequisitionApproval] (
            RequisitionId, Level, ApproverRole, Status
        )
        VALUES 
            (@RequisitionId, 1, 'Department Head', 1),
            (@RequisitionId, 2, 'Procurement Officer', 1);

        COMMIT TRANSACTION;

        SELECT @RequisitionId AS RequisitionId, 'Requisition created successfully' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END

