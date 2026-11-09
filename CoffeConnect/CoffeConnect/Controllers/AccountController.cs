using CoffeConnect.Models;
using CoffeConnect.ViewModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm)
    {
        _userManager = um;
        _signInManager = sm;
    }

    [HttpGet] public IActionResult Register() => View();
    [HttpGet] public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            NombreCompleto = model.NombreCompleto,
            Region = model.Region
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Usuario");
            await _signInManager.SignInAsync(user, false);
            return RedirectToAction("Index", "Home");
        }
        foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
        return View(model);
    }

    // Login usando ApplicationUser
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        if (result.Succeeded) return RedirectToAction("Index", "Home");
        ModelState.AddModelError("", "Email o contraseña incorrectos");
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        var model = new EditProfileViewModel
        {
            Email = user.Email,
            NombreCompleto = user.NombreCompleto,
            Region = user.Region
        };
        return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        user.NombreCompleto = model.NombreCompleto;
        user.Region = model.Region;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = "Perfil actualizado correctamente";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public IActionResult DeleteAccount() => View();

    [HttpPost, ActionName("DeleteAccount")]
    [Authorize]
    public async Task<IActionResult> DeleteAccountConfirmed()
    {
        var user = await _userManager.GetUserAsync(User);
        await _signInManager.SignOutAsync();
        await _userManager.DeleteAsync(user);
        return RedirectToAction("Index", "Home");
    }
}