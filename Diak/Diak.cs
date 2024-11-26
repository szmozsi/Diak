using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Diak
{
    public class Course
    {
        public string Name { get; set; }
        public string Instructor { get; set; }
        public int Grade { get; set; }
        public bool IsOptional { get; set; }
    }

    public class Student
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int Age { get; set; }
        public string Major { get; set; }
        public List<Course> Courses { get; set; } = new List<Course>();

        public event EventHandler<Course> CourseAdded;

        public void AddCourse(Course course)
        {
            Courses.Add(course);
            CourseAdded?.Invoke(this, course);
        }
    }

    public class StudentManager
    {
        public List<Student> Students { get; set; } = new List<Student>();

        public void ExportToXml(string filePath)
        {
            var studentsXml = new XElement("Students",
                Students.Select(student =>
                {
                    var studentXml = new XElement("Student",
                        new XAttribute("Id", student.Id),
                        new XElement("Name", student.Name),
                        new XElement("Age", student.Age));

                    if (student.Age >= 20)
                    {
                        studentXml.Add(new XElement("Major", student.Major));
                    }

                    var coursesXml = new XElement("Courses",
                        student.Courses.Where(course => !course.IsOptional || course.Grade > 50)
                        .Select(course =>
                            new XElement("Course",
                                new XElement("Name", course.Name),
                                new XElement("Instructor", course.Instructor),
                                new XElement("Grade", course.Grade),
                                new XElement("IsOptional", course.IsOptional))));

                    studentXml.Add(coursesXml);
                    return studentXml;
                }));

            studentsXml.Save(filePath);
            Console.WriteLine($"Data exported to {filePath}");
        }

        public double GetAveragePerformance(string major = null)
        {
            var courses = Students
                .Where(s => major == null || s.Major == major)
                .SelectMany(s => s.Courses);

            return courses.Any() ? courses.Average(c => c.Grade) : 0;
        }

        public List<Student> GetTopPerformers(int count)
        {
            return Students
                .OrderByDescending(s => s.Courses.Average(c => c.Grade))
                .Take(count)
                .ToList();
        }

        public List<Student> GetConsistentlyGoodStudents()
        {
            return Students
                .Where(s => s.Courses.Where(c => !c.IsOptional).All(c => c.Grade >= 60))
                .ToList();
        }
    }

    class Diak
    {
        static void Main(string[] args)
        {
            var manager = new StudentManager();

            var student1 = new Student
            {
                Name = "Anna",
                Age = 21,
                Major = "Informatika"
            };
            student1.CourseAdded += (sender, course) =>
            {
                var s = (Student)sender;
                Console.WriteLine($"New course added: {s.Name}, {course.Name}, {course.Instructor}");
            };
            student1.AddCourse(new Course { Name = "Adatbázisok", Instructor = "Dr. Nagy", Grade = 85, IsOptional = false });
            student1.AddCourse(new Course { Name = "Analízis", Instructor = "Dr. Kiss", Grade = 92, IsOptional = true });

            var student2 = new Student
            {
                Name = "Béla",
                Age = 19,
                Major = "Matematika"
            };
            student2.AddCourse(new Course { Name = "Adatbázisok", Instructor = "Dr. Nagy", Grade = 70, IsOptional = false });

            manager.Students.Add(student1);
            manager.Students.Add(student2);

            manager.ExportToXml("students.xml");

            Console.WriteLine($"Average performance (all): {manager.GetAveragePerformance()}");
            Console.WriteLine($"Average performance (Informatika): {manager.GetAveragePerformance("Informatika")}");

            var topPerformers = manager.GetTopPerformers(1);
            Console.WriteLine("Top performer:");
            foreach (var student in topPerformers)
            {
                Console.WriteLine(student.Name);
            }

            var goodStudents = manager.GetConsistentlyGoodStudents();
            Console.WriteLine("Consistently good students:");
            foreach (var student in goodStudents)
            {
                Console.WriteLine(student.Name);
            }
        }
    }
}
