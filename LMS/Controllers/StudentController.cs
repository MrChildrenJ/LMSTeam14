using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        // Sentinel value to indicate "not graded" submissions
        private const uint NOT_GRADED_SENTINEL = uint.MaxValue;
        
        private LMSContext db;
        private readonly IGradeCalculationService _gradeCalculationService;
        
        public StudentController(LMSContext _db, IGradeCalculationService gradeCalculationService)
        {
            db = _db;
            _gradeCalculationService = gradeCalculationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classes = db.Enrolled
                .Where(e => e.Student == uid)
                .Select(e => new {
                    subject = e.ClassNavigation.ListingNavigation.DepartmentNavigation.Subject,
                    number = e.ClassNavigation.ListingNavigation.Number,
                    name = e.ClassNavigation.ListingNavigation.Name,
                    season = e.ClassNavigation.Season,
                    year = e.ClassNavigation.Year,
                    grade = string.IsNullOrEmpty(e.Grade) ? "--" : e.Grade
                })
                .ToArray();
            return Json(classes);
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var assignments = db.Assignments
                .Where(a => a.CategoryNavigation.InClassNavigation.ListingNavigation.DepartmentNavigation.Subject == subject
                    && a.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num
                    && a.CategoryNavigation.InClassNavigation.Season == season
                    && a.CategoryNavigation.InClassNavigation.Year == year)
                .Select(a => new {
                    aname = a.Name,
                    cname = a.CategoryNavigation.Name,
                    due = a.Due,
                    score = a.Submissions.Where(s => s.Student == uid).Select(s => (uint?)s.Score).FirstOrDefault(),
                    isGraded = a.Submissions.Where(s => s.Student == uid).Any(s => s.Score != NOT_GRADED_SENTINEL)
                })
                .ToArray();
            return Json(assignments);
        }

        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var assignment = db.Assignments
                .Include(a => a.CategoryNavigation)
                .ThenInclude(ac => ac.InClassNavigation)
                .FirstOrDefault(a =>
                    a.Name == asgname &&
                    a.CategoryNavigation.Name == category &&
                    a.CategoryNavigation.InClassNavigation.ListingNavigation.DepartmentNavigation.Subject == subject &&
                    a.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num &&
                    a.CategoryNavigation.InClassNavigation.Season == season &&
                    a.CategoryNavigation.InClassNavigation.Year == year);
            if (assignment == null)
                return Json(new { success = false });

            var submission = db.Submissions.FirstOrDefault(s => s.Assignment == assignment.AssignmentId && s.Student == uid);
            if (submission == null)
            {
                db.Submissions.Add(new Submission
                {
                    Assignment = assignment.AssignmentId,
                    Student = uid,
                    SubmissionContents = contents,
                    Time = DateTime.Now,
                    Score = NOT_GRADED_SENTINEL // Indicates "not graded yet"
                });
            }
            else
            {
                submission.SubmissionContents = contents;
                submission.Time = DateTime.Now;
                // Score remains unchanged
            }
            db.SaveChanges();
            
            // Update the student's grade after submission
            if (assignment.CategoryNavigation?.InClassNavigation != null)
            {
                _gradeCalculationService.UpdateGradeForStudent(uid, assignment.CategoryNavigation.InClass);
            }
            
            return Json(new { success = true });
        }

        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var classObj = db.Classes.FirstOrDefault(c =>
                c.ListingNavigation.DepartmentNavigation.Subject == subject &&
                c.ListingNavigation.Number == num &&
                c.Season == season &&
                c.Year == year);
            if (classObj == null)
                return Json(new { success = false });
            bool alreadyEnrolled = db.Enrolled.Any(e => e.Student == uid && e.Class == classObj.ClassId);
            if (alreadyEnrolled)
                return Json(new { success = false });
            db.Enrolled.Add(new Enrolled
            {
                Student = uid,
                Class = classObj.ClassId,
                Grade = null  // Will display as "--" when never calculated
            });
            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var grades = db.Enrolled.Where(e => e.Student == uid && !string.IsNullOrEmpty(e.Grade) && e.Grade != "--").Select(e => e.Grade).ToList();
            if (grades.Count == 0)
                return Json(new { gpa = 0.0 });
            var gradePoints = new Dictionary<string, double> {
                {"A", 4.0}, {"A-", 3.7}, {"B+", 3.3}, {"B", 3.0}, {"B-", 2.7}, {"C+", 2.3}, {"C", 2.0}, {"C-", 1.7}, {"D+", 1.3}, {"D", 1.0}, {"D-", 0.7}, {"E", 0.0}
            };
            double total = 0;
            int count = 0;
            foreach (var g in grades)
            {
                if (gradePoints.ContainsKey(g))
                {
                    total += gradePoints[g];
                    count++;
                }
            }
            double gpa = count > 0 ? total / count : 0.0;
            return Json(new { gpa });
        }
                
        /*******End code to modify********/

    }
}

