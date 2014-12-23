using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Thinktecture.IdentityServer.Models;
using Thinktecture.IdentityServer.Repositories;
using Thinktecture.IdentityServer.Web.Areas.Admin.Resources;

namespace Thinktecture.IdentityServer.Web.Areas.Admin.ViewModels
{
    public class DelegationSettingsForUserViewModel
    {
        [Required]
        public string UserName { get; set; }
        public IEnumerable<DelegationSetting> DelegationSettings { get; set; }
        public IEnumerable<SelectListItem> AllUserNames { get; set; }

        public bool IsNew
        {
            get
            {
                return UserName == null;
            }
        }

        public DelegationSettingsForUserViewModel(IDelegationRepository delegationRepository, IUserManagementRepository userManagementRepository, string username)
        {
            int totalCount;
            var allnames =
                userManagementRepository.GetUsers(0, 100, out totalCount)
                    .Select(x => new SelectListItem
                    {
                        Text = x.UserName
                    }).ToList();
            allnames.Insert(0, new SelectListItem { Text = DelegationSettingsForUserInputModel.ChooseItem, Value = "" });
            AllUserNames = allnames;
            
            UserName = username;
            if (!IsNew)
            {
                var realmSettings = delegationRepository .GetDelegationSettingsForUser(UserName).ToArray();
                DelegationSettings = realmSettings;
            }
            else
            {
                DelegationSettings = new DelegationSetting[0];
            }
        }
    }
}