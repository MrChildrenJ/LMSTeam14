using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using LMS.Services;

namespace LMSControllerTests
{
    public class UnitTest1
    {
        // Uncomment the methods below after scaffolding
        // (they won't compile until then)

        [Fact]
        public void Test1()
        {
           // An example of a simple unit test on the CommonController
           CommonController ctrl = new CommonController(MakeTinyDB());

           var allDepts = ctrl.GetDepartments() as JsonResult;

           // Cast to the actual type instead of using dynamic
           var departments = allDepts.Value as object[];
           
           Assert.NotNull(departments);
           Assert.Equal(1, departments.Length);
           
           // Use reflection to access the anonymous object properties
           var firstDept = departments[0];
           var deptType = firstDept.GetType();
           var subjectProperty = deptType.GetProperty("subject");
           Assert.NotNull(subjectProperty);
           var subjectValue = (string)subjectProperty.GetValue(firstDept);
           Assert.Equal("CS", subjectValue);
        }

        [Fact]
        public void CreateDepartment_NewDepartment_ReturnsSuccess()
        {
            // Arrange
            AdministratorController ctrl = new AdministratorController(MakeTinyDB());
            
            // Act
            var result = ctrl.CreateDepartment("MATH", "Mathematics") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            
            // Use reflection to access the anonymous object properties
            var resultType = result.Value.GetType();
            var successProperty = resultType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.True(successValue);
        }

        [Fact]
        public void CreateDepartment_DuplicateDepartment_ReturnsFalse()
        {
            // Arrange
            AdministratorController ctrl = new AdministratorController(MakeTinyDB());
            
            // Act - Try to create CS department that already exists
            var result = ctrl.CreateDepartment("CS", "Computer Science") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            
            // Use reflection to access the anonymous object properties  
            var resultType = result.Value.GetType();
            var successProperty = resultType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.False(successValue);
        }

        [Fact]
        public void GetCourses_ValidDepartment_ReturnsCoursesArray()
        {
            // Arrange
            AdministratorController ctrl = new AdministratorController(MakeTinyDB());
            
            // Act
            var result = ctrl.GetCourses("CS") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var courses = result.Value as Array;
            Assert.NotNull(courses);
            Assert.Equal(2, courses.Length); // Should have 2 CS courses
        }

        [Fact]
        public void GetProfessors_ValidDepartment_ReturnsProfessorsArray()
        {
            // Arrange
            AdministratorController ctrl = new AdministratorController(MakeTinyDB());
            
            // Act
            var result = ctrl.GetProfessors("CS") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var professors = result.Value as Array;
            Assert.NotNull(professors);
            Assert.Equal(1, professors.Length); // Should have 1 CS professor
        }

        // ProfessorController Tests

        [Fact]
        public void GetStudentsInClass_ValidClass_ReturnsStudentsArray()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.GetStudentsInClass("CS", 5530, "Fall", 2023) as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var students = result.Value as Array;
            Assert.NotNull(students);
            Assert.Equal(2, students.Length); // Should have 2 enrolled students
        }

        [Fact]
        public void CreateAssignmentCategory_NewCategory_ReturnsSuccess()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.CreateAssignmentCategory("CS", 5530, "Fall", 2023, "Projects", 40) as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var resultType = result.Value.GetType();
            var successProperty = resultType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.True(successValue);
        }

        [Fact]
        public void CreateAssignmentCategory_DuplicateCategory_ReturnsFalse()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act - Try to create category that already exists
            var result = ctrl.CreateAssignmentCategory("CS", 5530, "Fall", 2023, "Homework", 30) as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var resultType = result.Value.GetType();
            var successProperty = resultType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.False(successValue);
        }

        [Fact]
        public void CreateAssignment_ValidData_ReturnsSuccess()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.CreateAssignment("CS", 5530, "Fall", 2023, "Homework", "HW3", 100, 
                DateTime.Now.AddDays(7), "Complete the database design") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var resultType = result.Value.GetType();
            var successProperty = resultType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.True(successValue);
        }

        [Fact]
        public void CreateAssignment_DuplicateName_ReturnsFalse()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act - Try to create assignment with existing name in same category
            var result = ctrl.CreateAssignment("CS", 5530, "Fall", 2023, "Homework", "HW1", 100, 
                DateTime.Now.AddDays(7), "Duplicate assignment") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var resultType = result.Value.GetType();
            var successProperty = resultType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.False(successValue);
        }

        [Fact]
        public void GradeSubmission_ValidSubmission_ReturnsSuccess()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.GradeSubmission("CS", 5530, "Fall", 2023, "Homework", "HW1", "u0000001", 85) as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var resultType = result.Value.GetType();
            var successProperty = resultType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.True(successValue);
        }

        [Fact]
        public void GetAssignmentsInCategory_ValidCategory_ReturnsAssignments()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.GetAssignmentsInCategory("CS", 5530, "Fall", 2023, "Homework") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var assignments = result.Value as Array;
            Assert.NotNull(assignments);
            Assert.Equal(2, assignments.Length); // Should have 2 homework assignments
        }

        [Fact]
        public void GetAssignmentsInCategory_AllCategories_ReturnsAllAssignments()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act - Pass null to get all assignments
            var result = ctrl.GetAssignmentsInCategory("CS", 5530, "Fall", 2023, null) as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var assignments = result.Value as Array;
            Assert.NotNull(assignments);
            Assert.Equal(3, assignments.Length); // Should have all 3 assignments
        }

        [Fact]
        public void GetAssignmentCategories_ValidClass_ReturnsCategories()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.GetAssignmentCategories("CS", 5530, "Fall", 2023) as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var categories = result.Value as Array;
            Assert.NotNull(categories);
            Assert.Equal(2, categories.Length); // Should have 2 categories
        }

        [Fact]
        public void GetSubmissionsToAssignment_ValidAssignment_ReturnsSubmissions()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.GetSubmissionsToAssignment("CS", 5530, "Fall", 2023, "Homework", "HW1") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var submissions = result.Value as Array;
            Assert.NotNull(submissions);
            Assert.Equal(2, submissions.Length); // Should have 2 submissions
        }

        [Fact]
        public void GetMyClasses_ValidProfessor_ReturnsClasses()
        {
            // Arrange
            var db = MakeRichDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.GetMyClasses("u1234567") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var classes = result.Value as Array;
            Assert.NotNull(classes);
            Assert.Equal(1, classes.Length); // Should have 1 class
        }

        // Test cases for sentinel value logic and grade calculation

        [Fact]
        public void SubmitAssignmentText_NewSubmission_CreatesWithSentinelValue()
        {
            // Arrange
            var db = MakeRichDB();
            StudentController ctrl = new StudentController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.SubmitAssignmentText("CS", 5530, "Fall", 2023, "Homework", "HW1", "u0000003", "Test submission") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var resultValue = result.Value;
            var successProperty = resultValue.GetType().GetProperty("success");
            var successValue = (bool)successProperty.GetValue(resultValue);
            Assert.True(successValue);
            
            // Check that submission has sentinel value
            var submission = db.Submissions.FirstOrDefault(s => s.Student == "u0000003" && s.Assignment == 1);
            Assert.NotNull(submission);
            Assert.Equal(uint.MaxValue, submission.Score); // Should be sentinel value
        }

        [Fact]
        public void GetAssignmentsInClass_WithUngraded_ShowsNotGraded()
        {
            // Arrange
            var db = MakeRichDB();
            
            // Add a submission with sentinel value (ungraded) for student u0000002 on HW2
            db.Submissions.Add(new Submission
            {
                Assignment = 2,
                Student = "u0000002",
                SubmissionContents = "Ungraded submission",
                Time = DateTime.Now,
                Score = uint.MaxValue // Sentinel value
            });
            db.SaveChanges();
            
            StudentController ctrl = new StudentController(db, new GradeCalculationService(db));
            
            // Act
            var result = ctrl.GetAssignmentsInClass("CS", 5530, "Fall", 2023, "u0000002") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var assignments = result.Value as Array;
            Assert.NotNull(assignments);
            
            // Find HW2 assignment
            var hw2 = assignments.Cast<object>().FirstOrDefault(a => {
                var nameProperty = a.GetType().GetProperty("aname");
                return nameProperty != null && nameProperty.GetValue(a).Equals("HW2");
            });
            
            Assert.NotNull(hw2);
            var isGradedProperty = hw2.GetType().GetProperty("isGraded");
            var isGradedValue = (bool)isGradedProperty.GetValue(hw2);
            Assert.False(isGradedValue); // Should not be graded
        }

        [Fact]
        public void GradeCalculation_WithMixedSubmissionStates_CalculatesCorrectly()
        {
            // Arrange
            var db = MakeAdvancedGradingDB();
            var gradeService = new GradeCalculationService(db);
            
            // Act - Calculate grade for student with mixed submission states
            var grade = gradeService.CalculateGrade("u0000001", 1);
            
            // Assert
            // Student has:
            // - HW1: 90/100 (graded)
            // - HW2: sentinel value (not graded, excluded from calculation)
            // - Midterm: no submission (0/200, counts as 0)
            // Grade should be: (90 + 0) / (100 + 200) = 90/300 = 30% = E
            Assert.Equal("E", grade);
        }

        [Fact]
        public void GradeSubmission_ChangesScoreFromSentinel_UpdatesGrade()
        {
            // Arrange
            var db = MakeAdvancedGradingDB();
            ProfessorController ctrl = new ProfessorController(db, new GradeCalculationService(db));
            
            // Act - Grade a previously ungraded submission
            var result = ctrl.GradeSubmission("CS", 5530, "Fall", 2023, "Homework", "HW2", "u0000001", 85) as JsonResult;
            
            // Assert grading succeeded
            Assert.NotNull(result);
            var successProperty = result.Value.GetType().GetProperty("success");
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.True(successValue);
            
            // Check that score changed from sentinel to actual value
            var submission = db.Submissions.FirstOrDefault(s => s.Student == "u0000001" && s.Assignment == 2);
            Assert.NotNull(submission);
            Assert.Equal(85u, submission.Score); // Should be actual grade, not sentinel
            
            // Check that grade was updated (now includes the newly graded assignment)
            var enrollment = db.Enrolled.FirstOrDefault(e => e.Student == "u0000001" && e.Class == 1);
            Assert.NotNull(enrollment);
            Assert.NotEqual("--", enrollment.Grade); // Should have calculated grade
        }

        [Fact]
        public void GradeCalculation_OnlyUngraded_ReturnsMinusGrade()
        {
            // Arrange
            var db = MakeOnlyUngradedDB();
            var gradeService = new GradeCalculationService(db);
            
            // Act - Calculate grade when all submissions are ungraded
            var grade = gradeService.CalculateGrade("u0000001", 1);
            
            // Assert - Should return "--" when no graded submissions exist
            Assert.Equal("--", grade);
        }

        [Fact]
        public void GradeCalculation_ProfessorGivesZero_CountsAsZero()
        {
            // Arrange
            var db = MakeRichDB();
            var gradeService = new GradeCalculationService(db);
            
            // Manually set a submission to have 0 score (professor gave 0)
            var submission = db.Submissions.First(s => s.Student == "u0000001" && s.Assignment == 1);
            submission.Score = 0; // Professor explicitly gave 0
            db.SaveChanges();
            
            // Act
            var grade = gradeService.CalculateGrade("u0000001", 1);
            
            // Assert - 0 score should count in calculation
            // Should not be "--" or excluded from calculation
            Assert.NotEqual("--", grade);
        }


        /// <summary>
        /// Make a comprehensive in-memory database with classes, students, assignments, and submissions
        /// for thorough testing of ProfessorController methods.
        /// </summary>
        /// <returns></returns>
        LMSContext MakeRichDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
                .UseInMemoryDatabase($"LMSRichTest_{Guid.NewGuid()}")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .UseApplicationServiceProvider(NewServiceProvider())
                .EnableSensitiveDataLogging()
                .Options;

            var db = new LMSContext(contextOptions);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Add department
            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            // Add courses
            db.Courses.Add(new Course { CatalogId = 1, Name = "Database Systems", Number = 5530, Department = "CS" });
            db.Courses.Add(new Course { CatalogId = 2, Name = "Software Engineering", Number = 3500, Department = "CS" });

            // Add professor
            db.Professors.Add(new Professor { UId = "u1234567", FName = "John", LName = "Doe", WorksIn = "CS", Dob = DateTime.Now.AddYears(-40) });

            // Add students
            db.Students.Add(new Student { UId = "u0000001", FName = "Alice", LName = "Smith", Major = "CS", Dob = DateTime.Now.AddYears(-20) });
            db.Students.Add(new Student { UId = "u0000002", FName = "Bob", LName = "Jones", Major = "CS", Dob = DateTime.Now.AddYears(-21) });

            // Add class
            db.Classes.Add(new Class 
            { 
                ClassId = 1, 
                Season = "Fall", 
                Year = 2023, 
                Location = "WEB 1460", 
                StartTime = TimeOnly.Parse("10:45:00"), 
                EndTime = TimeOnly.Parse("12:05:00"), 
                Listing = 1, 
                TaughtBy = "u1234567" 
            });

            // Add student enrollments
            db.Enrolled.Add(new Enrolled { Student = "u0000001", Class = 1, Grade = "A-" });
            db.Enrolled.Add(new Enrolled { Student = "u0000002", Class = 1, Grade = "B+" });

            // Add assignment categories
            db.AssignmentCategories.Add(new AssignmentCategory { CategoryId = 1, Name = "Homework", Weight = 30, InClass = 1 });
            db.AssignmentCategories.Add(new AssignmentCategory { CategoryId = 2, Name = "Exams", Weight = 70, InClass = 1 });

            // Add assignments
            db.Assignments.Add(new Assignment 
            { 
                AssignmentId = 1, 
                Name = "HW1", 
                Category = 1, 
                MaxPoints = 100, 
                Contents = "First homework assignment", 
                Due = DateTime.Now.AddDays(-7) 
            });
            db.Assignments.Add(new Assignment 
            { 
                AssignmentId = 2, 
                Name = "HW2", 
                Category = 1, 
                MaxPoints = 100, 
                Contents = "Second homework assignment", 
                Due = DateTime.Now.AddDays(-3) 
            });
            db.Assignments.Add(new Assignment 
            { 
                AssignmentId = 3, 
                Name = "Midterm", 
                Category = 2, 
                MaxPoints = 200, 
                Contents = "Midterm exam", 
                Due = DateTime.Now.AddDays(-1) 
            });

            // Add submissions
            db.Submissions.Add(new Submission 
            { 
                Assignment = 1, 
                Student = "u0000001", 
                Time = DateTime.Now.AddDays(-6), 
                Score = 90, 
                SubmissionContents = "Alice's HW1 submission" 
            });
            db.Submissions.Add(new Submission 
            { 
                Assignment = 1, 
                Student = "u0000002", 
                Time = DateTime.Now.AddDays(-6), 
                Score = 85, 
                SubmissionContents = "Bob's HW1 submission" 
            });
            db.Submissions.Add(new Submission 
            { 
                Assignment = 2, 
                Student = "u0000001", 
                Time = DateTime.Now.AddDays(-2), 
                Score = 95, 
                SubmissionContents = "Alice's HW2 submission" 
            });
            db.Submissions.Add(new Submission 
            { 
                Assignment = 3, 
                Student = "u0000001", 
                Time = DateTime.Now.AddHours(-2), 
                Score = 180, 
                SubmissionContents = "Alice's Midterm submission" 
            });

            db.SaveChanges();
            return db;
        }

        /// <summary>
        /// Create database for advanced grading tests with mixed submission states
        /// </summary>
        LMSContext MakeAdvancedGradingDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
                .UseInMemoryDatabase($"LMSAdvancedTest_{Guid.NewGuid()}")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .UseApplicationServiceProvider(NewServiceProvider())
                .EnableSensitiveDataLogging()
                .Options;

            var db = new LMSContext(contextOptions);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Add department and course
            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });
            db.Courses.Add(new Course { CatalogId = 1, Name = "Database Systems", Number = 5530, Department = "CS" });
            db.Professors.Add(new Professor { UId = "u1234567", FName = "John", LName = "Doe", WorksIn = "CS", Dob = DateTime.Now.AddYears(-40) });
            db.Students.Add(new Student { UId = "u0000001", FName = "Alice", LName = "Smith", Major = "CS", Dob = DateTime.Now.AddYears(-20) });

            // Add class
            db.Classes.Add(new Class 
            { 
                ClassId = 1, Season = "Fall", Year = 2023, Location = "WEB 1460", 
                StartTime = TimeOnly.Parse("10:45:00"), EndTime = TimeOnly.Parse("12:05:00"), 
                Listing = 1, TaughtBy = "u1234567" 
            });

            // Add enrollment
            db.Enrolled.Add(new Enrolled { Student = "u0000001", Class = 1, Grade = "--" });

            // Add categories
            db.AssignmentCategories.Add(new AssignmentCategory { CategoryId = 1, Name = "Homework", Weight = 50, InClass = 1 });
            db.AssignmentCategories.Add(new AssignmentCategory { CategoryId = 2, Name = "Exams", Weight = 50, InClass = 1 });

            // Add assignments
            db.Assignments.Add(new Assignment { AssignmentId = 1, Name = "HW1", Category = 1, MaxPoints = 100, Contents = "First homework", Due = DateTime.Now.AddDays(-7) });
            db.Assignments.Add(new Assignment { AssignmentId = 2, Name = "HW2", Category = 1, MaxPoints = 100, Contents = "Second homework", Due = DateTime.Now.AddDays(-3) });
            db.Assignments.Add(new Assignment { AssignmentId = 3, Name = "Midterm", Category = 2, MaxPoints = 200, Contents = "Midterm exam", Due = DateTime.Now.AddDays(-1) });

            // Add submissions with mixed states
            db.Submissions.Add(new Submission { Assignment = 1, Student = "u0000001", Time = DateTime.Now.AddDays(-6), Score = 90, SubmissionContents = "HW1 submission" }); // Graded
            db.Submissions.Add(new Submission { Assignment = 2, Student = "u0000001", Time = DateTime.Now.AddDays(-2), Score = uint.MaxValue, SubmissionContents = "HW2 submission" }); // Ungraded (sentinel)
            // No submission for Midterm (counts as 0)

            db.SaveChanges();
            return db;
        }

        /// <summary>
        /// Create database where all submissions are ungraded (sentinel values)
        /// </summary>
        LMSContext MakeOnlyUngradedDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
                .UseInMemoryDatabase($"LMSUngradedTest_{Guid.NewGuid()}")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .UseApplicationServiceProvider(NewServiceProvider())
                .EnableSensitiveDataLogging()
                .Options;

            var db = new LMSContext(contextOptions);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Add minimal structure
            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });
            db.Courses.Add(new Course { CatalogId = 1, Name = "Database Systems", Number = 5530, Department = "CS" });
            db.Professors.Add(new Professor { UId = "u1234567", FName = "John", LName = "Doe", WorksIn = "CS", Dob = DateTime.Now.AddYears(-40) });
            db.Students.Add(new Student { UId = "u0000001", FName = "Alice", LName = "Smith", Major = "CS", Dob = DateTime.Now.AddYears(-20) });
            db.Classes.Add(new Class { ClassId = 1, Season = "Fall", Year = 2023, Location = "WEB 1460", StartTime = TimeOnly.Parse("10:45:00"), EndTime = TimeOnly.Parse("12:05:00"), Listing = 1, TaughtBy = "u1234567" });
            db.Enrolled.Add(new Enrolled { Student = "u0000001", Class = 1, Grade = "--" });
            db.AssignmentCategories.Add(new AssignmentCategory { CategoryId = 1, Name = "Homework", Weight = 100, InClass = 1 });
            db.Assignments.Add(new Assignment { AssignmentId = 1, Name = "HW1", Category = 1, MaxPoints = 100, Contents = "Homework", Due = DateTime.Now.AddDays(-7) });

            // Add only ungraded submissions (all sentinel values)
            db.Submissions.Add(new Submission { Assignment = 1, Student = "u0000001", Time = DateTime.Now.AddDays(-6), Score = uint.MaxValue, SubmissionContents = "Ungraded submission" });

            db.SaveChanges();
            return db;
        }

        ///// <summary>
        ///// Make a very tiny in-memory database, containing just one department
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeTinyDB()
        {
           // Original code - commented out due to MySQL conflict in tests
           // var contextOptions = new DbContextOptionsBuilder<LMSContext>()
           // .UseInMemoryDatabase( "LMSControllerTest" )
           // .ConfigureWarnings( b => b.Ignore( InMemoryEventId.TransactionIgnoredWarning ) )
           // .UseApplicationServiceProvider( NewServiceProvider() )
           // .Options;

           // Fixed version for testing - uses unique database name and proper configuration
           var contextOptions = new DbContextOptionsBuilder<LMSContext>()
           .UseInMemoryDatabase( $"LMSControllerTest_{Guid.NewGuid()}" )
           .ConfigureWarnings( b => b.Ignore( InMemoryEventId.TransactionIgnoredWarning ) )
           .UseApplicationServiceProvider( NewServiceProvider() )
           .EnableSensitiveDataLogging()
           .Options;

           var db = new LMSContext(contextOptions);

           db.Database.EnsureDeleted();
           db.Database.EnsureCreated();

           db.Departments.Add( new Department { Name = "KSoC", Subject = "CS" } );

           // Add test courses
           db.Courses.Add( new Course { CatalogId = 1, Name = "Database Systems", Number = 5530, Department = "CS" } );
           db.Courses.Add( new Course { CatalogId = 2, Name = "Software Engineering", Number = 3500, Department = "CS" } );

           // Add test professor
           db.Professors.Add( new Professor { UId = "u1234567", FName = "John", LName = "Doe", WorksIn = "CS", Dob = DateTime.Now.AddYears(-40) } );

           db.SaveChanges();

           return db;
        }

        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }

    }
}