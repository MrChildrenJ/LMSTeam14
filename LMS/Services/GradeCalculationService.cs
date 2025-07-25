using LMS.Models.LMSModels;

namespace LMS.Services
{
    public interface IGradeCalculationService
    {
        string CalculateGrade(string studentUid, uint classId);
        void UpdateAllGradesForClass(uint classId);
        void UpdateGradeForStudent(string studentUid, uint classId);
    }

    public class GradeCalculationService : IGradeCalculationService
    {
        // Sentinel value to indicate "not graded" submissions
        private const uint NOT_GRADED_SENTINEL = uint.MaxValue;
        
        private readonly LMSContext _db;

        public GradeCalculationService(LMSContext db)
        {
            _db = db;
        }

        public string CalculateGrade(string studentUid, uint classId)
        {
            var categories = _db.AssignmentCategories.Where(ac => ac.InClass == classId).ToList();
            if (categories.Count == 0)
                return "-";
                
            double totalWeighted = 0;
            double totalWeights = 0;
            
            foreach (var cat in categories)
            {
                var assignments = _db.Assignments.Where(a => a.Category == cat.CategoryId).ToList();
                
                // Skip categories with no assignments (per requirements)
                if (assignments.Count == 0)
                    continue;
                    
                double earned = 0;
                double max = 0;
                
                foreach (var asg in assignments)
                {
                    max += asg.MaxPoints;
                    var submission = _db.Submissions.FirstOrDefault(s => s.Assignment == asg.AssignmentId && s.Student == studentUid);
                    
                    if (submission == null)
                    {
                        // No submission = 0 points (counts in calculation)
                        earned += 0;
                    }
                    else if (submission.Score == NOT_GRADED_SENTINEL)
                    {
                        // Submitted but not graded = don't count (per professor's clarification)
                        // Don't add to earned, don't subtract from max
                        max -= asg.MaxPoints; // Remove this assignment from total possible points
                    }
                    else
                    {
                        // Graded submission = count actual score (including 0 if professor gave 0)
                        earned += submission.Score;
                    }
                }
                
                double percent = max > 0 ? earned / max : 0;
                totalWeighted += percent * cat.Weight;
                totalWeights += cat.Weight;
            }
            
            if (totalWeights == 0)
                return "--";
                
            double scaling = 100.0 / totalWeights;
            double finalPercent = totalWeighted * scaling;
            
            return ConvertPercentToLetterGrade(finalPercent);
        }

        public void UpdateAllGradesForClass(uint classId)
        {
            var enrolledStudents = _db.Enrolled.Where(e => e.Class == classId).ToList();
            foreach (var enrollment in enrolledStudents)
            {
                if (enrollment == null)
                    continue;
                    
                var grade = CalculateGrade(enrollment.Student, classId);
                if (string.IsNullOrEmpty(grade))
                    grade = "--";
                    
                enrollment.Grade = grade.Length > 2 ? grade.Substring(0, 2) : grade;
                _db.Enrolled.Attach(enrollment);
                _db.Entry(enrollment).Property(e => e.Grade).IsModified = true;
            }
            
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update grades for class", ex);
            }
        }

        public void UpdateGradeForStudent(string studentUid, uint classId)
        {
            var enrollment = _db.Enrolled.FirstOrDefault(e => e.Student == studentUid && e.Class == classId);
            if (enrollment == null)
                return;
                
            var grade = CalculateGrade(studentUid, classId);
            if (string.IsNullOrEmpty(grade))
                grade = "--";
                
            enrollment.Grade = grade.Length > 2 ? grade.Substring(0, 2) : grade;
            _db.Enrolled.Attach(enrollment);
            _db.Entry(enrollment).Property(e => e.Grade).IsModified = true;
            
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update grade for student {studentUid}", ex);
            }
        }

        private static string ConvertPercentToLetterGrade(double percent)
        {
            return percent switch
            {
                >= 93 => "A",
                >= 90 => "A-",
                >= 87 => "B+",
                >= 83 => "B",
                >= 80 => "B-",
                >= 77 => "C+",
                >= 73 => "C",
                >= 70 => "C-",
                >= 67 => "D+",
                >= 63 => "D",
                >= 60 => "D-",
                _ => "E"
            };
        }
    }
}