using LibraryApplication.DTOs;
using LibraryApplication.DTOs.BookDtos;
using LibraryApplication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _service;
    public BooksController(IBookService service) => _service = service;

    [HttpGet]
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookDto>>> GetAll(
    [FromQuery] string? search,
    [FromQuery] int? categoryId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);  
        return Ok(await _service.SearchAsync(search, categoryId, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookDto>> Update(int id, UpdateBookDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}