using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeDemo.Services;

namespace ResumeDemo.Server.Controllers;

public class ResumesController(ResumeManager manager) : Controller
{
    public IActionResult Index() { return View(); }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) { return NotFound(); }

        var resume = await manager.GetByIdAsync(id.Value, ct: HttpContext.RequestAborted);
        if (resume == null) { return NotFound(); }

        return View(resume);
    }

    public IActionResult Create()
    {
        ViewData["Title"]    = "Create";
        ViewData["endpoint"] = "/api/resume";
        ViewData["method"]   = "POST";
        return View(nameof(Edit));
    }

    public async Task<IActionResult> Edit([FromRoute] int? id)
    {
        if (id == null) { return NotFound(); }

        var resume = await manager.GetByIdAsync(id.Value);
        if (resume == null) { return NotFound(); }

        ViewData["Title"]    = "Edit";
        ViewData["endpoint"] = $"/api/resume/{id}";
        ViewData["method"]   = "PUT";

        return View(resume);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) { return NotFound(); }

        var resume = await manager.GetByIdAsync(id.Value);
        if (resume == null) { return NotFound(); }

        return View(resume);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, Guid version)
    {
        try { await manager.DeleteByIdAsync(id, version); }
        catch (DbUpdateConcurrencyException) { }
        catch (DbUpdateException) { }

        return RedirectToAction(nameof(Index));
    }
}