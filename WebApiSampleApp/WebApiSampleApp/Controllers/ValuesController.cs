using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiSampleApp.ViewModel;
using Newtonsoft.Json.Serialization;
namespace WebApiSampleApp.Controllers
{
    public class ValuesController : ApiController
    {
       

        [HttpGet]
        public IEnumerable<ContactModel> GetContactbyId(string id)
        {
            List<ContactModel> contacts = new List<ContactModel>();
            List<ContactModel> searchedContacts = new List<ContactModel>();
            try
            {
                MYCONTACTBOOKEntities dc = new MYCONTACTBOOKEntities();
                var v = (from a in dc.Contacts
                         join b in dc.Countries on a.CountryID equals b.CountryID
                         join c in dc.States on a.StateID equals c.StateID
                         select new ContactModel
                         {
                             ContactID = a.ContactID,
                             FirstName = a.ContactPersonFname,
                             LastName = a.ContactPersonLname,
                             ContactNo1 = a.ContactNo1,
                             ContactNo2 = a.ContactNo2,
                             EmailID = a.EmailID,
                             Country = b.CountryName,
                             State = c.StateName,
                             Address = a.Address,
                             ImagePath = a.ImagePath
                         }).ToList();
                contacts = v;
                searchedContacts = contacts.Where(x => x.FirstName.ToLower().Contains(id.ToLower()) || x.LastName.ToLower().Contains(id.ToLower())).ToList();

                return searchedContacts;
            }
            catch(Exception ex)
            {
                return searchedContacts;
            }

        }

        [HttpGet]
        public IEnumerable<ContactModel> GetContactlist()
        {
           
                List<ContactModel> contacts = new List<ContactModel>();
            try
            {
                MYCONTACTBOOKEntities dc = new MYCONTACTBOOKEntities();
                var v = (from a in dc.Contacts
                         join b in dc.Countries on a.CountryID equals b.CountryID
                         join c in dc.States on a.StateID equals c.StateID
                         select new ContactModel
                         {
                             ContactID = a.ContactID,
                             FirstName = a.ContactPersonFname,
                             LastName = a.ContactPersonLname,
                             ContactNo1 = a.ContactNo1,
                             ContactNo2 = a.ContactNo2,
                             EmailID = a.EmailID,
                             Country = b.CountryName,
                             State = c.StateName,
                             Address = a.Address,
                             ImagePath = a.ImagePath
                         }).ToList();
                contacts = v;

                return contacts;
            }
            catch(Exception ex)
            {
                return contacts;
            }
        }


        //This is the function for get contact by id, I am going to create this as we will use this multiple time
        [HttpGet]
        public IEnumerable<ContactModel> GetFetchContact(int contactID)
        {
            Contact contact = null;
            List<ContactModel> contacts = new List<ContactModel>();
            List<ContactModel> FetchedContact = new List<ContactModel>();
            try
            {
                MYCONTACTBOOKEntities dc = new MYCONTACTBOOKEntities();
                var v = (from a in dc.Contacts
                         join b in dc.Countries on a.CountryID equals b.CountryID
                         join c in dc.States on a.StateID equals c.StateID
                         select new ContactModel
                         {
                             ContactID = a.ContactID,
                             FirstName = a.ContactPersonFname,
                             LastName = a.ContactPersonLname,
                             ContactNo1 = a.ContactNo1,
                             ContactNo2 = a.ContactNo2,
                             EmailID = a.EmailID,
                             Country = b.CountryName,
                             State = c.StateName,
                             Address = a.Address,
                             ImagePath = a.ImagePath
                         }).ToList();
                contacts = v;
                FetchedContact = contacts.Where(x => x.ContactID.Equals(contactID)).ToList();
                return FetchedContact;
            }
            catch(Exception ex)
            {
                return FetchedContact;
            }
        }

        [HttpPost]
        public IHttpActionResult PostNewContact(Contact c)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Not a valid model");

                using (var ctx = new MYCONTACTBOOKEntities())
                {
                    ctx.Contacts.Add(new Contact()
                    {
                        ContactPersonFname = c.ContactPersonFname,
                        ContactPersonLname = c.ContactPersonLname,
                        ContactNo1 = c.ContactNo1,
                        ContactNo2 = c.ContactNo1,
                        EmailID = c.EmailID,
                        CountryID = c.CountryID,
                        StateID = c.StateID,
                        Address = c.Address,
                        ImagePath = c.ImagePath
                    });

                    ctx.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                return BadRequest("Not a valid model");
            }

            return Ok();
        }



        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {

        }



        [HttpDelete]
        public IHttpActionResult DeleteProduct(int id)
        {
            try
            {

                if (id <= 0)
                    return BadRequest("Not a valid contact id");

                using (var ctx = new MYCONTACTBOOKEntities())
                {
                    var contact = ctx.Contacts
                        .Where(c => c.ContactID == id)
                        .FirstOrDefault();

                    ctx.Entry(contact).State = System.Data.Entity.EntityState.Deleted;
                    ctx.SaveChanges();
                }

            }
            catch (Exception)
            {
                return BadRequest("Not a valid contact id");
            }

            return Ok();
        }

    }
}
