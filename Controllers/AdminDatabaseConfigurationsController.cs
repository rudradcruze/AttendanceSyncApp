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
    /// Manages database configurations for administrators,
    /// including CRUD operations and secure credential handling.
    /// </summary>
    [AdminAuthorize]
    public class AdminDatabaseConfigurationsController : BaseController
    {
        /// <summary>
        /// Database configuration service for managing database settings.
        /// </summary>
        private readonly IAdminDatabaseConfigService _service;

        /// <summary>
        /// Initializes controller with default services.
        /// </summary>
        public AdminDatabaseConfigurationsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _service = new AdminDatabaseConfigService(unitOfWork);
        }

        // GET: AdminDatabaseConfigurations/Index
        public ActionResult Index()
        {
            // Return the database configurations management view
            return View("~/Views/Admin/DatabaseConfigurations.cshtml");
        }

        // GET: AdminDatabaseConfigurations/GetAll
        [HttpGet]
        public JsonResult GetAll(int page = 1, int pageSize = 20)
        {
            // Retrieve paginated list of database configurations
            var result = _service.GetAllConfigsPaged(page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return configuration data with pagination info
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminDatabaseConfigurations/Get
        [HttpGet]
        public JsonResult Get(int id)
        {
            // Retrieve specific database configuration details by ID
            var result = _service.GetConfigById(id);

            // If configuration not found or error occurs, return failure
            if (!result.Success)
            {
                return Json(ApiResponse<DatabaseConfigDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return configuration details
            return Json(ApiResponse<DatabaseConfigDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminDatabaseConfigurations/GetPassword
        [HttpGet]
        public JsonResult GetPassword(int id)
        {
            // Retrieve decrypted database password for a configuration
            var result = _service.GetDatabasePassword(id);

            // If retrieval fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<string>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return decrypted password
            return Json(ApiResponse<string>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminDatabaseConfigurations/Create
        [HttpPost]
        public JsonResult Create(DatabaseConfigCreateDto dto)
        {
            // Attempt to create a new database configuration
            var result = _service.CreateConfig(dto);

            // If creation fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminDatabaseConfigurations/Update
        [HttpPost]
        public JsonResult Update(DatabaseConfigUpdateDto dto)
        {
            // Attempt to update existing database configuration
            var result = _service.UpdateConfig(dto);

            // If update fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminDatabaseConfigurations/Delete
        [HttpPost]
        public JsonResult Delete(int id)
        {
            // Attempt to delete the specified database configuration
            var result = _service.DeleteConfig(id);

            // If deletion fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // GET: AdminDatabaseConfigurations/GetCompanies
        [HttpGet]
        public JsonResult GetCompanies()
        {
            // Retrieve available companies for database configuration assignment
            var result = _service.GetAvailableCompanies();

            // If retrieval fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return list of available companies
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }
    }
}