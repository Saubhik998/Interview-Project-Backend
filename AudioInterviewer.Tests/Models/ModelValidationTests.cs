using AudioInterviewer.API.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Xunit;

namespace AudioInterviewer.Tests.Models
{
    /// <summary>
    /// Helper class for validating model annotations.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates the specified model against its data annotations.
        /// </summary>
        /// <param name="model">The object to validate.</param>
        /// <returns>A list of validation results.</returns>
        public static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }

    /// <summary>
    /// Unit tests for model-level data annotation validation.
    /// </summary>
    public class ModelValidationTests
    {
        /// <summary>
        /// Ensures AnswerDto is valid when all required fields are correctly populated.
        /// </summary>
        [Fact]
        public void AnswerDto_ShouldBeValid_WhenRequiredFieldsAreProvided()
        {
            var model = new AnswerDto
            {
                Question = "What is your name?",
                AudioBase64 = Convert.ToBase64String(new byte[4096]),
                Transcript = "John Doe"
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Empty(results);
        }

        /// <summary>
        /// Ensures validation fails when the Question field is missing in AnswerDto.
        /// </summary>
        [Fact]
        public void AnswerDto_ShouldFailValidation_WhenQuestionIsMissing()
        {
            var model = new AnswerDto
            {
                AudioBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("valid audio")),
                Transcript = "Transcript"
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AnswerDto.Question)));
        }

        /// <summary>
        /// Ensures validation fails when AudioBase64 is too short or malformed in AnswerDto.
        /// </summary>
        [Fact]
        public void AnswerDto_ShouldFailValidation_WhenAudioBase64IsInvalid()
        {
            var model = new AnswerDto
            {
                Question = "Tell me about yourself",
                AudioBase64 = "short",
                Transcript = "Some transcript"
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Contains(results, r => r.ErrorMessage == "Audio data is too short or malformed.");
        }

        /// <summary>
        /// Ensures InterviewReport is valid when all fields are properly set.
        /// </summary>
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
                    new Answer
                    {
                        Question = "What is data cleaning?",
                        AudioUrl = "http://example.com/audio.mp3",
                        Transcript = "Answer..."
                    }
                }
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Empty(results);
        }

        /// <summary>
        /// Ensures validation fails when Email is missing in InterviewReport.
        /// </summary>
        [Fact]
        public void InterviewReport_ShouldFailValidation_WhenEmailIsMissing()
        {
            var model = new InterviewReport
            {
                JobDescription = "ML Engineer",
                CandidateFitScore = 85
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InterviewReport.Email)));
        }

        /// <summary>
        /// Ensures validation fails when Email format is invalid in InterviewReport.
        /// </summary>
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

        /// <summary>
        /// Ensures validation fails when Email is missing in InterviewSession.
        /// </summary>
        [Fact]
        public void InterviewSession_ShouldFailValidation_WhenEmailIsMissing()
        {
            var model = new InterviewSession
            {
                JobDescription = "Backend Engineer",
                Questions = new List<Question> { new Question { Text = "What is REST?" } },
                Answers = new List<Answer>
                {
                    new Answer
                    {
                        Question = "Q1",
                        AudioUrl = "http://example.com/audio.mp3",
                        Transcript = "transcript"
                    }
                },
                CurrentIndex = 1
            };

            var results = ValidationHelper.ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InterviewSession.Email)));
        }

        /// <summary>
        /// Ensures validation fails when CurrentIndex is negative in InterviewSession.
        /// </summary>
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

        /// <summary>
        /// Ensures validation fails when AudioUrl is not a valid URL in Answer.
        /// </summary>
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
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("valid URL"));
        }

        /// <summary>
        /// Ensures validation fails when Question.Text is missing.
        /// </summary>
        [Fact]
        public void Question_ShouldFailValidation_WhenTextIsMissing()
        {
            var model = new Question(); // Missing Text

            var results = ValidationHelper.ValidateModel(model);
            Assert.Single(results);
            Assert.Equal("Question text is required.", results[0].ErrorMessage);
        }
    }
}
