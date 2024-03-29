﻿using Falcon.Client.Features.Auth.Models;

namespace Falcon.Client.Features.Auth
{
    public interface IAuthService
    {
        Task<string> LoginAsync(User user);
    }
}