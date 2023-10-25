using IdentityMessageBoard.DataAccess;
using IdentityMessageBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IdentityMessageBoard.Controllers
{
    public class MessagesController : Controller
    {
        private readonly MessageBoardContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(MessageBoardContext context, UserManager<ApplicationUser> userManager )
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var messages = _context.Messages
                .Include(m => m.Author)
                .OrderBy(m => m.ExpirationDate)
                .ToList()
                .Where(m => m.IsActive()); // LINQ Where(), not EF Where()

            return View(messages);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AllMessages()
        {
            var allMessages = new Dictionary<string, List<Message>>()
            {
                { "active" , new List<Message>() },
                { "expired", new List<Message>() }
            };

            foreach (var message in _context.Messages)
            {
                if (message.IsActive())
                {
                    allMessages["active"].Add(message);
                }
                else
                {
                    allMessages["expired"].Add(message);
                }
            }


            return View(allMessages);
        }

        [Authorize(Roles = "Super User")]
        [Route("/messages/yourmessages")]
        public IActionResult YourMessages()
        {
            var userId = _userManager.GetUserId(User);
            var messages = _context.Messages.Include(m => m.Author).Where(m => m.Author.Id == userId);

            var allMessages = new Dictionary<string, List<Message>>()
            {
                { "active" , new List<Message>() },
                { "expired", new List<Message>() }
            };

            foreach (var message in messages)
            {
                if (message.IsActive())
                {
                    allMessages["active"].Add(message);
                }
                else
                {
                    allMessages["expired"].Add(message);
                }
            }

            return View(allMessages);
        }

        [Authorize(Roles = "Super User")]
        [Route("/messages/{id:int}/edit")]
        public IActionResult Edit(int id)
        {
            var message = _context.Messages.Include(m => m.Author).FirstOrDefault(m => m.Id == id);
            var userId = _userManager.GetUserId(User);
            if (message.Author.Id == userId)
            {
                return View(message);
            }
            else
            {
                return Redirect("/messages/yourmessages");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Super User")]
        [Route("/messages/{id:int}/update")]
        public IActionResult Update(int id, string content, int? expiresIn)
        {
            var message = _context.Messages.Include(m => m.Author).FirstOrDefault(m => m.Id == id);
            var userId = _userManager.GetUserId(User);
            if (message.Author.Id == userId)
            {
                message.Content = content;
                if (expiresIn != null)
                {
                    message.ExpirationDate = DateTime.UtcNow.AddDays((int)expiresIn);
                }
                _context.Messages.Update(message);
                _context.SaveChanges();
                return Redirect("/messages/yourmessages");
            }
            else
            {
                return Redirect("/messages/yourmessages");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Super User")]
        [Route("/messages/{id:int}/delete")]
        public IActionResult Delete(int id)
        {
            var message = _context.Messages.Include(m => m.Author).FirstOrDefault(m => m.Id == id);
            var userId = _userManager.GetUserId(User);
            if (message.Author.Id == userId)
            {
                _context.Messages.Remove(message);
                _context.SaveChanges();
                return Redirect("/messages/yourmessages");
            }
            else
            {
                return Redirect("/messages/yourmessages");
            }
        }

        [Authorize]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(string userId, string content, int expiresIn)
        {
            
            var user = _context.ApplicationUsers.Find(userId);

            _context.Messages.Add(
                new Message()
                {
                    Content = content,
                    ExpirationDate = DateTime.UtcNow.AddDays(expiresIn),
                    Author = user
                });

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
