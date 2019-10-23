using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using WebApiSampleApp.Repository;
using WebApiSampleApp.ViewModel;
namespace WebApiSampleApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public async Task<ActionResult> GetContactlist(string filter)
            {
                List<ContactModel> ContactInfo = new List<ContactModel>();
            try
            {
                ServiceRepository serviceObj = new ServiceRepository();
                string apistring = "api/Values";

                if (!string.IsNullOrEmpty(filter))
                {
                    apistring = "api/Values/" + filter;
                }
                HttpResponseMessage response = serviceObj.GetResponse(apistring);
                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    ContactInfo = response.Content.ReadAsAsync<List<ContactModel>>().Result;

                    return View(ContactInfo);
                }

                else
                {
                    return View(ContactInfo);
                }

            }
            catch(Exception ex)
            {
                return View(ContactInfo);
            }
        }

        public ActionResult GetStates(int countryID)
        {
            using (MYCONTACTBOOKEntities dc = new MYCONTACTBOOKEntities())
            {
                //We will off Lazy Loading
                var State = (from a in dc.States
                             where a.CountryID.Equals(countryID)
                             orderby a.StateName
                             select a).ToList();

                var result = (from s in State
                              select new
                              {
                                  StateID = s.StateID,
                                  StateName = s.StateName
                              }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);


            }
        }



        public async Task<ActionResult> View(int id)
        {
            

             List < ContactModel > Contactmodel = new List<ContactModel>();
             Contact c = new Contact();
            try
            {

                ServiceRepository serviceObj = new ServiceRepository();

                string apistring = "api/Values?contactID=" + id;
                HttpResponseMessage response = serviceObj.GetResponse(apistring);
                if (response.IsSuccessStatusCode)
                {

                    response.EnsureSuccessStatusCode();
                    Contactmodel = response.Content.ReadAsAsync<List<ContactModel>>().Result;


                    foreach (var con in Contactmodel)
                    {
                        c.ContactPersonFname = con.FirstName;
                        c.ContactPersonLname = con.LastName;
                        c.ContactNo1 = con.ContactNo1;
                        c.ContactNo2 = con.ContactNo2;
                        c.EmailID = con.EmailID;
                        c.CountryName = con.Country;
                        c.StateName = con.State;
                        c.Address = con.Address;
                        c.ImagePath = con.ImagePath;
                    }

                    //returning the contacts list to view  
                    return View(c);
                }

                else
                {
                    ModelState.AddModelError(string.Empty, "Server Error. Please contact administrator.");


                    return View(c);
                }
            }
            catch(Exception ex)
            {
                return View(c);
            }
            
        }

        [System.Web.Mvc.HttpGet]
            public ActionResult Add()
        {
            //fetch country data 
            List<Country> AllCountry = new List<Country>();
            List<State> states = new List<State>();
          
            using (MYCONTACTBOOKEntities dc = new MYCONTACTBOOKEntities())
            {
                AllCountry = dc.Countries.OrderBy(a => a.CountryName).ToList();
                //Not need to fetch state as we dont know which country will user select here
            }

            ViewBag.Country = new SelectList(AllCountry, "CountryID", "CountryName");
            ViewBag.State = new SelectList(states, "StateID", "StateName");

            return View();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Contact c, HttpPostedFileBase file)
        {
            try
            {
                #region // Fetch Country & State
                List<Country> allCountry = new List<Country>();
                List<State> states = new List<State>();
                using (MYCONTACTBOOKEntities dc = new MYCONTACTBOOKEntities())
                {
                    allCountry = dc.Countries.OrderBy(a => a.CountryName).ToList();
                    if (c.CountryID > 0)
                    {
                        states = dc.States.Where(a => a.CountryID.Equals(c.CountryID)).OrderBy(a => a.StateName).ToList();
                    }
                }
                ViewBag.Country = new SelectList(allCountry, "CountryID", "CountryName", c.CountryID);
                ViewBag.State = new SelectList(states, "StateID", "StateName", c.StateID);
                #endregion
                #region// Validate file if selected
                if (file != null)
                {
                    if (file.ContentLength > (512 * 1000)) // 512 KB
                    {
                        ModelState.AddModelError("FileErrorMessage", "File size must be within 512 KB");
                    }
                    string[] allowedType = new string[] { "image/png", "image/gif", "image/jpeg", "image/jpg" };
                    bool isFileTypeValid = false;
                    foreach (var i in allowedType)
                    {
                        if (file.ContentType == i.ToString())
                        {
                            isFileTypeValid = true;
                            break;
                        }
                    }
                    if (!isFileTypeValid)
                    {
                        ModelState.AddModelError("FileErrorMessage", "Only .png, .gif and .jpg file type allowed");
                    }
                }
                #endregion
                #region// Validate Model & Save to Database
                if (ModelState.IsValid)
                {
                    //Save here
                    if (file != null)
                    {
                        string savePath = Server.MapPath("~/Image");
                        string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        file.SaveAs(Path.Combine(savePath, fileName));
                        c.ImagePath = fileName;
                    }


                    
                    ServiceRepository serviceObj = new ServiceRepository();
                    HttpResponseMessage response = serviceObj.PostResponse("api/Values/", c);
                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();
                        return RedirectToAction("GetContactlist");
                    }

                    return View(c);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Please capture all the required fileds.");
                    return View(c);
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.ToString());
                return View(c);
            }
            #endregion
        }





        //Delete 
        public ActionResult Delete(int id)
        {
            ////Fetch Contact

            List<ContactModel> Contactmodel = new List<ContactModel>();
            Contact c = new Contact();
            try
            {
                ServiceRepository serviceObj = new ServiceRepository();

                string apistring = "api/Values?contactID=" + id;
                HttpResponseMessage response = serviceObj.GetResponse(apistring);
                if (response.IsSuccessStatusCode)
                {

                    response.EnsureSuccessStatusCode();
                    Contactmodel = response.Content.ReadAsAsync<List<ContactModel>>().Result;


                    foreach (var con in Contactmodel)
                    {
                        c.ContactPersonFname = con.FirstName;
                        c.ContactPersonLname = con.LastName;
                        c.ContactNo1 = con.ContactNo1;
                        c.ContactNo2 = con.ContactNo2;
                        c.EmailID = con.EmailID;
                        c.CountryName = con.Country;
                        c.StateName = con.State;
                        c.Address = con.Address;
                        c.ImagePath = con.ImagePath;
                    }

                    //returning the Contact list to view  
                    return View(c);
                }

                else
                {
                    ModelState.AddModelError(string.Empty, "Server Error. Please contact administrator.");


                    return View(c);
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.ToString());

                return View(c);
            }

        }

        //Delete POST
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.ActionName("Delete")] // Here Action Name                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        is required as we can not make same signature for Get & Post Method
        public ActionResult DeleteConfirm(Contact c,int id)
        {
            try
            {
                ServiceRepository serviceObj = new ServiceRepository();
                HttpResponseMessage response = serviceObj.DeleteResponse("api/Values/Delete?id=" + id.ToString());

                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    return RedirectToAction("GetContactlist");
                }

                else
                {
                    return HttpNotFound("Contact Not Found!");
                }
            }
            catch(Exception ex)
            {
                return HttpNotFound("Contact Not Found!");
            }

        }

    }
}
