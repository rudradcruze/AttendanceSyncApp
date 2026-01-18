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
    public class AdminDatabaseConfigurationsController : BaseController
    {
        private readonly IAdminDatabaseConfigService _service;

        public AdminDatabaseConfigurationsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _service = new AdminDatabaseConfigService(unitOfWork);
        }

        // GET: AdminDatabaseConfigurations/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/DatabaseConfigurations.cshtml");
        }

        // GET: AdminDatabaseConfigurations/GetAll
        [HttpGet]
        public JsonResult GetAll(int page = 1, int pageSize = 20)
        {
            var result = _service.GetAllConfigsPaged(page, pageSize);
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminDatabaseConfigurations/Get
        [HttpGet]
        public JsonResult Get(int id)
        {
            var result = _service.GetConfigById(id);
            if (!result.Success)
            {
                return Json(ApiResponse<DatabaseConfigDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }
            return Json(ApiResponse<DatabaseConfigDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminDatabaseConfigurations/GetPassword
        [HttpGet]
        public JsonResult GetPassword(int id)
        {
            var result = _service.GetDatabasePassword(id);
            if (!result.Success)
            {
                return Json(ApiResponse<string>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }
            return Json(ApiResponse<string>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminDatabaseConfigurations/Create
        [HttpPost]
        public JsonResult Create(DatabaseConfigCreateDto dto)
        {
            var result = _service.CreateConfig(dto);
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminDatabaseConfigurations/Update
        [HttpPost]
        public JsonResult Update(DatabaseConfigUpdateDto dto)
        {
            var result = _service.UpdateConfig(dto);
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminDatabaseConfigurations/Delete
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var result = _service.DeleteConfig(id);
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }
            return Json(ApiResponse.Success(result.Message));
        }

        // GET: AdminDatabaseConfigurations/GetCompanies
        [HttpGet]
        public JsonResult GetCompanies()
        {
            var result = _service.GetAvailableCompanies();
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }
    }
}