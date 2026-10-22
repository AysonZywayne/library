using library.Models;
using Npgsql;

namespace library.Data;

// Every SQL statement about notes lives in this one class.
public class NoteRepository
{
    private readonly NpgsqlDataSource _dataSource;

    // ASP.NET Core hands in the data source that Program.cs registered.
    public NoteRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row of the reader into a Note.
    // The positions 0 to 3 match the column order in the SELECT statement below.
    private static Note ReadNote(NpgsqlDataReader reader)
    {
        return new Note
        {
            // FIXED: Mapped to Note fields instead of Book fields
            NoteId = reader.GetInt64(0),
            Title = reader.GetString(1),
            Body = reader.IsDBNull(2) ? null : reader.GetString(2),
            CreatedOn = reader.IsDBNull(3) ? null : reader.GetDateTime(3)
        };
    }

    // READ: every note, sorted by title.
    public async Task<List<Note>> GetAllAsync()
    {
        // FIXED: Swapped out book schema tables/columns for a notes schema table'
        const string sql = "SELECT note_id, title, body, created_on FROM lending.note ORDER BY title;";



        // FIXED: Instantiated the list using Note objects
        var notes = new List<Note>();

        // The command borrows a connection; 'await using' gives it back when the method ends.
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) // once for each row
        {
            // FIXED: Appended rows directly to the local 'notes' list variable
            notes.Add(ReadNote(reader));
        }

        return notes;
    }

    // CREATE: insert a new note. The database chooses its note_id.
    public async Task AddAsync(Note note)
    {
        const string sql = "INSERT INTO lending.note (title, body) " +
                           "VALUES (@title, @body);";

        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue("title", note.Title);

        // A C# null must be sent as DBNull.Value, which is how a database NULL is written.
        command.Parameters.AddWithValue("body", (object?)note.Body ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(); // runs SQL that returns no rows
    }

    // READ: one note, or null when no note has that id.
    public async Task<Note?> GetByIdAsync(long id)
    {
        const string sql = "SELECT note_id, title, body, created_on FROM lending.note WHERE note_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id); // @id in the SQL receives this value
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadNote(reader) : null;
    }

    // UPDATE: change every column of one note.
    public async Task UpdateAsync(Note note)
    {
        const string sql = "UPDATE lending.note " +
                           "SET title = @title, body = @body " +
                           "WHERE note_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue("title", note.Title);
        command.Parameters.AddWithValue("body", (object?)note.Body ?? DBNull.Value);
        command.Parameters.AddWithValue("id", note.NoteId);

        await command.ExecuteNonQueryAsync();
    }

    // DELETE: remove one note.
    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM lending.note WHERE note_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync();
    }
}
