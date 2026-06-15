namespace ProfProgress.Domain.Enums;

public enum AttemptStatus
{
    InProgress = 0,  // Boshlangan, hali topshirilmagan
    Submitted = 1,   // Javoblar yuborilgan va baholangan
    Expired = 2      // Vaqt tugadi, javob berilmadi
}
