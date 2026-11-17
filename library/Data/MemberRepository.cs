using library.Models;
using Npgsql;

namespace library.Data;
// Every SQL statement about books lives in this one class.
public class MemberRepository
{
    private readonly NpgsqlDataSource _dataSource;
    // ASP.NET Core hands in the data source that Program.cs registered.
    public MemberRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row of the reader into a Book.
    // The positions 0 to 3 match the column order in both SELECTs above.
    private static Member ReadMember(NpgsqlDataReader reader)
    {
        return new Member
        {
            MemberId = reader.GetInt64(0),
            FullName = reader.GetString(1),                           // Fixed: lowercase 'n' to match C# conventions/model
            Email = reader.IsDBNull(2) ? null : reader.GetString(2),   // Email can be NULL
            MemberType = reader.GetString(3)                          // Fixed: MemberType cannot be null, retrieved as String
        };
    }

    // READ: every member, with how many books each has out now.
    public async Task<List<Member>> GetAllAsync()
    {
        const string sql ="SELECT m.member_id, m.full_name, m.email, m.member_type, COUNT(l.loan_id) " +
        "FROM lending.member m " +
        "LEFT JOIN lending.loan l ON l.member_id = m.member_id AND l.returned_on IS NULL " +
        "GROUP BY m.member_id " +
        "ORDER BY m.full_name;";
        var members = new List<Member>();
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var member = ReadMember(reader);
            member.OpenLoans = reader.GetInt64(4);
            members.Add(member);
        }
        return members;
    }

    // CREATE: insert a new member. The database chooses its member_id.
    public async Task AddAsync(Member member)
    {
        const string sql = "INSERT INTO lending.member (full_name, email, member_type) " +
                           "VALUES (@full_name, @email, @member_type);";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("full_name", member.FullName);
        // email can be null, so it gets the DBNull check
        command.Parameters.AddWithValue("email", (object?)member.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("member_type", member.MemberType);
        await command.ExecuteNonQueryAsync();        // runs SQL that returns no rows
    }

    // READ: one member, or null when no member has that id.
    public async Task<Member?> GetByIdAsync(long id)
    {
        const string sql = "SELECT member_id, full_name, email, member_type FROM lending.member WHERE member_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadMember(reader); // Call your static mapping helper method
        }

        return null;
    }

    // UPDATE: change every column of one member.
    public async Task UpdateAsync(Member member)
    {
        const string sql = "UPDATE lending.member " +
                           "SET full_name = @full_name, email = @email, member_type = @member_type " +
                           "WHERE member_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("full_name", member.FullName);

        // email can be null, so it gets the DBNull check
        command.Parameters.AddWithValue("email", (object?)member.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("member_type", member.MemberType);
        command.Parameters.AddWithValue("id", member.MemberId);

        await command.ExecuteNonQueryAsync();
    }

    // DELETE: remove one book.
    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM lending.member WHERE member_id = @id;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync();
    }
}