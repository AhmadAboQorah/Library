using Library.Extensions;
using LibraryApplication.DTOs.CreateLoanDtos;
using LibraryApplication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loans;
    public LoansController(ILoanService loans) => _loans = loans;

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<LoanDto>> Borrow(CreateLoanDto dto)
    {
        var loan = await _loans.BorrowAsync(User.GetUserId(), dto.BookId);
        return StatusCode(StatusCodes.Status201Created, loan);
    }

    [HttpPut("{id:int}/return")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<LoanDto>> Return(int id) =>
        Ok(await _loans.ReturnAsync(User.GetUserId(), id));

    [HttpGet("my")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<List<LoanDto>>> My() =>
        Ok(await _loans.GetMyLoansAsync(User.GetUserId()));

    [HttpGet("overdue")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<LoanDto>>> Overdue() =>
        Ok(await _loans.GetOverdueAsync());
}