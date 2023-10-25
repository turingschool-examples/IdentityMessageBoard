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

        public MessagesController(MessageBoardContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var messages = _context.Messages
                .Include(m => m.Author)
                .OrderBy(m => m.ExpirationDate)
                .ToList()
                .Where(m => m.IsActive()); // LINQ Where(), not EF Where()

            return View(messages);
        }

        [Authorize(Roles = "Admin,SuperUser")]
        public IActionResult AllMessages()
        {
            var allMessages = new Dictionary<string, List<Message>>()
            {
                { "active" , new List<Message>() },
                { "expired", new List<Message>() }
            };

            foreach (var message in RoleRelatedMessages())
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

        [Authorize]
        public IActionResult New()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(string userId, string content, int expiresIn)
        {
            var user = _context.Users.Find(userId);
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

        [Authorize(Roles = "SuperUser")]
        public IActionResult Delete(int messageId)
        {
            var message = _context.Messages
                .Include(m => m.Author)
                .Where(m => m.Id == messageId)
                .First();

            if (message is null || message.Author.Id != _userManager.GetUserId(User))
            {
                return BadRequest();
            }

            _context.Messages.Remove(message);
            _context.SaveChanges();

            return RedirectToAction("AllMessages");
        }

        private List<Message> RoleRelatedMessages()
        {
            List<Message> messages;

            if (User.IsInRole("SuperUser"))
            {
                messages = _context.Messages
                    .Include(m => m.Author)
                    .Where(m => m.Author.Id == _userManager.GetUserId(User)).ToList();
            }
            else
            {
                messages = _context.Messages.Include(m => m.Author).ToList();
            }

            return messages;
        }
    }
}
