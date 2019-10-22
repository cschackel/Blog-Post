using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blogs.Controllers
{
    public class AdminController : Controller
    {

        private UserManager<AppUser> userManager;
        private IUserValidator<AppUser> userValidator;
        private IPasswordHasher<AppUser> passwordHasher;
        private IPasswordValidator<AppUser> passwordValidator;
        public AdminController(UserManager<AppUser> usrMgr, IUserValidator<AppUser> userValid, IPasswordValidator<AppUser> passValid, IPasswordHasher<AppUser> passwordHash)
        {
            userManager = usrMgr;
            userValidator = userValid;
            passwordValidator = passValid;
            passwordHasher = passwordHash;
        }

        public ViewResult Index() => View(userManager.Users);

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = model.Name,
                    Email = model.Email
                };
                IdentityResult result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View("Index", userManager.Users);
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            AppUser editUser = await userManager.FindByIdAsync(id);
            if (editUser != null)
            {
                EditModel em = new EditModel();
                //em.ID = editUser.Id;
                //em.Name = editUser.UserName;
                //em.Email = editUser.Email;
                //em.Password = editUser.PasswordHash;
                return View(editUser);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string email, string password, string userName)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Email = email;
                user.UserName = userName;
                IdentityResult validEmailAndUsername = await userValidator.ValidateAsync(userManager, user);
                if (!validEmailAndUsername.Succeeded)
                {
                    AddErrorsFromResult(validEmailAndUsername);
                }
                //
               // user.UserName = userName;
                //IdentityResult validName = await userValidator.ValidateAsync(userManager, user);
                //if (!validName.Succeeded)
                //{
                //    AddErrorsFromResult(validEmail);
                //}
                //
                IdentityResult validPass = null;
                if (!string.IsNullOrEmpty(password))
                {
                    validPass = await passwordValidator.ValidateAsync(userManager, user, password);
                    if (validPass.Succeeded)
                    {
                        user.PasswordHash = passwordHasher.HashPassword(user, password);
                    }
                    else
                    {
                        AddErrorsFromResult(validPass);
                    }
                }
                if ((validEmailAndUsername.Succeeded && validPass == null) || (validEmailAndUsername.Succeeded && password != string.Empty && validPass.Succeeded))
                {
                    IdentityResult result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View(user);
        }
    }
}