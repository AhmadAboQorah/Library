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
    public async Task<ActionResult<List<BookDto>>> GetAll([FromQuery] string? title, [FromQuery] int? categoryId) =>
        Ok(await _service.SearchAsync(title, categoryId));

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