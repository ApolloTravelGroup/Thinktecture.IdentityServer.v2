using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Thinktecture.IdentityServer.Models;
using Thinktecture.IdentityServer.Repositories;

namespace Thinktecture.IdentityServer.Web.Areas.Admin.ViewModels
{
    public class ClientCertificatesForUserViewModel
    {
        [Required]
        public string UserName { get; set; }
        public IEnumerable<ClientCertificate> Certificates { get; set; }
        public IEnumerable<SelectListItem> AllUserNames { get; set; }
        public ClientCertificate NewCertificate { get; set; }

        public bool IsNew
        {
            get
            {
                return UserName == null;
            }
        }

        public ClientCertificatesForUserViewModel(IClientCertificatesRepository clientCertificatesRepository, IUserManagementRepository userManagementRepository, string username)
        {            
            int totalCount;
            var allnames = 
                userManagementRepository.GetUsers(0, 100, out totalCount)
                .Select(x => new SelectListItem
                {
                    Text = x.UserName
                }).ToList();
            allnames.Insert(0, new SelectListItem { Text = Resources.ClientCertificatesForUserViewModel.ChooseItem, Value = "" });
            AllUserNames = allnames;
            
            UserName = username;
            NewCertificate = new ClientCertificate { UserName = username };
            if (!IsNew)
            {
                Certificates = clientCertificatesRepository.GetClientCertificatesForUser(UserName).ToArray();
            }
            else
            {
                Certificates = new ClientCertificate[0];
            }
        }
    }
}