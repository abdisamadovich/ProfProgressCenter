using System.Text;
using ProfProgress.Domain.Entities;

namespace ProfProgress.Application.Features.Tests;

public static class GradingHelper
{
    /// <summary>
    /// Talaba yuborgan xom matnni ("1a2b3c..." yoki "abcd...") variant harflari
    /// ketma-ketligiga aylantiradi: faqat A–E harflarini ajratib oladi.
    /// </summary>
    public static string Normalize(string raw, int optionCount)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        char maxLetter = (char)('A' + Math.Clamp(optionCount, 2, 8) - 1);
        var sb = new StringBuilder();
        foreach (var ch in raw)
        {
            var up = char.ToUpperInvariant(ch);
            if (up >= 'A' && up <= maxLetter)
                sb.Append(up);
        }
        return sb.ToString();
    }

    /// <summary>Berilgan savol raqami uchun ball (blokka qarab, blok bo'lmasa 1 ball).</summary>
    public static decimal PointsForQuestion(IEnumerable<TestBlock> blocks, int questionNumber)
    {
        foreach (var block in blocks)
        {
            if (questionNumber >= block.FromQuestion && questionNumber <= block.ToQuestion)
                return block.PointsPerQuestion;
        }
        return 1m;
    }

    /// <summary>Testning maksimal mumkin bo'lgan bali.</summary>
    public static decimal MaxScore(Test test)
    {
        decimal sum = 0;
        for (int q = 1; q <= test.QuestionCount; q++)
            sum += PointsForQuestion(test.Blocks, q);
        return sum;
    }

    /// <summary>Javoblarni kalit bilan solishtirib baholaydi.</summary>
    public static (int correctCount, decimal totalScore, List<int> wrongQuestions) Grade(Test test, string normalizedAnswers)
    {
        int correct = 0;
        decimal score = 0;
        var wrong = new List<int>();

        for (int i = 0; i < test.QuestionCount; i++)
        {
            char studentAns = i < normalizedAnswers.Length ? normalizedAnswers[i] : ' ';
            char correctAns = i < test.AnswerKey.Length ? char.ToUpperInvariant(test.AnswerKey[i]) : ' ';

            if (studentAns != ' ' && studentAns == correctAns)
            {
                correct++;
                score += PointsForQuestion(test.Blocks, i + 1);
            }
            else
            {
                wrong.Add(i + 1);
            }
        }

        return (correct, score, wrong);
    }
}
