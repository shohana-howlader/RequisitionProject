using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RequisitionProject.Models;
using RequisitionProject.Models.ViewModel;
using System.Data;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace RequisitionProject.Controllers
{
    public class RequisitionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequisitionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Requisition/Create
        public async Task<IActionResult> Create()
        {
            var model = new CreateRequisitionViewModel();
            // Initialize with at least one empty item for proper binding
            model.Items.Add(new RequisitionItemViewModel());
            await LoadDropdownData(model);
            return View(model);
        }

        // POST: Requisition/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequisitionViewModel model)
        {
            // Remove empty items before validation
            model.Items = model.Items?.Where(i => i.ProductId > 0 && i.Quantity > 0).ToList() ?? new List<RequisitionItemViewModel>();

            if (!ModelState.IsValid || !model.Items.Any())
            {
                if (!model.Items.Any())
                {
                    ModelState.AddModelError("Items", "At least one item is required.");
                    model.Items.Add(new RequisitionItemViewModel()); // Add empty item for form
                }
                await LoadDropdownData(model);
                return View(model);
            }

            try
            {
                var requisitionNumber = await GenerateRequisitionNumber();

                var itemsJson = JsonConvert.SerializeObject(model.Items.Select(i => new
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Purpose = i.Purpose ?? string.Empty,
                    Remarks = i.Remarks ?? string.Empty
                }));

                // Use parameters properly
                var parameters = new[]
                {
                    new SqlParameter("@RequisitionNumber", SqlDbType.NVarChar, 50) { Value = requisitionNumber },
                    new SqlParameter("@RequestedBy", SqlDbType.NVarChar, 255) { Value = User.Identity?.Name ?? "Anonymous" },
                    new SqlParameter("@Department", SqlDbType.NVarChar, 255) { Value = model.Department },
                    new SqlParameter("@Purpose", SqlDbType.NVarChar, 500) { Value = model.Purpose },
                    new SqlParameter("@Type", SqlDbType.Int) { Value = (int)model.Type },
                    new SqlParameter("@Remarks", SqlDbType.NVarChar, 1000) { Value = model.Remarks ?? (object)DBNull.Value },
                    new SqlParameter("@Items", SqlDbType.NVarChar, -1) { Value = itemsJson },
                    new SqlParameter("@RequisitionId", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_InsertRequisition @RequisitionNumber, @RequestedBy, @Department, @Purpose, @Type, @Remarks, @Items, @RequisitionId OUTPUT",
                    parameters);

                var requisitionId = (int)parameters[7].Value;
                TempData["Success"] = "Requisition created successfully!";
                return RedirectToAction("Details", new { id = requisitionId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating requisition: " + ex.Message);
                await LoadDropdownData(model);
                return View(model);
            }
        }

        // GET: Requisition/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var results = await _context.RequisitionDetailsViewModels
                    .FromSqlRaw("EXEC sp_GetRequisitionForApproval @RequisitionId = {0}", id)
                    .ToListAsync();

                if (results == null || results.Count == 0)
                {
                    TempData["Error"] = "Requisition not found.";
                    return RedirectToAction("Index");
                }

                var requisition = results.First();
                var items = await _context.RequisitionItemDetailsViewModels
                    .FromSqlRaw("EXEC sp_GetRequisitionItems @RequisitionId = {0}", id)
                    .ToListAsync();

                var approvals = await _context.ApprovalDetailsViewModels
                    .FromSqlRaw("EXEC sp_GetRequisitionApprovals @RequisitionId = {0}", id)
                    .ToListAsync();

                requisition.Items = items;
                requisition.Approvals = approvals;

                return View(requisition);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading requisition: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Requisition/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var requisitions = await _context.RequisitionListViewModels
                    .FromSqlRaw("SELECT RequisitionId, RequisitionNumber, RequestDate, RequestedBy, Department, Purpose, Type, Status FROM Requisition ORDER BY RequestDate DESC")
                    .ToListAsync();

                return View(requisitions);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading requisitions: " + ex.Message;
                return View(new List<RequisitionListViewModel>());
            }
        }

        // POST: Requisition/Submit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_SubmitRequisition @RequisitionId = {0}",
                    id);

                TempData["Success"] = "Requisition submitted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error submitting requisition: " + ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }

        // GET: Requisition/Approve/5
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var requisition = await _context.RequisitionDetailsViewModels
                    .FromSqlRaw("EXEC sp_GetRequisitionForApproval @RequisitionId = {0}", id)
                    .FirstOrDefaultAsync();

                if (requisition == null)
                {
                    TempData["Error"] = "Requisition not found.";
                    return RedirectToAction("PendingApprovals");
                }

                var items = await _context.RequisitionItemDetailsViewModels
                    .FromSqlRaw("EXEC sp_GetRequisitionItems @RequisitionId = {0}", id)
                    .ToListAsync();

                var model = new ApprovalViewModel
                {
                    RequisitionId = requisition.RequisitionId,
                    RequisitionNumber = requisition.RequisitionNumber,
                    RequestedBy = requisition.RequestedBy,
                    Department = requisition.Department,
                    RequestDate = requisition.RequestDate,
                    Type = requisition.Type,
                    Purpose = requisition.Purpose,
                    Items = items.Select(i => new RequisitionItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        Purpose = i.Purpose,
                        Remarks = i.Remarks
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading requisition for approval: " + ex.Message;
                return RedirectToAction("PendingApprovals");
            }
        }

        // POST: Requisition/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApprovalViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var level = DetermineApprovalLevel(User);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_ApproveRequisition @RequisitionId = {0}, @Level = {1}, @ApproverName = {2}, @Status = {3}, @Remarks = {4}",
                    model.RequisitionId,
                    level,
                    User.Identity.Name ?? "Approver",
                    (int)model.Decision,
                    model.ApprovalRemarks
                );

                TempData["Success"] = model.Decision == ApprovalStatus.Approved
                    ? "Requisition approved successfully!"
                    : "Requisition rejected.";

                return RedirectToAction("PendingApprovals");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error processing approval: " + ex.Message);
                return View(model);
            }
        }

        // GET: Requisition/PendingApprovals
        public async Task<IActionResult> PendingApprovals()
        {
            try
            {
                var userRole = GetUserRole(User);
                var level = userRole == "Department Head" ? 1 : 2;

                var pendingApprovals = await _context.PendingApprovalViewModels
                    .FromSqlRaw("EXEC sp_GetPendingApprovals @ApproverRole = {0}, @Level = {1}", userRole, level)
                    .ToListAsync();

                return View(pendingApprovals);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading pending approvals: " + ex.Message;
                return View(new List<PendingApprovalViewModel>());
            }
        }

        // AJAX: Get Product Details
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            try
            {
                var product = await _context.Products
                    .Where(p => p.ProductId == productId && p.IsActive)
                    .Select(p => new { p.ProductId, p.Name, p.Unit, p.UnitPrice })
                    .FirstOrDefaultAsync();

                if (product == null)
                    return Json(new { success = false, message = "Product not found" });

                return Json(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region Private Methods

        private async Task LoadDropdownData(CreateRequisitionViewModel model)
        {
            model.Products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.Name
                }).ToListAsync();

            model.Departments = new List<SelectListItem>
            {
                new SelectListItem { Value = "IT Department", Text = "IT Department" },
                new SelectListItem { Value = "HR Department", Text = "HR Department" },
                new SelectListItem { Value = "Finance Department", Text = "Finance Department" },
                new SelectListItem { Value = "Operations", Text = "Operations" },
                new SelectListItem { Value = "Marketing", Text = "Marketing" }
            };
        }

        private async Task<string> GenerateRequisitionNumber()
        {
            var currentYear = DateTime.Now.Year;
            var count = await _context.Requisitions.CountAsync(r => r.RequestDate.Year == currentYear);
            return $"REQ-{currentYear}-{(count + 1):D4}";
        }

        private int DetermineApprovalLevel(ClaimsPrincipal user)
        {
            var role = GetUserRole(user);
            return role == "Department Head" ? 1 : 2;
        }

        private string GetUserRole(ClaimsPrincipal user)
        {
            return user.IsInRole("DepartmentHead") ? "Department Head" : "Procurement Officer";
        }

        #endregion
    }
}