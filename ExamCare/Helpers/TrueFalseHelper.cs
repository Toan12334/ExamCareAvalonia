using System;
using System.Collections.Generic;
using System.Linq;
using ExamCare.Models;

namespace ExamCare.Helpers
{
    public static class TrueFalseHelper
    {
        public static Dictionary<int, string> ParseAnswerString(string? answerString)
        {
            var result = new Dictionary<int, string>();

            if (string.IsNullOrWhiteSpace(answerString))
                return result;

            var pairs = answerString.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                var parts = pair.Split(':', 2);
                if (parts.Length != 2) continue;

                if (int.TryParse(parts[0], out int statementId))
                {
                    result[statementId] = parts[1];
                }
            }

            return result;
        }

        public static string BuildAnswerString(Question question)
        {
            if (question.TrueFalseStatements == null || question.TrueFalseStatements.Count == 0)
                return string.Empty;

            return string.Join(";", question.TrueFalseStatements.Select(s =>
                $"{s.StatementId}:{s.SelectedValue ?? string.Empty}"));
        }

        public static void RestoreSelection(Question question, AttemptQuestion attemptQuestion)
        {
            if (question.TrueFalseStatements == null || question.TrueFalseStatements.Count == 0)
                return;

            var map = ParseAnswerString(attemptQuestion.CurrentAnswer?.SelectedOption);

            foreach (var statement in question.TrueFalseStatements)
            {
                if (map.TryGetValue(statement.StatementId, out var value))
                {
                    statement.SelectedValue = value;
                    statement.IsTrueSelected = value == "T";
                    statement.IsFalseSelected = value == "F";
                }
                else
                {
                    statement.SelectedValue = null;
                    statement.IsTrueSelected = false;
                    statement.IsFalseSelected = false;
                }
            }
        }

        public static string GetStatementActionValue(TrueFalseStatement statement)
        {
            if (statement.IsTrueSelected) return "T";
            if (statement.IsFalseSelected) return "F";
            return string.Empty;
        }
    }
}