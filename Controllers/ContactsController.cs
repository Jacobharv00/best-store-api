using ecommerce.Models;
using ecommerce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ContactsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("subjects")]
        public IActionResult GetSubjects()
        {
            var listSubjects = context.Subjects.ToList();
            return Ok(listSubjects);
        }

        [HttpGet]
        public IActionResult GetContacts()
        {
            var contacts = context.Contacts.Include(c => c.Subject).ToList();

            return Ok(contacts);
        }

        [HttpGet("{id}")]
        public IActionResult GetContactById(int id)
        {
            var contact = context.Contacts.Include(c => c.Subject).FirstOrDefault(c => c.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            return Ok(contact);
        }

        [HttpPost]
        public IActionResult CreateContact(ContactDto newContact)
        {
            var subject = context.Subjects.Find(newContact.SubjectId);
            if(subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a valid subject");
                return BadRequest(ModelState);
            }

            Contact contact = new Contact()
            {
                FirstName = newContact.FirstName,
                LastName = newContact.LastName,
                Email = newContact.Email,
                Phone = newContact.Phone ?? "",
                Subject = subject,
                Message = newContact.Message,
                CreatedAt = DateTime.Now
            };

            context.Contacts.Add(contact);
            context.SaveChanges();

            return Ok(contact);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateContact(int id, ContactDto updatedContact)
        {
            var contact = context.Contacts.Find(id);

            if (contact == null)
            {
                return NotFound();
            }

            var subject = context.Subjects.Find(updatedContact.SubjectId);
            if(subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a valid subject");
                return BadRequest(ModelState);
            }

            contact.FirstName = updatedContact.FirstName;
            contact.LastName = updatedContact.LastName;
            contact.Email = updatedContact.Email;
            contact.Phone = updatedContact.Phone ?? "";
            contact.Subject = subject;
            contact.Message = updatedContact.Message;

            context.SaveChanges();

            return Ok(contact);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteContact(int id)
        {
            // 2 Queries
            // var contact = context.Contacts.Find(id);

            // if (contact == null)
            // {
            //     return NotFound();
            // }

            // context.Contacts.Remove(contact);
            // context.SaveChanges();

            // return Ok();

            // 1 Query
            try
            {
            var contact = new Contact { Id = id, Subject = new Subject() };
            context.Contacts.Remove(contact);
            context.SaveChanges();
            }
            catch (Exception)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
