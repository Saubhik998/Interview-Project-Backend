using AudioInterviewer.API.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace AudioInterviewer.Tests.Models
{
    public static class ValidationHelper
    {
        public static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }

    public class ModelValidationTests
    {
        [Fact]
        public void AnswerDto_ShouldBeValid_WhenAllFieldsAreProvided()
        {
            var model = new AnswerDto
            {
                Question = "What is your name?",
                AudioBase64 = "somebase64string",
                Transcript = "John Doe"
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Empty(results);
        }

        [Fact]
        public void AnswerDto_ShouldFailValidation_WhenRequiredFieldsAreMissing()
        {
            var model = new AnswerDto(); // All fields are empty

            var results = ValidationHelper.ValidateModel(model);
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public void InterviewReport_ShouldBeValid_WhenAllFieldsArePresent()
        {
            var model = new InterviewReport
            {
                Id = "123456789",
                Email = "candidate@example.com",
                JobDescription = "Data Scientist",
                CandidateFitScore = 90,
                Strengths = new List<string> { "Analysis", "Communication" },
                ImprovementAreas = new List<string> { "Time management" },
                SuggestedFollowUp = new List<string> { "Discuss data ethics" },
                Answers = new List<Answer>
                {
                    new Answer { Question = "What is data cleaning?", AudioUrl = "http://example.com/audio.mp3", Transcript = "Answer..." }
                }
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Empty(results);
        }

        [Fact]
        public void InterviewReport_ShouldFailValidation_WhenEmailMissing()
        {
            var model = new InterviewReport
            {
                JobDescription = "ML Engineer",
                CandidateFitScore = 85
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InterviewReport.Email)));
        }

        [Fact]
        public void InterviewReport_ShouldFailValidation_WhenEmailIsInvalid()
        {
            var model = new InterviewReport
            {
                Email = "invalid-email",
                JobDescription = "AI Engineer",
                CandidateFitScore = 90
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InterviewReport.Email)));
        }

        [Fact]
        public void InterviewSession_ShouldFailValidation_WhenEmailIsMissing()
        {
            var model = new InterviewSession
            {
                JobDescription = "Backend Engineer",
                Questions = new List<Question> { new Question { Text = "What is REST?" } },
                Answers = new List<Answer> { new Answer { Question = "Q1", AudioUrl = "http://example.com/audio.mp3", Transcript = "transcript" } },
                CurrentIndex = 1
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InterviewSession.Email)));
        }

        [Fact]
        public void InterviewSession_ShouldFailValidation_WhenCurrentIndexIsNegative()
        {
            var model = new InterviewSession
            {
                Email = "dev@example.com",
                JobDescription = "Backend Engineer",
                CurrentIndex = -1
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InterviewSession.CurrentIndex)));
        }

        [Fact]
        public void Answer_ShouldFailValidation_WhenAudioUrlIsInvalid()
        {
            var model = new Answer
            {
                Question = "Explain ML.",
                AudioUrl = "not-a-valid-url",
                Transcript = "ML is about..."
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Contains(results, r => r.ErrorMessage.Contains("valid URL"));
        }

        [Fact]
        public void Question_ShouldFailValidation_WhenTextIsMissing()
        {
            var model = new Question(); // Text is empty

            var results = ValidationHelper.ValidateModel(model);
            Assert.Single(results);
            Assert.Equal("The Text field is required.", results[0].ErrorMessage);
        }
    }
}
