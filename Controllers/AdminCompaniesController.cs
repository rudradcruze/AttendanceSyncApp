using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminCompaniesController : BaseController
    {
        private readonly ICompanyManagementService _companyService;

        public AdminCompaniesController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _companyService = new CompanyManagementService(unitOfWork);
        }

        // GET: AdminCompanies/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/Companies.cshtml");
        }

        // GET: AdminCompanies/GetCompanies
        [HttpGet]
        public JsonResult GetCompanies(int page = 1, int pageSize = 20)
        {
            var result = _companyService.GetCompaniesPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminCompanies/GetCompany
        [HttpGet]
        public JsonResult GetCompany(int id)
        {
            var result = _companyService.GetCompanyById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<CompanyManagementDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<CompanyManagementDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminCompanies/CreateCompany
        [HttpPost]
        public JsonResult CreateCompany(CompanyCreateDto dto)
        {
            var result = _companyService.CreateCompany(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanies/UpdateCompany
        [HttpPost]
        public JsonResult UpdateCompany(CompanyUpdateDto dto)
        {
            var result = _companyService.UpdateCompany(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanies/DeleteCompany
        [HttpPost]
        public JsonResult DeleteCompany(int id)
        {
            var result = _companyService.DeleteCompany(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanies/ToggleCompanyStatus
        [HttpPost]
        public JsonResult ToggleCompanyStatus(int id)
        {
            var result = _companyService.ToggleCompanyStatus(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }
    }
}
