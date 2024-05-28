using ecommerce.EntityFramework;
using ecommerce.Middleware;
using ecommerce.Models;
using ecommerce.service;
using ecommerce.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Controller;

[ApiController]
[Route("/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthService _authService;

    public UserController(UserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 5)
    {
        var users = await _userService.GetUsers();
        if (!users.Any())
        {
            throw new NotFoundException("There is no users");
        }

        return ApiResponse.Success(
            users.Skip((page - 1) * limit).Take(limit).ToList(),
            "All users inside E-commerce system"
        );
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var found =
            await _userService.GetUserById(id) ?? throw new NotFoundException("The user not found");

        return ApiResponse.Success(found, "User Detail");
    }

    [HttpPost("signIn")]
    public async Task<IActionResult> SignIn(SignIn signInInfo)
    {
        var userSignIn = await _userService.SignIn(signInInfo);
        var token = _authService.GenerateJwt(userSignIn!);
        Console.WriteLine($"{token}");

        return ApiResponse.Success(new { token, userSignIn }, "User is SignIn successfully");
    }

    [HttpPost("signUp")]
    public async Task<IActionResult> CreateAccount(UserModel newUser)
    {
        var user = await _userService.CreateAccount(newUser);
        return ApiResponse.Created(user, "The user is Added");
    }

    [HttpPut("{id:guid}")]
    //[Authorize]
  //  [Authorize(Policy = "RequiredNotBanned")]
    public async Task<IActionResult> UpdateUser(Guid id, UserModel updateData)
    {
        var found =
            await _userService.UpdateUser(id, updateData)
            ?? throw new NotFoundException("The user not found");

        return ApiResponse.Success(found, "User updated");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "RequiredNotBanned")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deleted = await _userService.DeleteUser(id);
        if (!deleted)
        {
            throw new NotFoundException("The user not found");
        }
        return ApiResponse.Success(id, "User Deleted");
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "RequiredNotBanned")]
    public async Task<IActionResult> UpdateUserStatus(
        Guid id,
        UserStatusUpdateModel statusUpdateModel
    )
    {
        var updatedUser =
            await _userService.UpdateUserStatus(id, statusUpdateModel)
            ?? throw new NotFoundException("The user not found");

        return ApiResponse.Success(updatedUser, "User status updated successfully");
    }
}
