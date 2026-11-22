namespace VeloCarbon.Core.Services;

/// <summary>Kesalahan aturan bisnis / validasi yang pesannya aman ditampilkan ke pengguna.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
