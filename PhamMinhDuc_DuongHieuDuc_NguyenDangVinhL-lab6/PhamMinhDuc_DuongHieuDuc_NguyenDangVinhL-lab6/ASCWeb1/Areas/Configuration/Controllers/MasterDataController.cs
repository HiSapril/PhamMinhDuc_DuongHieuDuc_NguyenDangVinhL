using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Utilities;
using ASCWeb1.Areas.Configuration.Models;
using ASCWeb1.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var masterKeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterKeys);
            // Hold all Master Keys in session
            HttpContext.Session.SetSession("MasterKeys", masterKeysViewModel);
            return View(new MasterKeysViewModel
            {
                MasterKeys = masterKeysViewModel ?? new List<MasterDataKeyViewModel>(),
                IsEdit = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel model)
        {
            model.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys") ?? new List<MasterDataKeyViewModel>();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.MasterKeyInContext == null)
            {
                return View(model);
            }

            var masterKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(model.MasterKeyInContext);
            if (model.IsEdit)
            {
                // Update Master Key
                var userDetails = HttpContext.User.GetCurrentUserDetails();
                masterKey.UpdatedBy = userDetails?.Name ?? "System";
                var originalPartitionKey = model.MasterKeyInContext.PartitionKey ?? masterKey.PartitionKey;
                await _masterData.UpdateMasterKeyAsync(originalPartitionKey, masterKey);
            }
            else
            {
                // Insert Master Key
                // Check if PartitionKey already exists
                var existingKeys = await _masterData.GetMasterKeyByNameAsync(model.MasterKeyInContext.Name);
                if (existingKeys.Any())
                {
                    ModelState.AddModelError("", $"Master Key '{model.MasterKeyInContext.Name}' already exists!");
                    model.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys") ?? new List<MasterDataKeyViewModel>();
                    return View(model);
                }
                
                masterKey.RowKey = Guid.NewGuid().ToString();
                masterKey.PartitionKey = masterKey.Name;
                masterKey.IsDeleted = false;
                var userDetails = HttpContext.User.GetCurrentUserDetails();
                masterKey.CreatedBy = userDetails?.Name ?? "System";
                masterKey.UpdatedBy = userDetails?.Name ?? "System";
                masterKey.CreatedDate = DateTime.UtcNow;
                masterKey.UpdatedDate = DateTime.UtcNow;
                await _masterData.InsertMasterKeyAsync(masterKey);
            }

            return RedirectToAction("MasterKeys");
        }

        // Master Value
        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            // Get All Master Keys and hold them in ViewBag for Select tag
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            
            // Filter out deleted keys
            var activeMasterKeys = masterKeys.Where(k => !k.IsDeleted).ToList();
            
            Console.WriteLine($"[DEBUG] Total Master Keys count: {masterKeys.Count}");
            Console.WriteLine($"[DEBUG] Active Master Keys count: {activeMasterKeys.Count}");
            
            foreach (var key in activeMasterKeys)
            {
                Console.WriteLine($"[DEBUG] Key: {key.PartitionKey}, Name: {key.Name}, IsActive: {key.IsActive}, IsDeleted: {key.IsDeleted}");
            }
            
            ViewBag.MasterKeys = activeMasterKeys;
            
            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                IsEdit = false
            });
        }

        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            Console.WriteLine($"[DEBUG] MasterValuesByKey called with key: '{key}'");
            
            // Get Master values based on master key.
            var masterValues = await _masterData.GetAllMasterValuesByKeyAsync(key);
            
            Console.WriteLine($"[DEBUG] Found {masterValues.Count} Master Values for key '{key}'");
            foreach (var value in masterValues)
            {
                Console.WriteLine($"[DEBUG]   - RowKey: {value.RowKey}, Name: {value.Name}, PartitionKey: {value.PartitionKey}, IsActive: {value.IsActive}, IsDeleted: {value.IsDeleted}");
            }
            
            // Map to ViewModel to ensure consistent property names
            var masterValuesViewModel = _mapper.Map<List<MasterDataValue>, List<MasterDataValueViewModel>>(masterValues);
            
            return Json(new { data = masterValuesViewModel });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMasterKeys()
        {
            // Get all master keys for dropdown
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            return Json(new { data = masterKeys });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel? masterValue)
        {
            Console.WriteLine($"[DEBUG] POST MasterValues - isEdit: {isEdit}");
            Console.WriteLine($"[DEBUG] MasterValue - PartitionKey: {masterValue?.PartitionKey}, Name: {masterValue?.Name}, IsActive: {masterValue?.IsActive}");
            
            if (masterValue == null)
            {
                Console.WriteLine("[DEBUG] MasterValue is NULL!");
                return Json("Error");
            }
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("[DEBUG] ModelState is INVALID!");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"[DEBUG] Error: {error.ErrorMessage}");
                }
                return Json("Error");
            }

            var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);
            Console.WriteLine($"[DEBUG] Mapped MasterDataValue - PartitionKey: {masterDataValue.PartitionKey}, RowKey: {masterDataValue.RowKey}, Name: {masterDataValue.Name}");
            
            if (isEdit)
            {
                // Update Master Value
                Console.WriteLine("[DEBUG] Updating Master Value...");
                await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey, masterDataValue.RowKey, masterDataValue);
            }
            else
            {
                // Insert Master Value
                Console.WriteLine("[DEBUG] Inserting Master Value...");
                masterDataValue.RowKey = Guid.NewGuid().ToString();
                masterDataValue.IsDeleted = false;
                var userDetails = HttpContext.User.GetCurrentUserDetails();
                masterDataValue.CreatedBy = userDetails?.Name ?? "System";
                masterDataValue.UpdatedBy = userDetails?.Name ?? "System";
                masterDataValue.CreatedDate = DateTime.UtcNow;
                masterDataValue.UpdatedDate = DateTime.UtcNow;
                Console.WriteLine($"[DEBUG] Before Insert - PartitionKey: {masterDataValue.PartitionKey}, RowKey: {masterDataValue.RowKey}, Name: {masterDataValue.Name}, CreatedBy: {masterDataValue.CreatedBy}");
                await _masterData.InsertMasterValueAsync(masterDataValue);
                Console.WriteLine("[DEBUG] Insert completed!");
            }

            return Json(true);
        }

        // Upload Excel
        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded" });
            }

            try
            {
                var masterValues = new List<MasterDataValue>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        var userDetails = HttpContext.User.GetCurrentUserDetails();
                        var userName = userDetails?.Name ?? "System";
                        
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var masterValue = new MasterDataValue
                            {
                                PartitionKey = worksheet.Cells[row, 1].Value?.ToString() ?? "",
                                RowKey = Guid.NewGuid().ToString(),
                                Name = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                                IsActive = bool.Parse(worksheet.Cells[row, 3].Value?.ToString() ?? "true"),
                                IsDeleted = bool.Parse(worksheet.Cells[row, 4].Value?.ToString() ?? "false"),
                                CreatedBy = userName,
                                UpdatedBy = userName
                            };
                            masterValues.Add(masterValue);
                        }
                    }
                }

                await _masterData.UploadBulkMasterData(masterValues);
                return Json(new { success = true, message = "Data uploaded successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
