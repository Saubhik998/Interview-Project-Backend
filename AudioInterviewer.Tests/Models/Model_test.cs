using Xunit;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AudioInterviewer.API.Models;

namespace AudioInterviewer.UnitTests.Models
{
    public class ModelValidationTests
    {
        // ------------------- Answer -------------------

        [Fact]
        public void Answer_Default_Properties()
        {
            var ans = new Answer();
            Assert.Equal("", ans.Question);
            Assert.Equal("", ans.AudioUrl);
            Assert.Equal("", ans.Transcript);
        }

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

        [Fact]
        public void Answer_Validation_Invalid_AudioUrl()
        {
            var ans = new Answer { Question = "Q", AudioUrl = "badurl", Transcript = "T" };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(ans, new ValidationContext(ans), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("AudioUrl must be a valid URL"));
        }

        // ------------------- AnswerDto -------------------

        [Fact]
        public void AnswerDto_Default_Properties()
        {
            var dto = new AnswerDto();
            Assert.Equal(string.Empty, dto.SessionId);
            Assert.Equal("", dto.Question);
            Assert.Equal("", dto.AudioBase64);
            Assert.Equal("", dto.Transcript);
        }

        [Fact]
        public void AnswerDto_Validation_Required()
        {
            var dto = new AnswerDto();
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("SessionId is required"));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Question is required"));
        }

        [Fact]
        public void AnswerDto_Validation_Short_Question()
        {
            var dto = new AnswerDto { SessionId = "s1", Question = "short", AudioBase64 = new string('a', 5000), Transcript = "" };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Question text is too short"));
        }

        [Fact]
        public void AnswerDto_Validation_Short_Audio()
        {
            var dto = new AnswerDto { SessionId = "s1", Question = "A valid long question?", AudioBase64 = "abc", Transcript = "" };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Audio data is too short or malformed"));
        }

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

        [Fact]
        public void InterviewReport_Default_Properties_CollectionsInitialized()
        {
            var report = new InterviewReport();
            Assert.NotNull(report.Strengths);
            Assert.NotNull(report.ImprovementAreas);
            Assert.NotNull(report.SuggestedFollowUp);
            Assert.NotNull(report.Answers);
        }

        [Fact]
        public void InterviewReport_Validation_Required_And_EmailFormat()
        {
            var report = new InterviewReport();
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(report, new ValidationContext(report), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Email is required"));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Job description is required"));
        }

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

        [Fact]
        public void InterviewSession_Default_Properties_CollectionsInitialized()
        {
            var s = new InterviewSession();
            Assert.NotNull(s.Questions);
            Assert.NotNull(s.Answers);
            Assert.Equal(0, s.CurrentIndex);
        }

        [Fact]
        public void InterviewSession_Validation_Required_And_EmailFormat()
        {
            var s = new InterviewSession();
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(s, new ValidationContext(s), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Email is required"));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Job description is required"));
        }

        [Fact]
        public void InterviewSession_Validation_EmailFormat()
        {
            var s = new InterviewSession { Email = "bad_email", JobDescription = "JD" };
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(s, new ValidationContext(s), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Invalid email format"));
        }

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

        [Fact]
        public void Question_Default_Text()
        {
            var q = new Question();
            Assert.Equal("", q.Text);
        }

        [Fact]
        public void Question_Validation_Required()
        {
            var q = new Question();
            var results = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(q, new ValidationContext(q), results, true));
            Assert.Contains(results, r => r.ErrorMessage.Contains("Question text is required"));
        }

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
