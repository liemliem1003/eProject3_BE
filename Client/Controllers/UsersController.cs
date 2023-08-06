using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Client.Data;
using Client.Models;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class UsersController : Controller
    {
        private readonly ClientContext _context;
        private readonly string Uri = "http://localhost:5019/api/Users";

        public UsersController(ClientContext context)
        {
            _context = context;
        }

        private HttpClient client = new HttpClient();

        //Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Username, Password")] User employee)
        {
            var data = JsonConvert.DeserializeObject<IEnumerable<User>>(client.GetStringAsync(Uri).Result);
            var user = data.SingleOrDefault(em => em.Username == employee.Username);
            if (user != null)
            {
                HttpContext.Session.SetString("name", user.Username);
                //TempData["user"] = user.empName;
                if (HttpContext.Session.GetString("name") == null)
                {
                    return View("Login");
                }
                else
                {
                    if (user.Password.Equals(employee.Password))
                    {
                        ViewBag.name = HttpContext.Session.GetString("name");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.mess = "Invalid Password !";
                    }
                }
            }
            else
            {
                ViewBag.mess = "Invalid username !";
            }
            return View();
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("name") == null)
            {
                return View("Login");
            }
            else
            {
                var data = JsonConvert.DeserializeObject<IEnumerable<User>>(client.GetStringAsync(Uri).Result);
                return View(data);
            }
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var data = JsonConvert.DeserializeObject<User>(client.GetStringAsync(Uri + id).Result);
            return View(data);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User employee)
        {
            var data = client.PostAsJsonAsync<User>(Uri, employee).Result;
            if (data.IsSuccessStatusCode)
            {
                ViewBag.mess = "Insert new Employee successfully !";
            }
            return View();
        }

        //// GET: Employees/Edit/5
        //public async Task<IActionResult> Edit(string id)
        //{
        //    var data = JsonConvert.DeserializeObject<Employee>(client.GetStringAsync(Uri + id).Result);
        //    return View(data);
        //}

        //// POST: Employees/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, Employee employee)
        //{
        //    var data = client.PutAsJsonAsync<Employee>(Uri + id, employee).Result;
        //    if (data.IsSuccessStatusCode)
        //    {
        //        ViewBag.mess = "Edit successfully !";
        //    }
        //    //return RedirectToAction("Index");
        //    return View();
        //}

        //// GET: Employees/Delete/5
        //public async Task<IActionResult> Delete(string id)
        //{
        //    var data = client.DeleteAsync(Uri + id).Result;
        //    if (data.IsSuccessStatusCode)
        //    {
        //        ViewBag.mess = "Delete Employee successfully !";
        //    }
        //    return RedirectToAction("Index");
        //}

        private bool UserExists(int id)
        {
            return (_context.User?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}