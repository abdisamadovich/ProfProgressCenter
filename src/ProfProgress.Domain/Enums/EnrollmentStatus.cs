namespace ProfProgress.Domain.Enums;

public enum EnrollmentStatus
{
    Pending = 0,    // Ariza berildi, tasdiqlanmagan
    Active = 1,     // Faol o'qiyapti
    Completed = 2,  // Tugatdi
    Cancelled = 3   // Bekor qilindi
}