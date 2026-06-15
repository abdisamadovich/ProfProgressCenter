namespace ProfProgress.Domain.Enums;

public enum TestStatus
{
    Draft = 0,       // Yaratilmoqda / qoralama
    Scheduled = 1,   // Rejalashtirilgan (boshlanishini kutmoqda)
    Active = 2,      // Hozir topshirilmoqda
    Finished = 3     // Yakunlangan
}
