using System;

namespace library.Models;

public class Note
{
    public long NoteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public DateTime? CreatedOn { get; set; }
}
