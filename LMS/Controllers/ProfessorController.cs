using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
     [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {
        // Sentinel value to indicate "not graded" submissions
        private const uint NOT_GRADED_SENTINEL = uint.MaxValue;
        
        private readonly LMSContext db;
        private readonly IGradeCalculationService _gradeCalculationService;

        public ProfessorController(LMSContext _db, IGradeCalculationService gradeCalculationService)
        {
            db = _db;
            _gradeCalculationService = gradeCalculationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            uint courseNumber = (uint)num;
            uint classYear = (uint)year;
            var query = from cl in db.Classes
                        join e in db.Enrolled on cl.ClassId equals e.Class
                        join s in db.Students on e.Student equals s.UId
                        where cl.ListingNavigation.Department == subject &&
                              cl.ListingNavigation.Number == courseNumber &&
                              cl.Season == season &&
                              cl.Year == classYear
                        select new
                        {
                            fname = s.FName,
                            lname = s.LName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = e.Grade
                        };
            
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            uint courseNumber = (uint)num;
            uint classYear = (uint)year;

            var query = from a in db.Assignments
                        join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                        join c in db.Classes on ac.InClass equals c.ClassId
                        where c.ListingNavigation.Department == subject &&
                              c.ListingNavigation.Number == courseNumber &&
                              c.Season == season &&
                              c.Year == classYear &&
                              (category == null || ac.Name == category)
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,
                            submissions = a.Submissions.Count()
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            uint courseNumber = (uint)num;
            uint classYear = (uint)year;
            
            var query = from ac in db.AssignmentCategories
                        join c in db.Classes on ac.InClass equals c.ClassId
                        where c.ListingNavigation.Department == subject &&
                              c.ListingNavigation.Number == courseNumber &&
                              c.Season == season &&
                              c.Year == classYear
                        select new
                        {
                            name = ac.Name,
                            weight = ac.Weight
                        };
            
            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            uint courseNumber = (uint)num;
            uint classYear = (uint)year;
            uint categoryWeight = (uint)catweight;
            
            // Find the class using LINQ query style
            var classQuery = from c in db.Classes
                           where c.ListingNavigation.Department == subject &&
                                 c.ListingNavigation.Number == courseNumber &&
                                 c.Season == season &&
                                 c.Year == classYear
                           select c;
            
            var classObj = classQuery.FirstOrDefault();
            if (classObj == null)
            {
                return Json(new { success = false });
            }
            
            var existingCategoryQuery =
                from ac in db.AssignmentCategories
                join c in db.Classes on ac.InClass equals c.ClassId
                where c.ListingNavigation.Department == subject &&
                    c.ListingNavigation.Number == courseNumber &&
                    c.Season == season &&
                    c.Year == classYear &&
                    ac.Name == category
                select ac;
            
            if (existingCategoryQuery.Any())
            {
                return Json(new { success = false });
            }
            
            var newCategory = new AssignmentCategory
            {
                Name = category,
                Weight = categoryWeight,
                InClass = classObj.ClassId
            };
            
            db.AssignmentCategories.Add(newCategory);
            db.SaveChanges();
            
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            uint courseNumber = (uint)num;
            uint classYear = (uint)year;
            uint maxPoints = (uint)asgpoints;
            
            var categoryQuery =
                from ac in db.AssignmentCategories
                join c in db.Classes on ac.InClass equals c.ClassId
                where c.ListingNavigation.Department == subject &&
                    c.ListingNavigation.Number == courseNumber &&
                    c.Season == season &&
                    c.Year == classYear &&
                    ac.Name == category
                select ac;
            
            var assignmentCategory = categoryQuery.FirstOrDefault();
            if (assignmentCategory == null)
            {
                return Json(new { success = false });
            }
            
            var existingAssignmentQuery =
                from a in db.Assignments
                where a.Category == assignmentCategory.CategoryId &&
                      a.Name == asgname
                select a;
            
            if (existingAssignmentQuery.Any())
            {
                return Json(new { success = false });
            }
            
            var newAssignment = new Assignment
            {
                Name = asgname,
                Category = assignmentCategory.CategoryId,
                MaxPoints = maxPoints,
                Contents = asgcontents,
                Due = asgdue
            };
            
            db.Assignments.Add(newAssignment);
            db.SaveChanges();
            // Update grades for all students in this class
            _gradeCalculationService.UpdateAllGradesForClass(assignmentCategory.InClass);
            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            uint courseNumber = (uint)num;
            uint classYear = (uint)year;
            
            var query = from s in db.Submissions
                        join a in db.Assignments on s.Assignment equals a.AssignmentId
                        join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                        join c in db.Classes on ac.InClass equals c.ClassId
                        join st in db.Students on s.Student equals st.UId
                        where c.ListingNavigation.Department == subject &&
                              c.ListingNavigation.Number == courseNumber &&
                              c.Season == season &&
                              c.Year == classYear &&
                              ac.Name == category &&
                              a.Name == asgname
                        select new
                        {
                            fname = st.FName,
                            lname = st.LName,
                            uid = st.UId,
                            time = s.Time,
                            score = s.Score
                        };
            
            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            uint courseNumber = (uint)num;
            uint classYear = (uint)year;

            var assignmentQuery =
                from a in db.Assignments
                join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                join c in db.Classes on ac.InClass equals c.ClassId
                where c.ListingNavigation.Department == subject &&
                        c.ListingNavigation.Number == courseNumber &&
                        c.Season == season &&
                        c.Year == classYear &&
                        ac.Name == category &&
                        a.Name == asgname
                select a;
                
            var assignment = assignmentQuery.Include(a => a.CategoryNavigation).FirstOrDefault();
            if (assignment == null)
            {
                return Json(new { success = false, message = "Assignment not found" });
            }

            // Validate score range based on assignment's MaxPoints
            if (score < 0 || score > assignment.MaxPoints)
            {
                return Json(new { success = false, message = $"Score must be between 0 and {assignment.MaxPoints}" });
            }

            uint newScore = (uint)score;

            var submissionQuery =
                from s in db.Submissions
                where s.Assignment == assignment.AssignmentId &&
                        s.Student == uid
                select s;

            var submission = submissionQuery.FirstOrDefault();
            if (submission == null)
            {
                return Json(new { success = false });
            }

            submission.Score = newScore;
            db.SaveChanges();
            
            // Update grade for only this specific student
            if (assignment.CategoryNavigation != null)
            {
                _gradeCalculationService.UpdateGradeForStudent(uid, assignment.CategoryNavigation.InClass);
            }
            else
            {
                // Fallback: find class ID through assignment category
                var assignmentCategory = db.AssignmentCategories.FirstOrDefault(ac => ac.CategoryId == assignment.Category);
                if (assignmentCategory != null)
                {
                    _gradeCalculationService.UpdateGradeForStudent(uid, assignmentCategory.InClass);
                }
            }
            
            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Classes
                        where c.TaughtBy == uid
                        select new
                        {
                            subject = c.ListingNavigation.Department,
                            number = c.ListingNavigation.Number,
                            name = c.ListingNavigation.Name,
                            season = c.Season,
                            year = c.Year
                        };
            
            return Json(query.ToArray());
        }

        /*******End code to modify********/

        /// <summary>
        /// Test action: manually set a student's grade for a class and save to DB.
        /// Usage: /Professor/TestSetGrade?studentUid=U1234567&classId=1&grade=A
        /// </summary>
        [HttpPost]
        public IActionResult TestSetGrade(string studentUid, uint classId, string grade)
        {
            var enrollment = db.Enrolled.FirstOrDefault(e => e.Student == studentUid && e.Class == classId);
            if (enrollment == null)
                return Json(new { success = false, message = "Enrollment not found" });
            enrollment.Grade = grade;
            db.Enrolled.Attach(enrollment);
            db.Entry(enrollment).Property(e => e.Grade).IsModified = true;
            db.SaveChanges();
            return Json(new { success = true, grade = enrollment.Grade });
        }
    }
}

