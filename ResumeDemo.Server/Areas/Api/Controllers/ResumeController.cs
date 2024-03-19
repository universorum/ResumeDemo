using System.Net.Mime;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeDemo.Models;
using ResumeDemo.Server.Areas.Api.Models;
using ResumeDemo.Services;

namespace ResumeDemo.Server.Areas.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResumeController(ILogger<ResumeController> logger, ResumeManager manager, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<Resume>(StatusCodes.Status200OK)]
    [ProducesResponseType<Resume>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Pagination<Resume>>> GetResumes(
        [FromQuery] string? name   = null,
        [FromQuery] string? title  = null,
        [FromQuery] int?    after  = default,
        [FromQuery] int?    before = default)
    {
        if (before.HasValue && after.HasValue) { return BadRequest(); }

        return await manager.GetPaginationAsync(name: name,
            title: title,
            after: after,
            before: before,
            ct: HttpContext.RequestAborted);
    }

    [HttpGet("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<Resume>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resume>> GetResume(int id)
    {
        var result = await manager.GetByIdAsync(id, ct: HttpContext.RequestAborted);
        return result == null ? NotFound() : result;
    }

    [HttpPut("{id}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutResume(int id, ResumeRequest resume)
    {
        var entity = mapper.Map<Resume>(resume);
        entity.Id = id;

        try { await manager.UpdateAsync(entity); }
        catch (DbUpdateConcurrencyException) { return Conflict(); }
        catch (DbUpdateException) { return NotFound(); }

        return NoContent();
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<Resume>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resume>> PostResume([FromBody] ResumeRequest resume)
    {
        var entity = mapper.Map<Resume>(resume);
        await manager.AddAsync(entity);

        return CreatedAtAction("GetResume", new { id = entity.Id }, entity);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteResume([FromRoute] int id, [FromQuery] Guid version)
    {
        try { await manager.DeleteByIdAsync(id, version); }
        catch (DbUpdateConcurrencyException) { return Conflict(); }
        catch (DbUpdateException) { return NotFound(); }

        return NoContent();
    }
}