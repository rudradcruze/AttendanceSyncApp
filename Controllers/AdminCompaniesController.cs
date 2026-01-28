using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Handles company management operations for administrators,
    /// including CRUD operations and status management.
    /// </summary>
    [AdminAuthorize]
    public class AdminCompaniesController : BaseController
    {
        /// Company management service for business logic.
        private readonly ICompanyManagementService _companyService;

        /// Initializes controller with default services.
        public AdminCompaniesController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _companyService = new CompanyManagementService(unitOfWork);
        }

        // GET: AdminCompanies/Index
        public ActionResult Index()
        {
            // Return the company management view
            return View("~/Views/Admin/Companies.cshtml");
        }

        // GET: AdminCompanies/GetCompanies
        [HttpGet]
        public JsonResult GetCompanies(int page = 1, int pageSize = 20)
        {
            // Retrieve paginated list of companies
            var result = _companyService.GetCompaniesPaged(page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return company data with pagination info
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminCompanies/GetCompany
        [HttpGet]
        public JsonResult GetCompany(int id)
        {
            // Retrieve specific company details by ID
            var result = _companyService.GetCompanyById(id);

            // If company not found or error occurs, return failure
            if (!result.Success)
            {
                return Json(ApiResponse<CompanyManagementDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return company details
            return Json(ApiResponse<CompanyManagementDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminCompanies/CreateCompany
        [HttpPost]
        public JsonResult CreateCompany(CompanyCreateDto dto)
        {
            // Attempt to create a new company
            var result = _companyService.CreateCompany(dto);

            // If creation fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanies/UpdateCompany
        [HttpPost]
        public JsonResult UpdateCompany(CompanyUpdateDto dto)
        {
            // Attempt to update existing company information
            var result = _companyService.UpdateCompany(dto);

            // If update fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanies/DeleteCompany
        [HttpPost]
        public JsonResult DeleteCompany(int id)
        {
            // Attempt to delete the specified company
            var result = _companyService.DeleteCompany(id);

            // If deletion fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanies/ToggleCompanyStatus
        [HttpPost]
        public JsonResult ToggleCompanyStatus(int id)
        {
            // Toggle company active/inactive status
            var result = _companyService.ToggleCompanyStatus(id);

            // If toggle fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }
    }
}
