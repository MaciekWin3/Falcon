﻿using Falcon.Server.Features.Messages.Models;
using Microsoft.EntityFrameworkCore;

namespace Falcon.Server.Features.Messages.Repositories
{
    public class MessageRepository : BaseRepository, IMessageRepository
    {
        public MessageRepository(FalconDbContext context) : base(context)
        {
        }

        public async Task CreateAsync(Message message)
        {
            await context.Messages.AddAsync(message);
            var x = await context.SaveChangesAsync();
            Console.WriteLine(x);
        }

        public async Task<List<Message>> GetAllAsync()
        {
            return await context.Messages.ToListAsync();
        }
    }
}