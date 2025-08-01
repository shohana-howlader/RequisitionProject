USE [Requisition]
GO
/****** Object:  StoredProcedure [dbo].[sp_SubmitRequisition]    Script Date: 7/29/2025 6:18:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored procedure to submit requisition
-- Stored procedure to submit requisition
ALTER   PROCEDURE [dbo].[sp_SubmitRequisition]
    @RequisitionId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE Requisition 
        SET Status = 2 -- Submitted
        WHERE RequisitionId = @RequisitionId AND Status = 1; -- Only if currently Draft
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Requisition not found or already submitted', 16, 1);
            RETURN;
        END
        
        SELECT 'Requisition submitted successfully' as Message;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
