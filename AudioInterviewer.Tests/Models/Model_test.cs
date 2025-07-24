using Xunit;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AudioInterviewer.API.Models;

namespace AudioInterviewer.UnitTests.Models
{
    /// <summary>
    /// Unit tests for data annotation–level validation of model classes used in the AudioInterviewer API.
    /// Verifies default property values and validation rules for <see cref="Answer"/>, <see cref="AnswerDto"/>,
    /// <see cref="InterviewReport"/>, <see cref="InterviewSession"/>, and <see cref="Question"/>.
    /// </summary>
    public class ModelValidationTests
    {
        // ------------------- Answer -------------------

        /// <summary>
        /// Verifies that the default constructor for <see cref="Answer"/>
        /// sets all string properties to empty strings.
        /// </summary>
        [Fact]
        public void Answer_Default_Properties()
        {
            var ans = new Answer();
            Assert.Equal("", ans.Question);
            Assert.Equal("", ans.AudioUrl);
            Assert.Equal("", ans.Transcript);
        }

        /// <summary>
        /// Confirms that required fields in <see cref="Answer"/> are detected as missing during validation.
        /// </summary>
        [Fact]
        public void Answer_Validation_Missing_Fields()
        {
            var ans = new Answer();
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(ans, new ValidationContext(ans), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Question is required"));
            Assert.Contains(results, r => r.ErrorMessage.Contains("AudioUrl is required"));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Transcript is required"));
        }

        /// <summary>
        /// Confirms that <see cref="Answer.AudioUrl"/> fails validation if it is not a valid URL.
        /// </summary>
        [Fact]
        public void Answer_Validation_Invalid_AudioUrl()
        {
            var ans = new Answer { Question = "Q", AudioUrl = "badurl", Transcript = "T" };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(ans, new ValidationContext(ans), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("AudioUrl must be a valid URL"));
        }

        // ------------------- AnswerDto -------------------

        /// <summary>
        /// Verifies that the default constructor for <see cref="AnswerDto"/>
        /// sets all string properties to empty strings.
        /// </summary>
        [Fact]
        public void AnswerDto_Default_Properties()
        {
            var dto = new AnswerDto();
            Assert.Equal(string.Empty, dto.SessionId);
            Assert.Equal("", dto.Question);
            Assert.Equal("", dto.AudioBase64);
            Assert.Equal("", dto.Transcript);
        }

        /// <summary>
        /// Confirms that data annotation validation on <see cref="AnswerDto"/> catches required field errors.
        /// </summary>
        [Fact]
        public void AnswerDto_Validation_Required()
        {
            var dto = new AnswerDto();
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("SessionId is required"));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Question is required"));
        }

        /// <summary>
        /// Checks that a question field with insufficient length fails validation in <see cref="AnswerDto"/>.
        /// </summary>
        [Fact]
        public void AnswerDto_Validation_Short_Question()
        {
            var dto = new AnswerDto { SessionId = "s1", Question = "short", AudioBase64 = new string('a', 5000), Transcript = "" };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Question text is too short"));
        }

        /// <summary>
        /// Checks that too-short or malformed AudioBase64 in <see cref="AnswerDto"/> fails validation.
        /// </summary>
        [Fact]
        public void AnswerDto_Validation_Short_Audio()
        {
            var dto = new AnswerDto { SessionId = "s1", Question = "A valid long question?", AudioBase64 = "abc", Transcript = "" };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Audio data is too short or malformed"));
        }

        /// <summary>
        /// Checks that an excessively long transcript in <see cref="AnswerDto"/> fails validation.
        /// </summary>
        [Fact]
        public void AnswerDto_Validation_Transcript_TooLong()
        {
            var dto = new AnswerDto
            {
                SessionId = "s1",
                Question = "A valid long enough question.",
                AudioBase64 = new string('a', 6000),
                Transcript = new string('x', 5001)
            };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Transcript is too long"));
        }

        // ------------------- InterviewReport -------------------

        /// <summary>
        /// Verifies that the default constructor for <see cref="InterviewReport"/>
        /// initializes all collection-type properties.
        /// </summary>
        [Fact]
        public void InterviewReport_Default_Properties_CollectionsInitialized()
        {
            var report = new InterviewReport();
            Assert.NotNull(report.Strengths);
            Assert.NotNull(report.ImprovementAreas);
            Assert.NotNull(report.SuggestedFollowUp);
            Assert.NotNull(report.Answers);
        }

        /// <summary>
        /// Checks that required properties (including email format) in <see cref="InterviewReport"/> fail validation if missing/invalid.
        /// </summary>
        [Fact]
        public void InterviewReport_Validation_Required_And_EmailFormat()
        {
            var report = new InterviewReport();
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(report, new ValidationContext(report), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Email is required"));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Job description is required"));
        }

        /// <summary>
        /// Confirms that an invalid email in <see cref="InterviewReport.Email"/> fails validation.
        /// </summary>
        [Fact]
        public void InterviewReport_Validation_EmailFormat()
        {
            var report = new InterviewReport
            {
                Email = "not-an-email",
                JobDescription = "JD"
            };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(report, new ValidationContext(report), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Email must be a valid email address"));
        }

        /// <summary>
        /// Checks that an out-of-range candidate fit score in <see cref="InterviewReport"/> fails validation.
        /// </summary>
        [Fact]
        public void InterviewReport_Validation_CandidateFitScore_Range()
        {
            var report = new InterviewReport
            {
                Email = "user@email.com",
                JobDescription = "desc",
                CandidateFitScore = 101
            };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(report, new ValidationContext(report), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("CandidateFitScore must be between 0 and 100"));
        }

        // ------------------- InterviewSession -------------------

        /// <summary>
        /// Verifies that the default constructor for <see cref="InterviewSession"/>
        /// initializes collections and sets default index to 0.
        /// </summary>
        [Fact]
        public void InterviewSession_Default_Properties_CollectionsInitialized()
        {
            var s = new InterviewSession();
            Assert.NotNull(s.Questions);
            Assert.NotNull(s.Answers);
            Assert.Equal(0, s.CurrentIndex);
        }

        /// <summary>
        /// Asserts that required and email-format properties in <see cref="InterviewSession"/> fail validation if missing or invalid.
        /// </summary>
        [Fact]
        public void InterviewSession_Validation_Required_And_EmailFormat()
        {
            var s = new InterviewSession();
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(s, new ValidationContext(s), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Email is required"));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Job description is required"));
        }

        /// <summary>
        /// Confirms that a bad email address causes <see cref="InterviewSession"/> validation to fail.
        /// </summary>
        [Fact]
        public void InterviewSession_Validation_EmailFormat()
        {
            var s = new InterviewSession { Email = "bad_email", JobDescription = "JD" };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(s, new ValidationContext(s), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Invalid email format"));
        }

        /// <summary>
        /// Ensures that negative <see cref="InterviewSession.CurrentIndex"/> fails validation.
        /// </summary>
        [Fact]
        public void InterviewSession_Validation_CurrentIndexRange()
        {
            var s = new InterviewSession
            {
                Email = "email@ok.com",
                JobDescription = "desc",
                CurrentIndex = -5
            };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(s, new ValidationContext(s), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Current index cannot be negative"));
        }

        // ------------------- Question -------------------

        /// <summary>
        /// Confirms the default <see cref="Question.Text"/> value is an empty string.
        /// </summary>
        [Fact]
        public void Question_Default_Text()
        {
            var q = new Question();
            Assert.Equal("", q.Text);
        }

        /// <summary>
        /// Checks that <see cref="Question"/> validation fails if text is missing.
        /// </summary>
        [Fact]
        public void Question_Validation_Required()
        {
            var q = new Question();
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(q, new ValidationContext(q), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Question text is required"));
        }

        /// <summary>
        /// Checks that an overly long <see cref="Question.Text"/> fails validation.
        /// </summary>
        [Fact]
        public void Question_Validation_StringLength()
        {
            var q = new Question { Text = new string('a', 301) };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(q, new ValidationContext(q), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Question text is too long"));
        }
    }
}
