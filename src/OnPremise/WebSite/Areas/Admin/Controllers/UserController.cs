using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Authorization.Mvc;
using Thinktecture.IdentityServer.Repositories;
using Thinktecture.IdentityServer.Web.Areas.Admin.ViewModels;

namespace Thinktecture.IdentityServer.Web.Areas.Admin.Controllers
{
    [ClaimsAuthorize(Constants.Actions.Administration, Constants.Resources.Configuration)]
    public class UserController : Controller
    {
        [Import]
        public IUserManagementRepository UserManagementRepository { get; set; }

        public UserController()
        {
            Container.Current.SatisfyImportsOnce(this);
        }

        public UserController(IUserManagementRepository userManagementRepository)
        {
            UserManagementRepository = userManagementRepository;
        }

        public ActionResult Index(int page = 1, string filter = null)
        {
            var vm = new UsersViewModel(UserManagementRepository, page, filter);
            return View("Index", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int page, string filter, string action, UserModel[] users)
        {
            if (action == "unlock") return Unlock(page, filter, users);
            if (action == "new") return Create();
            if (action == "delete") return Delete(page, filter, users);

            ModelState.AddModelError("", Resources.UserController.InvalidAction);
            var vm = new UsersViewModel(UserManagementRepository, page, filter);
            return View("Index", vm);
        }

        private ActionResult Unlock(int page, string filter, UserModel[] list)
        {
            if (ModelState.IsValid)
            {
                foreach (var name in list.Where(x => x.IsSelectedForAction).Select(x => x.Username))
                {
                    UserManagementRepository.UnlockUser(name);
                }

                TempData["Message"] = Resources.UserController.UsersUnlocked;
                return RedirectToAction("Index", new { page, filter });
            }
            return Index(page, filter);
        }

        public ActionResult Create()
        {
            var rolesvm = new UserRolesViewModel(UserManagementRepository, String.Empty);
            return View("Create", new UserInputModel {Roles = rolesvm.RoleAssignments});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UserManagementRepository.CreateUser(model.Username, model.Password, model.Email);
                    if (model.Roles != null)
                    {
                        var roles = model.Roles.Where(x => x.InRole).Select(x => x.Role);
                        if (roles.Any())
                        {
                            UserManagementRepository.SetRolesForUser(model.Username, roles);
                        }
                    }
                    TempData["Message"] = Resources.UserController.UserCreated;
                    return RedirectToAction("Index", new { filter = model.Username });
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch
                {
                    ModelState.AddModelError("", Resources.UserController.ErrorCreatingUser);
                }
            }

            return View("Create", model);
        }

        private ActionResult Delete(int page, string filter, UserModel[] list)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var name in list.Where(x => x.IsSelectedForAction).Select(x => x.Username))
                    {
                        UserManagementRepository.DeleteUser(name);
                    }
                    TempData["Message"] = Resources.UserController.UsersDeleted;
                    return RedirectToAction("Index", new { page, filter });
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch
                {
                    ModelState.AddModelError("", Resources.UserController.ErrorDeletingUser);
                }
            }
            return Index(page, filter);
        }

        public ActionResult Roles(string username)
        {
            var vm = new UserRolesViewModel(this.UserManagementRepository, username);
            return View("Roles", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Roles(string username, UserRoleAssignment[] roleAssignments)
        {
            var vm = new UserRolesViewModel(UserManagementRepository, username);
            if (ModelState.IsValid)
            {
                try
                {
                    var currentRoles =
                        roleAssignments.Where(x => x.InRole).Select(x => x.Role);
                    UserManagementRepository.SetRolesForUser(username, currentRoles);
                    TempData["Message"] = Resources.UserController.RolesAssignedSuccessfully;
                    return RedirectToAction("Roles", new { username });
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch
                {
                    ModelState.AddModelError("", Resources.UserController.ErrorAssigningRoles);
                }
            }

            return View("Roles", vm);
        }

        public new ActionResult Profile(string username)
        {
            var vm = new UserProfileViewModel(username);
            return View(vm);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(string username, ProfilePropertyInputModel[] profileValues)
        {
            var vm = new UserProfileViewModel(username, profileValues);

            if (vm.UpdateProfileFromValues(ModelState))
            {
                TempData["Message"] = Resources.UserController.ProfileUpdated;
                return RedirectToAction("Profile", new { username });
            }

            return View(vm);
        }

        public ActionResult ChangePassword(string username)
        {
            UserPasswordModel model = new UserPasswordModel();
            model.Username = username;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(UserPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UserManagementRepository.SetPassword(model.Username, model.Password);
                    TempData["Message"] = Resources.UserController.ProfileUpdated;
                    return RedirectToAction("Index");
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch
                {
                    ModelState.AddModelError("", "Error updating password");
                }
            }
            
            return View("ChangePassword", model);
        }
    }
}
