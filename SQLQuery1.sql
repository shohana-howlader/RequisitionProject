-- =============================================
-- 1. Insert Product Stored Procedure
-- =============================================
CREATE PROCEDURE sp_InsertProduct
    @Name NVARCHAR(255),
    @Description NVARCHAR(500) = NULL,
    @Unit NVARCHAR(50),
    @UnitPrice DECIMAL(18,2),
    @IsActive BIT = 1,
    @ProductId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO Product (Name, Description, Unit, UnitPrice, IsActive)
        VALUES (@Name, @Description, @Unit, @UnitPrice, @IsActive);
        
        SET @ProductId = SCOPE_IDENTITY();
        
        SELECT @ProductId as ProductId, 'Product inserted successfully' as Message;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- =============================================
-- 2. Update Product Stored Procedure
-- =============================================
CREATE PROCEDURE sp_UpdateProduct
    @ProductId INT,
    @Name NVARCHAR(255),
    @Description NVARCHAR(500) = NULL,
    @Unit NVARCHAR(50),
    @UnitPrice DECIMAL(18,2),
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Product WHERE ProductId = @ProductId)
        BEGIN
            RAISERROR('Product not found', 16, 1);
            RETURN;
        END
        
        UPDATE Product 
        SET Name = @Name,
            Description = @Description,
            Unit = @Unit,
            UnitPrice = @UnitPrice,
            IsActive = @IsActive
        WHERE ProductId = @ProductId;
        
        SELECT 'Product updated successfully' as Message;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- =============================================
-- 3. Insert Requisition with Items Stored Procedure
-- =============================================
CREATE PROCEDURE sp_InsertRequisition
    @RequisitionNumber NVARCHAR(50),
    @RequestedBy NVARCHAR(255),
    @Department NVARCHAR(255),
    @Purpose NVARCHAR(500),
    @Type INT, -- RequisitionType enum value
    @Remarks NVARCHAR(1000) = NULL,
    @Items NVARCHAR(MAX), -- JSON string of items
    @RequisitionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Insert Requisition
        INSERT INTO Requisition (
            RequisitionNumber, 
            RequestDate, 
            RequestedBy, 
            Department, 
            Purpose, 
            Type, 
            Status, 
            Remarks
        )
        VALUES (
            @RequisitionNumber,
            GETDATE(),
            @RequestedBy,
            @Department,
            @Purpose,
            @Type,
            1, -- Draft status
            @Remarks
        );
        
        SET @RequisitionId = SCOPE_IDENTITY();
        
        -- Parse and insert requisition items from JSON
        INSERT INTO RequisitionItem (RequisitionId, ProductId, Quantity, Purpose, Remarks)
        SELECT 
            @RequisitionId,
            JSON_VALUE(value, '$.ProductId'),
            JSON_VALUE(value, '$.Quantity'),
            JSON_VALUE(value, '$.Purpose'),
            JSON_VALUE(value, '$.Remarks')
        FROM OPENJSON(@Items);
        
        -- Create initial approval records (Level 1 and Level 2)
        INSERT INTO RequisitionApproval (RequisitionId, Level, ApproverRole, Status)
        VALUES 
            (@RequisitionId, 1, 'Department Head', 1), -- Pending
            (@RequisitionId, 2, 'Procurement Officer', 1); -- Pending
        
        COMMIT TRANSACTION;
        
        SELECT @RequisitionId as RequisitionId, 'Requisition created successfully' as Message;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- 4. Submit Requisition (Change status from Draft to Submitted)
-- =============================================
CREATE PROCEDURE sp_SubmitRequisition
    @RequisitionId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Requisition WHERE RequisitionId = @RequisitionId)
        BEGIN
            RAISERROR('Requisition not found', 16, 1);
            RETURN;
        END
        
        IF EXISTS (SELECT 1 FROM Requisition WHERE RequisitionId = @RequisitionId AND Status != 1)
        BEGIN
            RAISERROR('Only draft requisitions can be submitted', 16, 1);
            RETURN;
        END
        
        UPDATE Requisition 
        SET Status = 2 -- Submitted
        WHERE RequisitionId = @RequisitionId;
        
        SELECT 'Requisition submitted successfully' as Message;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- =============================================
-- 5. Approve/Reject Requisition Stored Procedure
-- =============================================
CREATE PROCEDURE sp_ApproveRequisition
    @RequisitionId INT,
    @Level INT, -- 1 = Department Head, 2 = Procurement Officer
    @ApproverName NVARCHAR(255),
    @Status INT, -- ApprovalStatus: 1=Pending, 2=Approved, 3=Rejected
    @Remarks NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validate requisition exists
        IF NOT EXISTS (SELECT 1 FROM Requisition WHERE RequisitionId = @RequisitionId)
        BEGIN
            RAISERROR('Requisition not found', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Check if requisition is in submitted or partially approved status
        IF NOT EXISTS (SELECT 1 FROM Requisition WHERE RequisitionId = @RequisitionId AND Status IN (2, 3))
        BEGIN
            RAISERROR('Requisition must be in Submitted or Partially Approved status', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Update approval record
        UPDATE RequisitionApproval 
        SET ApproverName = @ApproverName,
            Status = @Status,
            ApprovalDate = GETDATE(),
            Remarks = @Remarks
        WHERE RequisitionId = @RequisitionId 
        AND Level = @Level;
        
        -- If rejected, update requisition status to rejected
        IF @Status = 3 -- Rejected
        BEGIN
            UPDATE Requisition 
            SET Status = 5 -- Rejected
            WHERE RequisitionId = @RequisitionId;
            
            SELECT 'Requisition rejected' as Message;
        END
        ELSE IF @Status = 2 -- Approved
        BEGIN
            DECLARE @PendingApprovals INT;
            
            -- Check if there are any pending approvals
            SELECT @PendingApprovals = COUNT(*)
            FROM RequisitionApproval 
            WHERE RequisitionId = @RequisitionId 
            AND Status = 1; -- Pending
            
            IF @PendingApprovals = 0
            BEGIN
                -- All approvals completed, mark as fully approved
                UPDATE Requisition 
                SET Status = 4 -- Approved
                WHERE RequisitionId = @RequisitionId;
                
                SELECT 'Requisition fully approved' as Message;
            END
            ELSE
            BEGIN
                -- Still has pending approvals, mark as partially approved
                UPDATE Requisition 
                SET Status = 3 -- PartiallyApproved
                WHERE RequisitionId = @RequisitionId;
                
                SELECT 'Requisition partially approved' as Message;
            END
        END
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- 6. Get Requisition Details for Approval
-- =============================================
CREATE PROCEDURE sp_GetRequisitionForApproval
    @RequisitionId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get requisition header
    SELECT 
        r.RequisitionId,
        r.RequisitionNumber,
        r.RequestedBy,
        r.Department,
        r.RequestDate,
        r.Type,
        r.Purpose,
        r.Status,
        r.Remarks
    FROM Requisition r
    WHERE r.RequisitionId = @RequisitionId;
    
    -- Get requisition items with product details
    SELECT 
        ri.RequisitionItemId,
        ri.ProductId,
        p.Name as ProductName,
        p.Unit,
        p.UnitPrice,
        ri.Quantity,
        ri.Purpose,
        ri.Remarks
    FROM RequisitionItem ri
    INNER JOIN Product p ON ri.ProductId = p.ProductId
    WHERE ri.RequisitionId = @RequisitionId;
    
    -- Get approval status
    SELECT 
        ra.Level,
        ra.ApproverName,
        ra.ApproverRole,
        ra.Status,
        ra.ApprovalDate,
        ra.Remarks
    FROM RequisitionApproval ra
    WHERE ra.RequisitionId = @RequisitionId
    ORDER BY ra.Level;
END
GO

-- =============================================
-- 7. Get Pending Approvals for User
-- =============================================
CREATE PROCEDURE sp_GetPendingApprovals
    @ApproverRole NVARCHAR(100) = NULL,
    @Level INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.RequisitionId,
        r.RequisitionNumber,
        r.RequestedBy,
        r.Department,
        r.RequestDate,
        r.Type,
        r.Purpose,
        ra.Level,
        ra.ApproverRole
    FROM Requisition r
    INNER JOIN RequisitionApproval ra ON r.RequisitionId = ra.RequisitionId
    WHERE ra.Status = 1 -- Pending
    AND r.Status IN (2, 3) -- Submitted or PartiallyApproved
    AND (@ApproverRole IS NULL OR ra.ApproverRole = @ApproverRole)
    AND (@Level IS NULL OR ra.Level = @Level)
    ORDER BY r.RequestDate;
END
GO

-- =============================================
-- Usage Examples:
-- =============================================

/*
-- Example 1: Insert a new product
DECLARE @ProductId INT;
EXEC sp_InsertProduct 
    @Name = 'Office Chair',
    @Description = 'Ergonomic office chair with lumbar support',
    @Unit = 'pcs',
    @UnitPrice = 150.00,
    @ProductId = @ProductId OUTPUT;

-- Example 2: Update a product
EXEC sp_UpdateProduct 
    @ProductId = 1,
    @Name = 'Executive Office Chair',
    @Description = 'Premium ergonomic office chair',
    @Unit = 'pcs',
    @UnitPrice = 200.00,
    @IsActive = 1;

-- Example 3: Insert requisition with items (JSON format)
DECLARE @RequisitionId INT;
DECLARE @ItemsJson NVARCHAR(MAX) = '[
    {"ProductId": 1, "Quantity": 5, "Purpose": "New office setup", "Remarks": "Black color preferred"},
    {"ProductId": 2, "Quantity": 10, "Purpose": "Stationery supplies", "Remarks": "Blue ink"}
]';

EXEC sp_InsertRequisition
    @RequisitionNumber = 'REQ-2024-001',
    @RequestedBy = 'John Doe',
    @Department = 'IT Department',
    @Purpose = 'Office supplies for new employees',
    @Type = 1, -- Regular
    @Remarks = 'Urgent requirement',
    @Items = @ItemsJson,
    @RequisitionId = @RequisitionId OUTPUT;

-- Example 4: Submit requisition
EXEC sp_SubmitRequisition @RequisitionId = 1;

-- Example 5: Approve requisition at level 1
EXEC sp_ApproveRequisition
    @RequisitionId = 1,
    @Level = 1,
    @ApproverName = 'Jane Smith',
    @Status = 2, -- Approved
    @Remarks = 'Approved by Department Head';

-- Example 6: Get requisition for approval
EXEC sp_GetRequisitionForApproval @RequisitionId = 1;

-- Example 7: Get pending approvals for Department Head
EXEC sp_GetPendingApprovals @ApproverRole = 'Department Head', @Level = 1;
*/