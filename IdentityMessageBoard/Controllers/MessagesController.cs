using IdentityMessageBoard.DataAccess;
using IdentityMessageBoard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace IdentityMessageBoard.Controllers
{
    public class MessagesController : Controller
    {
        private readonly MessageBoardContext _context;

        public MessagesController(MessageBoardContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var messages = _context.Messages
                .OrderBy(m => m.ExpirationDate)
                .ToList()
                .Where(m => m.IsActive()); // LINQ Where(), not EF Where()

            return View(messages);
        }

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

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(int userId, string content, int expiresIn)
        {
            _context.Messages.Add(
                new Message()
                {
                    Content = content,
                    ExpirationDate = DateTime.UtcNow.AddDays(expiresIn)
                });

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
