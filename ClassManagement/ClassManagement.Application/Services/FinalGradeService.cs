using Microsoft.EntityFrameworkCore;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Entities;
using ClassManagement.Infrastructure.Data;

namespace ClassManagement.Application.Services
{
    public class FinalGradeService : IFinalGradeService
    {
        private readonly ClassManagementDbContext _context;

        public FinalGradeService(ClassManagementDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SetFinalGradeAsync(SetFinalGradeDto dto)
        {
            var existingGrade = await _context.FinalGrades
                .FirstOrDefaultAsync(fg => fg.StudentId == dto.StudentId && fg.CourseId == dto.CourseId);

            if (existingGrade != null)
            {
                existingGrade.Grade = dto.Grade;
            }
            else
            {
                var finalGrade = new FinalGrade
                {
                    StudentId = dto.StudentId,
                    CourseId = dto.CourseId,
                    Grade = dto.Grade,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FinalGrades.Add(finalGrade);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FinalGrade?> GetStudentFinalGradeAsync(int studentId, int courseId)
        {
            return await _context.FinalGrades
                .FirstOrDefaultAsync(fg => fg.StudentId == studentId && fg.CourseId == courseId);
        }

        public async Task<List<FinalGradeDto>> GetCourseFinalGradesAsync(int courseId)
        {
            var grades = await _context.FinalGrades
                .Include(fg => fg.Student)
                .Where(fg => fg.CourseId == courseId)
                .Select(fg => new FinalGradeDto
                {
                    Id = fg.Id,
                    StudentId = fg.StudentId,
                    StudentName = fg.Student.FullName,
                    Grade = fg.Grade,
                    CreatedAt = fg.CreatedAt
                })
                .ToListAsync();

            return grades;
        }
    }
}




