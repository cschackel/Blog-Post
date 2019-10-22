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

        //Adds Errors to model State
        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        // Gets editable user ID and goes to Edit Page
        public async Task<IActionResult> Edit(string id)
        {
            AppUser editUser = await userManager.FindByIdAsync(id);
            if (editUser != null)
            {
                return View(editUser);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //Edits the User Profile based on Post Data
        [HttpPost]  //Post Method
        public async Task<IActionResult> Edit(string id, string email, string password, string userName)  //Inputs Recieved through Post
        {
            AppUser user = await userManager.FindByIdAsync(id);  //Get User
            if (user != null)  //If Valid User
            {
                user.Email = email;
                user.UserName = userName;  //Give User Updated Name & Email
                IdentityResult validEmailAndUsername = await userValidator.ValidateAsync(userManager, user);  //Validate
                if (!validEmailAndUsername.Succeeded)  //If Fail
                {
                    AddErrorsFromResult(validEmailAndUsername); //Add Errors
                }
                //
               // user.UserName = userName;
                //IdentityResult validName = await userValidator.ValidateAsync(userManager, user);
                //if (!validName.Succeeded)
                //{
                //    AddErrorsFromResult(validEmail);
                //}
                //
                IdentityResult validPass = null;  //Holds Validity of Password
                if (!string.IsNullOrEmpty(password))  //If change to password
                {
                    validPass = await passwordValidator.ValidateAsync(userManager, user, password); //Checj Validity
                    if (validPass.Succeeded) // If Valid update user password hash
                    {
                        user.PasswordHash = passwordHasher.HashPassword(user, password);
                    }
                    else //Else Generate Errors
                    {
                        AddErrorsFromResult(validPass);
                    }
                }
                if ((validEmailAndUsername.Succeeded && validPass == null) || (validEmailAndUsername.Succeeded && password != string.Empty && validPass.Succeeded))  // If Completely Valid
                {
                    IdentityResult result = await userManager.UpdateAsync(user); //Update DB
                    if (result.Succeeded) //If Success, redirect
                    {
                        return RedirectToAction("Index");
                    }
                    else  // If fail, send back to form
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else  // If no user, add error
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View(user); // Return to form
        }
    }
}