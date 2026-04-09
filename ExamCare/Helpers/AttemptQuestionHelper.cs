using System;
using ExamCare.Models;

namespace ExamCare.Helpers
{
    public static class AttemptQuestionHelper
    {
        public static void StartSession(AttemptQuestion attemptQuestion)
        {
            DateTime now = DateTime.Now;

            if (attemptQuestion.FirstActionTime == null)
                attemptQuestion.FirstActionTime = now;
               attemptQuestion.StartTime = now;



            attemptQuestion.LastActionTime = now;
            attemptQuestion.CurrentVisitStartTime = now;
        }

        public static void EndSession(AttemptQuestion attemptQuestion)
        {
            DateTime now = DateTime.Now;
            attemptQuestion.EndTime = now;

            if (attemptQuestion.CurrentVisitStartTime.HasValue)
            {
                attemptQuestion.TimeSpent +=
                    (int)(now - attemptQuestion.CurrentVisitStartTime.Value).TotalSeconds;

                attemptQuestion.CurrentVisitStartTime = null;
            }

            attemptQuestion.LastActionTime = now;
            attemptQuestion.EndTime = now;
        }
    }
}