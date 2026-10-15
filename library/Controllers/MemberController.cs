using library.Data;
using library.Models;
using Microsoft.AspNetCore.Mvc;

namespace library.Controllers;

public class MemberController : Controller
{
    private readonly MemberRepository _members;

    public MemberController(MemberRepository members)
    {
        _members = members;
    }

    // GET /Member : the list of members.
    public async Task<IActionResult> Index()
    {
        var members = await _members.GetAllAsync();
        return View(members);
    }

    // GET /Member/Create : an empty form.
    public IActionResult Create()
    {
        return View(new Member());
    }

    // FIXED POST ACTION: This handles saving the form submission!
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }

        await _members.AddAsync(member);
        return RedirectToAction(nameof(Index));
    }

    // GET /Member/Edit/5 : the form, filled with member 5.
    public async Task<IActionResult> Edit(long id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return View(member);
    }

    // POST /Member/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Member member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }

        await _members.UpdateAsync(member);
        return RedirectToAction(nameof(Index));
    }

    // GET /Member/Delete/5 : ask "are you sure?"
    public async Task<IActionResult> Delete(long id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return View(member);
    }

    // POST /Member/Delete/5 : the delete was confirmed.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _members.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
