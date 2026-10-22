using library.Data;
using library.Models;
using Microsoft.AspNetCore.Mvc;

namespace library.Controllers;

public class NoteController : Controller
{

    private readonly NoteRepository _notes;
    // ASP.NET Core hands in the repository, because Program.cs registered it.
    public NoteController(NoteRepository notes)
    {
        _notes = notes;
    }

    // GET /Note : the list of notes.
    public async Task<IActionResult> Index()
    {
        var notes = await _notes.GetAllAsync();
        return View(notes); // Views/Note/Index.cshtml
    }

    // GET /Note/Create : an empty form.
    public IActionResult Create()
    {
        return View(new Note());
    }

    // POST /Note/Create : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Note note)
    {
        if (!ModelState.IsValid) // a required field was left empty
        {
            return View(note);
        }
        await _notes.AddAsync(note);
        return RedirectToAction(nameof(Index));
    }

    // GET /Note/Edit/5 : the form, filled with note 5.
    public async Task<IActionResult> Edit(long id)
    {
        var note = await _notes.GetByIdAsync(id);
        if (note == null)
        {
            return NotFound();
        }
        return View(note);
    }

    // POST /Note/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Note note)
    {
        if (!ModelState.IsValid)
        {
            return View(note);
        }
        await _notes.UpdateAsync(note);
        return RedirectToAction(nameof(Index));
    }

    // GET /Note/Delete/5 : ask "are you sure?"
    public async Task<IActionResult> Delete(long id)
    {
        var note = await _notes.GetByIdAsync(id);
        if (note == null)
        {
            return NotFound();
        }
        return View(note);
    }

    // POST /Note/Delete/5 : the delete was confirmed.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _notes.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}