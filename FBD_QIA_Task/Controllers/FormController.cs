using Microsoft.AspNetCore.Mvc;
using static FBD_QIA_Task.Models.FormViewModels;
using System.Data;
using Microsoft.Data.SqlClient;

namespace FBD_QIA_Task.Controllers
{
    public class FormController : Controller
    {
        private readonly string _conn;
        public FormController(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        // GET: /Form/Create
        public IActionResult Create()
        {
            
            var vm = new FormViewModel();
            vm.Fields.Add(new FormFieldViewModel()); 
            return View(vm);
        }


        // POST: /Form/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the errors before saving!";
                return View(model);
            }

            int newFormId = 0;

            using (var conn = new SqlConnection(_conn))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert Form Title
                        using (var cmd = new SqlCommand("InsertForm", conn, tx))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Title", model.Title);

                            var pFormId = new SqlParameter("@FormId", SqlDbType.Int)
                            { Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(pFormId);

                            cmd.ExecuteNonQuery();
                            newFormId = Convert.ToInt32(pFormId.Value);
                        }

                        // Insert Fields
                        foreach (var fld in model.Fields)
                        {
                            using (var cmd = new SqlCommand("InsertFormField", conn, tx))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@FormId", newFormId);
                                cmd.Parameters.AddWithValue("@Label", fld.Label);
                                cmd.Parameters.AddWithValue("@IsRequired", fld.IsRequired);
                                cmd.Parameters.AddWithValue("@SelectedOption",
                                    string.IsNullOrEmpty(fld.SelectedOption) ? (object)DBNull.Value : fld.SelectedOption);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        TempData["Error"] = "Save failed: " + ex.Message;
                        return View(model);
                    }
                }
            }

            TempData["Success"] = "Form saved successfully!";
            return RedirectToAction("Index");
        }


        // GET: /Form/Index
        public IActionResult Index()
        {
            var list = new List<FormViewModel>();
            using (var conn = new SqlConnection(_conn))
            {
                conn.Open();
                using (var cmd = new SqlCommand("GetAllForms", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new FormViewModel
                            {
                                FormId = Convert.ToInt32(rdr["FormId"]),
                                Title = rdr["Title"].ToString()
                            });
                        }
                    }
                }
            }
            return View(list);
        }

        // GET: /Form/Preview/{id}
        public IActionResult Preview(int id)
        {
            var vm = new FormViewModel();
            vm.Fields = new List<FormFieldViewModel>();

            using (var conn = new SqlConnection(_conn))
            {
                conn.Open();
                using (var cmd = new SqlCommand("GetFormDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FormId", id);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            // set Title once (from first record)
                            if (vm.FormId == 0)
                            {
                                vm.FormId = Convert.ToInt32(rdr["FormId"]);
                                vm.Title = rdr["Title"].ToString();
                            }

                            vm.Fields.Add(new FormFieldViewModel
                            {
                                FieldId = Convert.ToInt32(rdr["FieldId"]),
                                Label = rdr["Label"].ToString(),
                                IsRequired = Convert.ToBoolean(rdr["IsRequired"]),
                                SelectedOption = rdr["SelectedOption"] == DBNull.Value ? null : rdr["SelectedOption"].ToString()
                            });
                        }
                    }
                }
            }

            return View(vm);
        }
    }
}
