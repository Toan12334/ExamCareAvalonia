using CommunityToolkit.Mvvm.ComponentModel;
using ExamCare.Helpers;
using LiveMarkdown.Avalonia;
using System;
using System.Collections.Generic;

namespace ExamCare.Models
{
    public class StudentExamResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StudentExamData? Data { get; set; }
    }

    public class StudentExamData
    {
        public int StudentExamId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public int AttemptNo { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int Duration { get; set; }
        public List<Question> Questions { get; set; } = new();
    }

    public class Question
    {
        private string _questionContent = string.Empty;
        public int Part { get; set; }
        public int QuestionOrder { get; set; }
        public int QuestionId { get; set; }
        public int TypeId { get; set; }

        public string QuestionContent
        {
            get => _questionContent;
            set => _questionContent = CleanRender.CleanMathSpaces(value ?? string.Empty);
        }

        public ObservableStringBuilder QuestionContentBuilder { get; } = new();
        public List<QuestionImage> Images { get; set; } = new();
        public List<QuestionOption> Options { get; set; } = new();
        public List<TrueFalseStatement> TrueFalseStatements { get; set; } = new();
        public string StudentAnswerText { get; set; } = string.Empty;

        public bool IsType1 => TypeId == 1;
        public bool IsType2 => TypeId == 2;
        public bool IsType3 => TypeId == 3;
    }

    public class QuestionImage
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public partial class QuestionOption : ObservableObject
    {
        [ObservableProperty]
        private bool isSelected;

        private string _optionContent = string.Empty;
        public int OptionId { get; set; }
        public string OptionLabel { get; set; } = string.Empty;

        public string OptionContent
        {
            get => _optionContent;
            set => _optionContent = CleanRender.CleanMathSpaces(value ?? string.Empty);
        }

        public ObservableStringBuilder OptionContentBuilder { get; } = new();
    }

    public partial class TrueFalseStatement : ObservableObject
    {
        private string _statementContent = string.Empty;

        public int StatementId { get; set; }
        public string StatementLabel { get; set; } = string.Empty;

        public string StatementContent
        {
            get => _statementContent;
            set => _statementContent = CleanRender.CleanMathSpaces(value ?? string.Empty);
        }

        public ObservableStringBuilder StatementContentBuilder { get; } = new();

        [ObservableProperty]
        private bool isTrueSelected;

        [ObservableProperty]
        private bool isFalseSelected;

        // "T" / "F" / null
        [ObservableProperty]
        private string? selectedValue;
    }
}