using System.Text;

namespace Find_My_Boef.Model
{
    public class Employee
    {
        public int EmployeeNumber { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string Insertion { get; set; }
        public string LastName { get; set; }
        public Employee(string firstName, string insertion, string lastName)
        {
            FirstName = firstName;
            Insertion = insertion;
            LastName = lastName;
            FullName = GetFullName();
        }
        public string GetFullName()
        {
            StringBuilder sb = new();
            sb.Append(FirstName);

            if (Insertion != "")
            {
                sb.Append(string.Format(" {0} ", Insertion));
            }
            else
            {
                sb.Append(" ");
            }

            sb.Append(LastName);

            return sb.ToString();
        }
    }
}
