using IdentityMessageBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityMessageBoard.DataAccess
{
    public class MessageBoardContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }

        public MessageBoardContext(DbContextOptions<MessageBoardContext> options) : base(options)
        {

        }
    }
}
