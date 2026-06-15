namespace ProfProgress.Application.Common.Exceptions;

/// <summary>Biznes-qoidalar buzilganda tashlanadigan bazaviy xatolik.</summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>Resurs topilmaganda (HTTP 404).</summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404)
    {
    }

    public NotFoundException(string name, object key)
        : base($"{name} (\"{key}\") topilmadi.", 404) { }
}

/// <summary>Autentifikatsiya/parol xatosi (HTTP 401).</summary>
public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message) : base(message, 401)
    {
    }
}

/// <summary>Ruxsat yetarli emas (HTTP 403).</summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403)
    {
    }
}

/// <summary>Konflikt — masalan, takroriy email (HTTP 409).</summary>
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409)
    {
    }
}