﻿namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    public interface IUserRepository
    {
        Task<IEnumerable<IdSrvUserDTO>> GetAll();

        Task<IdSrvUserDTO> GetByIdAsync(Guid id);

        Task<IdSrvUserDTO> GetByAuthInfoAsync(IdSrvUserAuthDTO userAuth);

        Task<RepositoryResponse> CreateAsync(NewIdSrvUserDTO user);

        Task<RepositoryResponse> DeleteAsync(Guid id);

        Task<RepositoryResponse> UpdateAsync(IdSrvUserDTO user);

        Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDTO password);
    }
}