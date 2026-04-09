using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamCare.Models
{
    public class AttemptQuestion
    {
        public long AttemptQuestionId { get; set; }
        public long StudentExamId { get; set; }
        public int QuestionId { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TimeSpent { get; set; }
        public int AnswerChangedCount { get; set; }
        public bool IsCorrect { get; set; } 
        public DateTime? FirstActionTime { get; set; }
        public DateTime? LastActionTime { get; set; }
        public DateTime? CurrentVisitStartTime { get; set; }

        // dữ liệu đi kèm cho từng câu
        public List<AnswerLog> AnswerLogs { get; set; } = new();
        public StudentAnswer? CurrentAnswer { get; set; }
    }

    public class AnswerLog
    {
        public long LogId { get; set; }
        public long AttemptQuestionId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? SelectedOption { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StudentAnswer
    {
        public long Id { get; set; }
        public long AttemptQuestionId { get; set; }
        public string? SelectedOption { get; set; }
        public string? ShortAnswer { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
