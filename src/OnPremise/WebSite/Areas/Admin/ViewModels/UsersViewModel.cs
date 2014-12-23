using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Thinktecture.IdentityServer.Models;
using Thinktecture.IdentityServer.Repositories;

namespace Thinktecture.IdentityServer.Web.Areas.Admin.ViewModels
{
    public class UsersViewModel
    {
        [Import]
        public IConfigurationRepository ConfigurationRepository { get; set; }

        private readonly IUserManagementRepository _userManagementRepository;

        public UsersViewModel(IUserManagementRepository userManagementRepository, int currentPage, string filter)
        {
            Container.Current.SatisfyImportsOnce(this);

            _userManagementRepository = userManagementRepository;
            Filter = filter;

            Init(currentPage, filter);
            if (TotalPages < CurrentPage)
            {
                Init(TotalPages, filter);
            }
        }

        private void Init(int currentPage, string filter)
        {
            if (currentPage <= 0) CurrentPage = 1;
            else CurrentPage = currentPage;

            const int rows = 20;
            int pageIndex = (currentPage - 1);
            IEnumerable<User> users;
            if (String.IsNullOrEmpty(filter))
            {
                int total;
                users = _userManagementRepository.GetUsers(pageIndex, rows, out total);
                Total = total;
            }
            else
            {
                int total;
                users = _userManagementRepository.GetUsers(filter, pageIndex, rows, out total);
                Total = total;
            }

            if (Total < rows)
            {
                Showing = Total;
            }
            else
            {
                Showing = rows;
            }
            Users = users.Select(x => new UserModel { Username = x.UserName, IsLockedOut = x.IsLockedOut }).ToArray();

        }

        public UserModel[] Users {get;set;}
        public string Filter { get; set; }
        public int Total { get; set; }
        public int Showing { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages
        {
            get
            {
                if (Total <= 0 || Showing <= 0) return 1;
                return (int)Math.Ceiling((1.0*Total) / Showing);
            }
        }

        public bool IsProfileEnabled
        {
            get
            {
                return System.Web.Profile.ProfileManager.Enabled;
            }
        }
        
        public bool IsOAuthRefreshTokenEnabled
        {
            get
            {
                return ConfigurationRepository.OAuth2.Enabled &&
                    (ConfigurationRepository.OAuth2.EnableCodeFlow || ConfigurationRepository.OAuth2.EnableResourceOwnerFlow);
            }
        }
    }

    public class UserModel
    {
        public string Username { get; set; }
        public bool IsSelectedForAction { get; set; }
        public bool IsLockedOut { get; set; }
    }
}