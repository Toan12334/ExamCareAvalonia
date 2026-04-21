using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamCare.Helpers;
using ExamCare.Models;
using ExamCare.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ExamCare.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private bool _isSubmitting;
        private readonly StudentExamApiService _studentExamApiService;
        private readonly Action? _onSubmitSuccess;

        private Dictionary<int, AttemptQuestion> _attemptQuestionMap = new();

        public ObservableCollection<int> QuestionNumbers { get; } = new();
        public ObservableCollection<Question> Questions { get; } = new();

        [ObservableProperty]
        private Question? selectedQuestion;

        [ObservableProperty]
        private string examName = string.Empty;

        [ObservableProperty]
        private int duration;

        [ObservableProperty]
        private int currentQuestionNumber;

        [ObservableProperty]
        private string studentAnswerText = string.Empty;

        [ObservableProperty]
        private string countdownText = "00:00:00";

        private DispatcherTimer? _timer;
        private TimeSpan _timeLeft;

        public DashboardViewModel(Action? onSubmitSuccess = null)
        {
            _studentExamApiService = new StudentExamApiService();
            _onSubmitSuccess = onSubmitSuccess;

            _ = LoadExamAsync();
        }

        private void StartCountdown()
        {
            _timeLeft = TimeSpan.FromMinutes(Duration);

            CountdownText = _timeLeft.ToString(@"hh\:mm\:ss");

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (s, e) =>
            {
                if (_timeLeft.TotalSeconds > 0)
                {
                    _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
                    CountdownText = _timeLeft.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    _timer?.Stop();
                    CountdownText = "Hết giờ!";
                }
            };

            _timer.Start();
        }

        [RelayCommand]
        private void Click(int questionNumber)
        {
            if (questionNumber <= 0 || questionNumber > Questions.Count)
                return;

            DateTime now = DateTime.Now;

            if (SelectedQuestion != null &&
                _attemptQuestionMap.TryGetValue(SelectedQuestion.QuestionId, out var oldAttemptQuestion))
            {
                if (SelectedQuestion.IsType3)
                {
                    SyncEssayAnswer(SelectedQuestion, oldAttemptQuestion, now);
                }

                AttemptQuestionHelper.EndSession(oldAttemptQuestion);

                oldAttemptQuestion.AnswerLogs.Add(new AnswerLog
                {
                    AttemptQuestionId = oldAttemptQuestion.AttemptQuestionId,
                    ActionType = "leave_question",
                    SelectedOption = SelectedQuestion.IsType3
                        ? oldAttemptQuestion.CurrentAnswer?.ShortAnswer
                        : oldAttemptQuestion.CurrentAnswer?.SelectedOption,
                    CreatedAt = now
                });
            }

            CurrentQuestionNumber = questionNumber;
            SelectedQuestion = Questions[questionNumber - 1];

            if (SelectedQuestion != null &&
                SelectedQuestion.IsType3 &&
                _attemptQuestionMap.TryGetValue(SelectedQuestion.QuestionId, out var essayAttemptQuestion))
            {
                SelectedQuestion.StudentAnswerText = essayAttemptQuestion.CurrentAnswer?.ShortAnswer ?? string.Empty;
            }

            if (SelectedQuestion != null &&
                SelectedQuestion.IsType2 &&
                _attemptQuestionMap.TryGetValue(SelectedQuestion.QuestionId, out var selectedAttemptQuestion))
            {
                TrueFalseHelper.RestoreSelection(SelectedQuestion, selectedAttemptQuestion);
            }

            if (SelectedQuestion != null &&
                _attemptQuestionMap.TryGetValue(SelectedQuestion.QuestionId, out var newAttemptQuestion))
            {
                AttemptQuestionHelper.StartSession(newAttemptQuestion);

                newAttemptQuestion.AnswerLogs.Add(new AnswerLog
                {
                    AttemptQuestionId = newAttemptQuestion.AttemptQuestionId,
                    ActionType = "enter_question",
                    SelectedOption = newAttemptQuestion.CurrentAnswer?.SelectedOption,
                    CreatedAt = now
                });
            }
        }

        [RelayCommand]
        private void NextQuestion()
        {
            if (CurrentQuestionNumber < Questions.Count)
            {
                Click(CurrentQuestionNumber + 1);
            }
        }

        [RelayCommand]
        private void PreviousQuestion()
        {
            if (CurrentQuestionNumber > 1)
            {
                Click(CurrentQuestionNumber - 1);
            }
        }

        [RelayCommand]
        private void SelectOption(QuestionOption option)
        {
            if (SelectedQuestion == null || option == null) return;
            if (SelectedQuestion.Options == null) return;

            int questionId = SelectedQuestion.QuestionId;

            if (!_attemptQuestionMap.TryGetValue(questionId, out var attemptQuestion))
                return;

            DateTime now = DateTime.Now;
            string newOption = option.OptionLabel;
            string? oldOption = attemptQuestion.CurrentAnswer?.SelectedOption;

            if (attemptQuestion.CurrentVisitStartTime == null)
                AttemptQuestionHelper.StartSession(attemptQuestion);

            attemptQuestion.LastActionTime = now;

            if (oldOption == newOption)
            {
                attemptQuestion.AnswerLogs.Add(new AnswerLog
                {
                    AttemptQuestionId = attemptQuestion.AttemptQuestionId,
                    ActionType = "select_same_option",
                    SelectedOption = newOption,
                    CreatedAt = now
                });
                return;
            }

            if (!string.IsNullOrEmpty(oldOption) && oldOption != newOption)
            {
                attemptQuestion.AnswerChangedCount++;
            }

            foreach (var item in SelectedQuestion.Options)
                item.IsSelected = false;

            option.IsSelected = true;

            attemptQuestion.AnswerLogs.Add(new AnswerLog
            {
                AttemptQuestionId = attemptQuestion.AttemptQuestionId,
                ActionType = "select_option",
                SelectedOption = newOption,
                CreatedAt = now
            });

            if (attemptQuestion.CurrentAnswer == null)
            {
                attemptQuestion.CurrentAnswer = new StudentAnswer
                {
                    AttemptQuestionId = attemptQuestion.AttemptQuestionId,
                    SelectedOption = newOption,
                    CreatedAt = now
                };
            }
            else
            {
                attemptQuestion.CurrentAnswer.SelectedOption = newOption;
                attemptQuestion.CurrentAnswer.CreatedAt = now;
            }

            Debug.WriteLine($"QuestionId: {questionId}");
            Debug.WriteLine($"CurrentAnswer: {attemptQuestion.CurrentAnswer?.SelectedOption}");
            Debug.WriteLine($"AnswerChangedCount: {attemptQuestion.AnswerChangedCount}");
            Debug.WriteLine($"FirstActionTime: {attemptQuestion.FirstActionTime}");
            Debug.WriteLine($"LastActionTime: {attemptQuestion.LastActionTime}");
            Debug.WriteLine($"CurrentVisitStartTime: {attemptQuestion.CurrentVisitStartTime}");
            Debug.WriteLine($"TimeSpent: {attemptQuestion.TimeSpent}");
            Debug.WriteLine($"LogCount: {attemptQuestion.AnswerLogs.Count}");
        }

        [RelayCommand]
        private void SelectTrueStatement(TrueFalseStatement statement)
        {
            SelectTrueFalseInternal(statement, "T");
        }

        [RelayCommand]
        private void SelectFalseStatement(TrueFalseStatement statement)
        {
            SelectTrueFalseInternal(statement, "F");
        }

        private void SelectTrueFalseInternal(TrueFalseStatement statement, string newValue)
        {
            if (SelectedQuestion == null || statement == null)
                return;

            int questionId = SelectedQuestion.QuestionId;

            if (!_attemptQuestionMap.TryGetValue(questionId, out var attemptQuestion))
                return;

            DateTime now = DateTime.Now;

            if (attemptQuestion.CurrentVisitStartTime == null)
                AttemptQuestionHelper.StartSession(attemptQuestion);

            attemptQuestion.LastActionTime = now;

            string? oldValue = statement.SelectedValue;

            if (newValue == "T")
            {
                statement.IsTrueSelected = true;
                statement.IsFalseSelected = false;
            }
            else
            {
                statement.IsTrueSelected = false;
                statement.IsFalseSelected = true;
            }

            if (oldValue == newValue)
            {
                attemptQuestion.AnswerLogs.Add(new AnswerLog
                {
                    AttemptQuestionId = attemptQuestion.AttemptQuestionId,
                    ActionType = "select_same_true_false",
                    SelectedOption = $"{statement.StatementId}:{newValue}",
                    CreatedAt = now
                });

                SyncAttemptAnswerForTrueFalse(SelectedQuestion, attemptQuestion, now);

                Debug.WriteLine($"QuestionId: {questionId}");
                Debug.WriteLine($"StatementId: {statement.StatementId}");
                Debug.WriteLine($"SelectedValue: {statement.SelectedValue}");
                Debug.WriteLine($"CurrentAnswer: {attemptQuestion.CurrentAnswer?.SelectedOption}");
                Debug.WriteLine($"AnswerChangedCount: {attemptQuestion.AnswerChangedCount}");
                Debug.WriteLine($"FirstActionTime: {attemptQuestion.FirstActionTime}");
                Debug.WriteLine($"LastActionTime: {attemptQuestion.LastActionTime}");
                Debug.WriteLine($"CurrentVisitStartTime: {attemptQuestion.CurrentVisitStartTime}");
                Debug.WriteLine($"TimeSpent: {attemptQuestion.TimeSpent}");
                Debug.WriteLine($"LogCount: {attemptQuestion.AnswerLogs.Count}");

                return;
            }

            statement.SelectedValue = newValue;

            if (!string.IsNullOrEmpty(oldValue) && oldValue != newValue)
            {
                attemptQuestion.AnswerChangedCount++;
            }

            attemptQuestion.AnswerLogs.Add(new AnswerLog
            {
                AttemptQuestionId = attemptQuestion.AttemptQuestionId,
                ActionType = "select_true_false",
                SelectedOption = $"{statement.StatementId}:{newValue}",
                CreatedAt = now
            });

            SyncAttemptAnswerForTrueFalse(SelectedQuestion, attemptQuestion, now);

            Debug.WriteLine($"QuestionId: {questionId}");
            Debug.WriteLine($"StatementId: {statement.StatementId}");
            Debug.WriteLine($"SelectedValue: {statement.SelectedValue}");
            Debug.WriteLine($"CurrentAnswer: {attemptQuestion.CurrentAnswer?.SelectedOption}");
            Debug.WriteLine($"AnswerChangedCount: {attemptQuestion.AnswerChangedCount}");
            Debug.WriteLine($"FirstActionTime: {attemptQuestion.FirstActionTime}");
            Debug.WriteLine($"LastActionTime: {attemptQuestion.LastActionTime}");
            Debug.WriteLine($"CurrentVisitStartTime: {attemptQuestion.CurrentVisitStartTime}");
            Debug.WriteLine($"TimeSpent: {attemptQuestion.TimeSpent}");
            Debug.WriteLine($"LogCount: {attemptQuestion.AnswerLogs.Count}");
        }

        [RelayCommand]
        private async Task Submit()
        {
            DateTime now = DateTime.Now;

            if (SelectedQuestion != null &&
                _attemptQuestionMap.TryGetValue(SelectedQuestion.QuestionId, out var currentAttemptQuestion))
            {
                if (SelectedQuestion.IsType3)
                {
                    SyncEssayAnswer(SelectedQuestion, currentAttemptQuestion, now);
                }

                AttemptQuestionHelper.EndSession(currentAttemptQuestion);
                currentAttemptQuestion.LastActionTime = now;

                currentAttemptQuestion.AnswerLogs.Add(new AnswerLog
                {
                    AttemptQuestionId = currentAttemptQuestion.AttemptQuestionId,
                    ActionType = "submit_exam",
                    SelectedOption = SelectedQuestion.IsType3
                        ? currentAttemptQuestion.CurrentAnswer?.ShortAnswer
                        : currentAttemptQuestion.CurrentAnswer?.SelectedOption,
                    CreatedAt = now
                });
            }

            foreach (var kvp in _attemptQuestionMap)
            {
                var questionId = kvp.Key;
                var attemptQuestion = kvp.Value;

                Debug.WriteLine("----");
                Debug.WriteLine($"StudentExamID: {attemptQuestion.StudentExamId}");
                Debug.WriteLine($"Submitting QuestionId: {questionId}");
                Debug.WriteLine($"SelectedOption: {attemptQuestion.CurrentAnswer?.SelectedOption}");
                Debug.WriteLine($"ShortAnswer: {attemptQuestion.CurrentAnswer?.ShortAnswer}");
                Debug.WriteLine($"AnswerChangedCount: {attemptQuestion.AnswerChangedCount}");
                Debug.WriteLine($"FirstActionTime: {attemptQuestion.FirstActionTime}");
                Debug.WriteLine($"LastActionTime: {attemptQuestion.LastActionTime}");
                Debug.WriteLine($"CurrentVisitStartTime: {attemptQuestion.CurrentVisitStartTime}");
                Debug.WriteLine($"TimeSpent: {attemptQuestion.TimeSpent}");
                Debug.WriteLine($"LogCount: {attemptQuestion.AnswerLogs.Count}");
            }

            if (_isSubmitting) return;
            _isSubmitting = true;

            try
            {
                await _studentExamApiService.SendAttemptMapAsync(_attemptQuestionMap);
                _onSubmitSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Submit lỗi: " + ex.Message);
            }
            finally
            {
                _isSubmitting = false;
            }
        }

        private async Task LoadExamAsync()
        {
            var response = await _studentExamApiService.StartExamAsync(1, 3);
            var examData = response?.Data;

            if (examData == null || examData.Questions == null || examData.Questions.Count == 0)
                return;

            ExamName = examData.ExamName;
            Duration = examData.Duration;

            QuestionNumbers.Clear();
            Questions.Clear();
            _attemptQuestionMap.Clear();

            foreach (var question in examData.Questions)
            {
                question.QuestionContentBuilder.Clear();
                question.QuestionContentBuilder.Append(question.QuestionContent ?? string.Empty);

                if (question.Options != null)
                {
                    foreach (var option in question.Options)
                    {
                        option.OptionContentBuilder.Clear();
                        option.OptionContentBuilder.Append(option.OptionContent ?? string.Empty);
                    }
                }

                if (question.TrueFalseStatements != null)
                {
                    foreach (var statement in question.TrueFalseStatements)
                    {
                        statement.StatementContentBuilder.Clear();
                        statement.StatementContentBuilder.Append(statement.StatementContent ?? string.Empty);
                    }
                }
            }

            for (int i = 0; i < examData.Questions.Count; i++)
            {
                var question = examData.Questions[i];

                QuestionNumbers.Add(i + 1);
                Questions.Add(question);

                _attemptQuestionMap[question.QuestionId] = new AttemptQuestion
                {
                    AttemptQuestionId = 0,
                    StudentExamId = examData.StudentExamId,
                    QuestionId = question.QuestionId,
                    FirstActionTime = null,
                    LastActionTime = null,
                    CurrentVisitStartTime = null,
                    TimeSpent = 0,
                    AnswerChangedCount = 0,
                    IsCorrect = false,
                    AnswerLogs = new List<AnswerLog>(),
                    CurrentAnswer = null
                };
            }

            CurrentQuestionNumber = 1;
            SelectedQuestion = Questions.FirstOrDefault();

            if (SelectedQuestion != null &&
                SelectedQuestion.IsType2 &&
                _attemptQuestionMap.TryGetValue(SelectedQuestion.QuestionId, out var firstType2Attempt))
            {
                TrueFalseHelper.RestoreSelection(SelectedQuestion, firstType2Attempt);
            }

            if (SelectedQuestion != null &&
                _attemptQuestionMap.TryGetValue(SelectedQuestion.QuestionId, out var firstAttemptQuestion))
            {
                AttemptQuestionHelper.StartSession(firstAttemptQuestion);

                firstAttemptQuestion.AnswerLogs.Add(new AnswerLog
                {
                    AttemptQuestionId = firstAttemptQuestion.AttemptQuestionId,
                    ActionType = "enter_question",
                    SelectedOption = null,
                    CreatedAt = DateTime.Now
                });

                firstAttemptQuestion.CurrentVisitStartTime = DateTime.Now;
            }

            StartCountdown();
        }

        private void SyncAttemptAnswerForTrueFalse(Question question, AttemptQuestion attemptQuestion, DateTime now)
        {
            string answerString = TrueFalseHelper.BuildAnswerString(question);

            if (attemptQuestion.CurrentAnswer == null)
            {
                attemptQuestion.CurrentAnswer = new StudentAnswer
                {
                    AttemptQuestionId = attemptQuestion.AttemptQuestionId,
                    SelectedOption = answerString,
                    CreatedAt = now
                };
            }
            else
            {
                attemptQuestion.CurrentAnswer.SelectedOption = answerString;
                attemptQuestion.CurrentAnswer.CreatedAt = now;
            }
        }

        private void SyncEssayAnswer(Question question, AttemptQuestion attemptQuestion, DateTime now)
        {
            string text = question.StudentAnswerText?.Trim() ?? string.Empty;

            if (attemptQuestion.CurrentAnswer == null)
            {
                attemptQuestion.CurrentAnswer = new StudentAnswer
                {
                    AttemptQuestionId = attemptQuestion.AttemptQuestionId,
                    ShortAnswer = text,
                    CreatedAt = now
                };
            }
            else
            {
                attemptQuestion.CurrentAnswer.ShortAnswer = text;
                attemptQuestion.CurrentAnswer.CreatedAt = now;
            }
        }
    }
}