namespace library.Models;

public class Member
{
    public long MemberId { get; set; }

    // Make sure 'Fullname' matches what you wrote in your Repository
    public string FullName { get; set; } = string.Empty;

    // string? allows this property to accept null values
    public string? Email { get; set; }

    public string MemberType { get; set; } = string.Empty;
}
